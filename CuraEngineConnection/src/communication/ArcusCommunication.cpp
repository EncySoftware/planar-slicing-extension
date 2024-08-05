// Copyright (c) 2023 UltiMaker
// CuraEngine is released under the terms of the AGPLv3 or higher

#ifdef ARCUS

#include "communication/ArcusCommunication.h"

#include "Application.h" //To get and set the current slice command.

#include "communication/ArcusCommunicationPrivate.h" //Our PIMPL.
#include "communication/Listener.h" //To listen to the Arcus socket.
#include <Arcus/Socket.h> //The socket to communicate to.

#include <spdlog/spdlog.h>

#include <cstring> //For strtok and strcopy.
#include <errno.h> // error number when trying to read file
#include <filesystem>
#include <fstream> //To check if files exist.
#include <numeric> //For std::accumulate.
#include <rapidjson/error/en.h> //Loading JSON documents to get settings from them.
#include <rapidjson/filereadstream.h>
#include <rapidjson/rapidjson.h>
#include "rapidjson/document.h"
#include <unordered_set>
#include <comdef.h>
#include "utils/CEMesh.h"
namespace cura
{
std::string BSTR_to_str(BSTR StrValue)
{
    std::wstring WideStr(StrValue);
    std::string Str(WideStr.begin(), WideStr.end());
    return Str;
}

ArcusCommunication::ArcusCommunication()
    : private_data(new Private)
{
}

ArcusCommunication::~ArcusCommunication()
{
    spdlog::info("Closing connection.");
//    private_data->socket->close();
    if (GlobalSettings.IsInitialized())
    {
        GlobalSettings.Clear();
    }
    SocketListener = nullptr;
    delete private_data->socket;
}

std::string ArcusCommunication::listen(
    const std::string& ip, 
    uint16_t port, 
    Listener* ArcusListener)
{
    SocketListener = ArcusListener;
    private_data->socket = new Arcus::Socket;
    private_data->socket->addListener(ArcusListener);

    private_data->socket->registerMessageType(&cura::proto::ObjectList::default_instance());
    private_data->socket->registerMessageType(&cura::proto::EnginePlugin::default_instance());
    private_data->socket->registerMessageType(&cura::proto::Slice::default_instance());
    private_data->socket->registerMessageType(&cura::proto::Extruder::default_instance());
    private_data->socket->registerMessageType(&cura::proto::Object::default_instance());
    private_data->socket->registerMessageType(&cura::proto::Progress::default_instance());
    private_data->socket->registerMessageType(&cura::proto::Layer::default_instance());
    private_data->socket->registerMessageType(&cura::proto::Polygon::default_instance());
    private_data->socket->registerMessageType(&cura::proto::LayerOptimized::default_instance());
    private_data->socket->registerMessageType(&cura::proto::PathSegment::default_instance());
    private_data->socket->registerMessageType(&cura::proto::GCodeLayer::default_instance());
    private_data->socket->registerMessageType(&cura::proto::PrintTimeMaterialEstimates::default_instance());
    private_data->socket->registerMessageType(&cura::proto::MaterialEstimates::default_instance());
    private_data->socket->registerMessageType(&cura::proto::SettingList::default_instance());
    private_data->socket->registerMessageType(&cura::proto::Setting::default_instance());
    private_data->socket->registerMessageType(&cura::proto::SettingExtruder::default_instance());
    private_data->socket->registerMessageType(&cura::proto::GCodePrefix::default_instance());
    private_data->socket->registerMessageType(&cura::proto::SliceUUID::default_instance());
    private_data->socket->registerMessageType(&cura::proto::SlicingFinished::default_instance());
    
 
    spdlog::info("Listening to {}:{}", ip, port);
    auto msg = "Listening to " + ip + ":" + std::to_string(port);
    SocketListener->CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR(msg.c_str()));
    private_data->socket->listen(ip, port);
    auto socket_state = private_data->socket->getState();
    int count = 0;
    while (socket_state != Arcus::SocketState::Listening)
    {
        std::this_thread::sleep_for(std::chrono::milliseconds(private_data->millisecUntilNextTry)); // Wait until we're listened. Check every XXXms.
        socket_state = private_data->socket->getState();
        if (socket_state == Arcus::SocketState::Error)
        {
            private_data->socket->reset();
            port++;  
            count++;
            if (count < 500)
            {
                spdlog::info("Listening to {}:{}", ip, port);
                msg = "Listening to " + ip + ":" + std::to_string(port);
                SocketListener->CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR(msg.c_str()));
                private_data->socket->listen(ip, port);
            }
            else
            {
                private_data->socket->close();
                return ("");
            }
        }
    }
    if (socket_state == Arcus::SocketState::Listening)
    {
        msg = "Listen to " + ip + ":" + std::to_string(port);
        SocketListener->CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR(msg.c_str()));
        spdlog::info("Listen to {}:{}", ip, port);
    }
    std::string IpPort = ip + ":" + std::to_string(port);
    return (IpPort);
}

