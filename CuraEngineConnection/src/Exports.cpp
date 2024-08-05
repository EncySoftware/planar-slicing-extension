#include "Exports.h"
#include "Application.h"
#include "CuraEngineControl.h"
#include <comutil.h>
extern "C" __declspec(dllexport) void* GetCuraEngineConnectionLibPointer()
{
    CEC = new CuraEngineControl();
    CEC.AddRef();
    CEC.AddRef();
    void* res = CEC.GetInterfacePtr();
    return (res);
}
extern "C" __declspec(dllexport) void FinalizeCuraEngineConnectionLib()
{
    CEC = NULL;
}