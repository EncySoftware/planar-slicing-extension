#ifndef CEMESH_H
#define CEMESH_H

#include <string>
#include "utils/CuraConnectionInterface.h"
#include <comutil.h>
#include <comdef.h>
#include <vector>
_COM_SMARTPTR_TYPEDEF(ITrianglesReciever, __uuidof(ITrianglesReciever));

class CuraEngineMesh
{
public:
    std::vector <TCE3SPoint> Points;
    std::string MeshName;
};

class CuraEngineModel
{
public:
    std::vector <CuraEngineMesh*> Meshes;
};
class CuraEngineMeshReceiver : public ITrianglesReciever
{
private:
    std::vector<CuraEngineModel*> Models;
    ULONG m_cRef = 0;
    int pointIndex = -1;
    CuraEngineMesh* currentMesh;

public:
    int GetModelCount()
    {
        return Models.size();
    }
    int GetMeshCount(int modelIndex)
    { 
        return Models[modelIndex]->Meshes.size();
    }
    std::string GetVertices(int modelIndex, int meshIndex)
    {
        auto points = Models[modelIndex]->Meshes[meshIndex]->Points;
        char* vertices = reinterpret_cast<char*>(&points[0]);
        std::string verticesStr = "";
        verticesStr.assign(vertices, (points.size()) * 3 * 4);
        return verticesStr;
    }
    int GetVerticesCount(int modelIndex, int meshIndex)
    {
        auto points = Models[modelIndex]->Meshes[meshIndex]->Points;
        return points.size()-1;
    }
    std::string GetMeshName(int modelIndex, int meshIndex)
    {
        return Models[modelIndex]->Meshes[meshIndex]->MeshName;
    }

    HRESULT __stdcall BeginTransfer(void) override
    {
        return 0;
    }

    HRESULT __stdcall EndTransfer(void) override
    {
        return 0;
    }

    HRESULT __stdcall BeginModel(void) override
    {
        auto model = new CuraEngineModel();
        Models.push_back(model);
        return 0;
    }

    HRESULT __stdcall EndModel(void) override
    {
        return 0;
    }

    HRESULT __stdcall BeginMesh(
        /* [in] */ BSTR meshName,
        /* [in] */ long triangleCount) override
    {
        currentMesh = new CuraEngineMesh();
        auto model = Models.back();
        model->Meshes.push_back(currentMesh);
        currentMesh->Points.resize(3 * triangleCount + 1);

        std::wstring name(meshName);
        std::string nameStr(name.begin(), name.end());
        currentMesh->MeshName = nameStr;

        pointIndex = -1;
        return 0;
    }

    HRESULT __stdcall AddTriangle(
        /* [in] */ TCE3SPoint p1,
        /* [in] */ TCE3SPoint p2,
        /* [in] */ TCE3SPoint p3) override
    {
        pointIndex++;
        //currentMesh->Points.push_back(&p1);
        //currentMesh->Points.push_back(&p2);
        //currentMesh->Points.push_back(&p3);
        auto size = currentMesh->Points.size();
        currentMesh->Points[pointIndex] = p1;
        pointIndex++;
        currentMesh->Points[pointIndex] = p2;
        pointIndex++;
        currentMesh->Points[pointIndex] = p3;
        return 0;
    }
    HRESULT __stdcall EndMesh(void) override
    {
        return 0;
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppvObject) override
    {
        if (! ppvObject)
            return E_INVALIDARG;
        *ppvObject = NULL;


        if (riid == __uuidof(ITrianglesReciever) || riid == IID_IUnknown)
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
    CuraEngineMeshReceiver()
    {
        AddRef();
        AddRef();
    }
};
#endif