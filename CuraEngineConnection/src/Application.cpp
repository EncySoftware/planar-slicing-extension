// Copyright (c) 2023 UltiMaker
// CuraEngine is released under the terms of the AGPLv3 or higher

#include "Application.h"
#include "communication/ArcusCommunication.h" //To connect via Arcus to the back-end.
#include <memory>
#include <boost/uuid/random_generator.hpp> //For generating a UUID.
#include <boost/uuid/uuid_io.hpp> //For generating a UUID.

#include <spdlog/cfg/helpers.h>
#include <spdlog/sinks/dup_filter_sink.h>
#include <spdlog/sinks/stdout_color_sinks.h>
#include <spdlog/spdlog.h>
#include <Arcus/Socket.h>
#include <Arcus/Types.h>
#include <windows.h>
#include "utils/CEMesh.h"
#include <errhandlingapi.h>
#include <spdlog/sinks/basic_file_sink.h>
namespace cura
{

HANDLE RunCuraEngine(std::string& IPPortConnection, BSTR CuraPath)
{
    STARTUPINFO cif;
    ZeroMemory(&cif, sizeof(STARTUPINFO));
    
    //comment out for debugging
    cif.dwFlags = STARTF_USESHOWWINDOW;  
    cif.wShowWindow = SW_HIDE;

    PROCESS_INFORMATION pi;
    std::wstring CuraPathWStr(CuraPath, SysStringLen(CuraPath));
    std::string CuraPathStr(CuraPathWStr.begin(), CuraPathWStr.end());
    std::string AppName = CuraPathStr + "CuraEngine.exe ";
    IPPortConnection = "\""+ AppName + "\" connect " + IPPortConnection;
    LPSTR commandLine = const_cast<char*>(IPPortConnection.c_str());
    if (CreateProcess(AppName.c_str(), commandLine, NULL, NULL, FALSE, NULL, NULL, NULL, &cif, &pi) == TRUE)
    {
        return (pi.hProcess);
    }
    else
    {
        return (NULL);
    }

}

Application::Application()
    : instance_uuid(boost::uuids::to_string(boost::uuids::random_generator()()))
{
    auto dup_sink = std::make_shared<spdlog::sinks::dup_filter_sink_mt>(std::chrono::seconds{ 10 });
    auto base_sink = std::make_shared<spdlog::sinks::stdout_color_sink_mt>();
    dup_sink->add_sink(base_sink);

   // auto sharedFileSink = std::make_shared<spdlog::sinks::basic_file_sink_mt>("D:\\Cura\\curaengineconnection_out.log");   
   // dup_sink->add_sink(sharedFileSink);

    spdlog::default_logger()->sinks()
        = std::vector<std::shared_ptr<spdlog::sinks::sink>>{ dup_sink }; // replace default_logger sinks with the duplicating filtering sink to avoid spamming

    if (auto spdlog_val = spdlog::details::os::getenv("CURAENGINE_LOG_LEVEL"); ! spdlog_val.empty())
    {
        spdlog::cfg::helpers::load_levels(spdlog_val);
    };
}

Application::~Application()
{
   communication = nullptr;
}

Application& Application::getInstance()
{
    static Application instance; // Constructs using the default constructor.
    return instance;
}
BSTR DWORDToBSTR(DWORD dwValue)
{
    wchar_t buffer[16];
    swprintf(buffer, sizeof(buffer) / sizeof(buffer[0]), L"%lu", dwValue);
    BSTR bstr = SysAllocString(buffer);
    return bstr;
}
#ifdef ARCUS

#endif // ARCUS
HRESULT Application::Slice(
    ICuraEngineControlProcess* CuraEngineControlProcess, 
    IParamsReceiver* ParamsReceiver,
    CuraEngineMeshReceiver* mesh, 
    BSTR CuraPath,
    VARIANT_BOOL* result)
{
    std::string ip = "127.0.0.1";
    int port = 49674;

    cura::Listener* ArcusListener = new cura::Listener();
    ArcusListener->CuraEngineControlProcess = CuraEngineControlProcess;

    int socketState = 0;
    ArcusListener->SlicedFinished = &socketState;
    ArcusCommunication* arcus_communication = new ArcusCommunication();
    std::string IPPort = arcus_communication->listen(ip, port, ArcusListener);

    if (socketState != static_cast<int>(Arcus::SocketState::Listening))
    {
        CuraEngineControlProcess->OnLogMessage(5, _com_util::ConvertStringToBSTR("Cannot connect to Arcus socket"));
        spdlog::error("Cannot connect to Arcus socket");
        return (0);
    }

    std::string msg = "IPPort: " + IPPort;
    CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR(msg.c_str()));
    CuraEngineControlProcess->OnLogMessage(2, CuraPath);
    HANDLE CuraEngineHandle = RunCuraEngine(IPPort, CuraPath);
    DWORD exitCode;
    if (CuraEngineHandle == NULL || ! GetExitCodeProcess(CuraEngineHandle, &exitCode) && exitCode == STILL_ACTIVE)
    {
        CuraEngineControlProcess->OnLogMessage(5, _com_util::ConvertStringToBSTR("CuraEngine.exe couldn't run. Error:"));
        CuraEngineControlProcess->OnLogMessage(5, DWORDToBSTR(GetLastError()));
        spdlog::error("CuraEngine.exe couldn't run");
        if (CuraEngineHandle != NULL)
        {
            TerminateProcess(CuraEngineHandle, NO_ERROR); 
        }
        return (0);
    }
    CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR("CuraEngine.exe is running"));
    communication = arcus_communication;
