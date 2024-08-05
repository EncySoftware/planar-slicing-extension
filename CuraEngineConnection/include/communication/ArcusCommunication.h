//  Copyright (c) 2022 Ultimaker B.V.
//  Copyright (c) 2022 Ultimaker B.V.
//  CuraEngine is released under the terms of the AGPLv3 or higher.

#ifndef ARCUSCOMMUNICATION_H
#define ARCUSCOMMUNICATION_H
#ifdef ARCUS

#include "Cura.pb.h" //To create Protobuf messages for CuraEngine.
#include <rapidjson/document.h> //Loading JSON documents to get settings from them.
#include <memory> //For unique_ptr and shared_ptr.
#include <spdlog/sinks/dup_filter_sink.h>
#include <spdlog/sinks/stdout_color_sinks.h>
#include "communication/Listener.h"
#include "utils/CEMesh.h"
namespace Arcus
{
class Socket;
}

namespace cura
{

/*
 * \brief Communication class that connects via libArcus to CuraEngine.
 */

class ArcusCommunication
{
public:

    ArcusCommunication();

    ~ArcusCommunication();

    std::string listen(
        const std::string& ip, 
        uint16_t port, 
        Listener* ArcusListener);

    void sendMessage(
        CuraEngineMeshReceiver* mesh, 
        IParamsReceiver* ParamsReceiver);

    void GetParams(
        proto::SettingList& SettingsList,
        IParamsReceiver* ParamsReceiver,
        bool IsGlobalSettings
    );

private:
    /*
     * \brief Put any mock-socket there to assist with Unit-Testing.
     */
    void setSocketMock(Arcus::Socket* socket);
    proto::SettingList GlobalSettings;
    /*
     * \brief PIMPL pattern subclass that contains the private implementation.
     */
    class Private;

    Listener* SocketListener;
    /*
     * \brief Pointer that contains the private implementation.
     */
    const std::unique_ptr<Private> private_data;

    std::map<std::string, std::string> MapSettings;
};

} // namespace cura

#endif // ARCUS
#endif // ARCUSCOMMUNICATION_H
