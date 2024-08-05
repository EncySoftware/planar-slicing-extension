#ifndef CURAENGINECONTROL_H
#define CURAENGINECONTROL_H

#include <cassert>
#include <cstddef>
#include <string>
#include "utils/CuraConnectionInterface.h"
#include  <initguid.h>
#include <Application.h>
#include "communication/Listener.h"
#include <comutil.h>
#include "communication/ArcusCommunication.h"
#include "utils/CEMesh.h"

_COM_SMARTPTR_TYPEDEF(ICuraConnectionLibrary, __uuidof(ICuraConnectionLibrary));

class CuraEngineControl : public ICuraConnectionLibrary
{
private:
    ULONG m_cRef = 0;
    ITrianglesRecieverPtr meshRecivI;
    CuraEngineMeshReceiver* meshReciv = nullptr;

public:
    cura::Application* App = nullptr;

    HRESULT __stdcall get_TrianglesReciever(
        /* [retval][out] */ ITrianglesReciever** Value) override
    { 
        if (meshReciv == nullptr)
        {
            meshReciv = new CuraEngineMeshReceiver();
            meshRecivI = meshReciv;
        }
        meshRecivI.AddRef();
        meshRecivI.AddRef();
        *Value = meshRecivI.GetInterfacePtr();
        return 0;
    }

    HRESULT __stdcall Slice(
        /* [in] */ ICuraEngineControlProcess* CuraEngineControlProcess,
        /* [in] */ IParamsReceiver* ParamsReceiver,
        /* [in] */ BSTR CuraPath,
        /* [retval][out] */ VARIANT_BOOL *result) override
    {
        return (App->Slice(CuraEngineControlProcess, ParamsReceiver, meshReciv, CuraPath, result));
    }
    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppvObject) override
    {
        if (! ppvObject)
            return E_INVALIDARG;
        *ppvObject = NULL;


        if (riid == __uuidof(ICuraConnectionLibrary) || riid == IID_IUnknown)
        {
            *ppvObject = (LPVOID)this;
            return NOERROR;
        }
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override
    {
        InterlockedIncrement(&m_cRef);
        return m_cRef;
    }
    ULONG STDMETHODCALLTYPE Release() override
    {
        // Decrement the object's internal counter.
        ULONG ulRefCount = InterlockedDecrement(&m_cRef);
        if (0 == m_cRef)
        {
            delete this;
        }
        return ulRefCount;
    }
    CuraEngineControl()
    {
        AddRef();
        App = &cura::Application::getInstance();
    }

    ~CuraEngineControl()
    {

    }
};
ICuraConnectionLibraryPtr CEC = nullptr;
#endif // APPLICATION_H