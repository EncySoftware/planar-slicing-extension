// Copyright (c) 2023 UltiMaker
// CuraEngine is released under the terms of the AGPLv3 or higher

#ifndef APPLICATION_H
#define APPLICATION_H

#include "utils/NoCopy.h"
#include <cassert>
#include <cstddef>
#include <string>
#include <comdef.h>
#include <initguid.h>
#include "communication/ArcusCommunication.h" //To connect via Arcus to the back-end.
#include "utils/CEMesh.h"

namespace cura
{

class ArcusCommunication;

struct PluginSetupConfiguration;

class Application : NoCopy
{
public:
    ArcusCommunication* communication = nullptr;

    static Application& getInstance();
   
    HRESULT Slice(
        ICuraEngineControlProcess* CuraEngineControlProcess, 
        IParamsReceiver* ParamsReceiver,
        CuraEngineMeshReceiver* mesh,
        BSTR CuraPath,
        VARIANT_BOOL* result);

    std::string instance_uuid;

private:

    Application();

    ~Application();
};

} // namespace cura

#endif // APPLICATION_H