//    arcus_communication->sendTestMessage();
    CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR("Sending message to CuraEngine"));

    arcus_communication->sendMessage(mesh, ParamsReceiver);
    if (socketState == -1)
    { 
        CuraEngineControlProcess->OnLogMessage(5, _com_util::ConvertStringToBSTR("Cannot send message to CuraEngine"));
        spdlog::error("Cannot send message to CuraEngine");
        return (0);
    }
    CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR("Message sent to CuraEngine"));
    auto lastCheckTime = std::chrono::steady_clock::now();
    bool isCuraEngineCloseManually = false;
    while (true)
    {
        auto currentTime = std::chrono::steady_clock::now();
        auto elapsedTime = std::chrono::duration_cast<std::chrono::seconds>(currentTime - lastCheckTime).count();
        if (elapsedTime >= 1)
        {
            if (!GetExitCodeProcess(CuraEngineHandle, &exitCode) || exitCode != STILL_ACTIVE)
            {
                CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR("Close CuraEngine handle because the CuraEngine process has ended"));
                CloseHandle(CuraEngineHandle);
                isCuraEngineCloseManually = true;
                break;
            }
            lastCheckTime = currentTime;
        }

        auto check = true;
        if (socketState == -1)
        {
            CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR("Arcus socket is closed"));
            break;
        }
        VARIANT_BOOL isCancelledProcess;
        CuraEngineControlProcess->IsProcessCancelled(&isCancelledProcess);
        if (isCancelledProcess == VARIANT_TRUE)
        {
            if (socketState != -1)
            {
                ArcusListener->getSocket()->close();
               // arcus_communication->SocketListener->getSocket()->close();
            }
            CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR("Process was canceled by user"));
            CloseHandle(CuraEngineHandle);
            isCuraEngineCloseManually = true;
            break;
        }
    }
    if (!isCuraEngineCloseManually)
    {
        CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR("Try to terminate CuraEngine process:"));
        if (CuraEngineHandle != NULL && (GetExitCodeProcess(CuraEngineHandle, &exitCode) && exitCode == STILL_ACTIVE))
        {
            CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR("CuraEngine process was successfully terminated"));
            TerminateProcess(CuraEngineHandle, NO_ERROR);
        }
        else
        {
            CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR("CuraEngine process was previously terminated automatically"));
        }
    }

    std::this_thread::sleep_for(std::chrono::milliseconds(100));
    auto socketStateMsg = "Socket state: " + std::to_string(socketState);
    CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR(socketStateMsg.c_str()));
    CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR("Calculate in CuraEngine is finished"));
    //if (! communication)
    //{
    //    exit(0);
    //}
    return (0);
}
} // namespace cura