void ArcusCommunication::GetParams(
    proto::SettingList& SettingsList, 
    IParamsReceiver* ParamsReceiver,
    bool IsGlobalSettings)
{
    BSTR name;
    BSTR value;
    long count = 0;

    if (IsGlobalSettings)
    {
       ParamsReceiver->GlobalParamsSize(&count);
       int countInt = static_cast<int>(count);
       for (int i = 0; i < countInt; i++)
       {
            ParamsReceiver->GlobalParamName(i, &name);
            ParamsReceiver->GlobalParamValue(i, &value);
            auto CurrentSetting = SettingsList.add_settings();
            CurrentSetting->set_name(BSTR_to_str(name));
            CurrentSetting->set_value(BSTR_to_str(value));
       }
    }
    else
    {
       ParamsReceiver->ExtruderParamsSize(&count);
       int countInt = static_cast<int>(count);
       for (int i = 0; i < countInt; i++)
       {
            ParamsReceiver->ExtruderParamName(i, &name);
            ParamsReceiver->ExtruderParamValue(i, &value);
            auto CurrentSetting = SettingsList.add_settings();
            CurrentSetting->set_name(BSTR_to_str(name));
            CurrentSetting->set_value(BSTR_to_str(value));
       }
    }
}

void ArcusCommunication::sendMessage(
    CuraEngineMeshReceiver* mesh, 
    IParamsReceiver* ParamsReceiver)
{
    auto msg = std::make_shared<proto::Slice>();

    auto MeshID = 0;
    auto modelCount = mesh->GetModelCount();
    auto modelMsg = "Model count - " + std::to_string(modelCount);
    SocketListener->CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR(modelMsg.c_str()));
    for (int i = 0; i < modelCount; i++)
    {           
        auto ObjectListMsg = msg->add_object_lists();
        auto MeshCount = mesh->GetMeshCount(i);
        auto meshMsg = "Model " + std::to_string(i+1) + ": Mesh count - " + std::to_string(MeshCount);
        SocketListener->CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR(meshMsg.c_str()));
        for (int j = 0; j < MeshCount; j++)
        {
            MeshID++;
            auto MeshName = mesh->GetMeshName(i, j);
            auto verticesStr = mesh->GetVertices(i, j);  
            auto verticesCount = mesh->GetVerticesCount(i, j);
            auto vertMsg = "Mesh " + std::to_string(j + 1) + ": Triangles - " + std::to_string(verticesCount/3) + ", Vertices - " + std::to_string(verticesCount);
            SocketListener->CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR(vertMsg.c_str()));
            auto objInfo = ObjectListMsg->add_objects();
            objInfo->set_id(MeshID);
            objInfo->set_vertices(verticesStr);
            objInfo->set_name(MeshName);
            auto objSetting = objInfo->add_settings();
            objSetting->set_name("extruder_nr");
            objSetting->set_value("0");
        }
    }

    GlobalSettings = msg->global_settings();
    GetParams(GlobalSettings, ParamsReceiver, true);

    msg->set_allocated_global_settings(&GlobalSettings);

    auto Extr = msg->add_extruders();
    Extr->set_id(0);
    auto ExtSettings = Extr->settings();
    GetParams(ExtSettings, ParamsReceiver, false);

    auto LimitToExtruder = msg->add_limit_to_extruder();
    LimitToExtruder->set_name("extruder0");
    LimitToExtruder->set_extruder(0);
    private_data->socket->sendMessage(msg);
}
    // On the one hand, don't expose the socket for normal use, but on the other, we need to mock it for unit-tests.
void ArcusCommunication::setSocketMock(Arcus::Socket* socket)
{
    private_data->socket = socket;
}


} // namespace cura

#endif // ARCUS