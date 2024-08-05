#include "Exports.h"
#include "Application.h"
#include "utils/CuraConnectionInterface.h"

namespace cura
{
__declspec(dllexport) void* __cdecl GetCuraEngineConnectionLibPointer()
{
    auto App = cura::Application::getInstance();
    ICuraConnectionLibrary IApp = App;
    return (IApp);
}
} // namespace cura