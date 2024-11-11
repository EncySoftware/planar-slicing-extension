using CAMAPI.DotnetHelper;
using CAMAPI.EventHandler;
using CAMAPI.ModelFormerTypes;
using CAMAPI.ResultStatus;
using CAMAPI.TechOperation;

namespace CuraEngineOperation;

public class OperationInitModelFormers : ICamApiEventHandler,
    ICamApiHandlerTechOperationInitModelFormers
{
    private class ModelFormerMakeSupportedItems : ICamApiModelFormerMakeSupportedItems
    {
        public void MakeSupportedItems(ICamApiModelFormerSupportedItems items)
        {
            using var itemsCom = new ComWrapper<ICamApiModelFormerSupportedItems>(items);
            itemsCom.Instance?.AddItem("Job Surfaces", 
                InterfaceInfo.IID<ICamApiMeshesArrayModelItem>(),
                "", "Job Surfaces", "", "", false, null);
        }
    }
    
    /// <summary>
    /// We always return false, because only one event is supported
    /// </summary>
    public bool GetAsyncMode(string interfaceUid)
    {
        return false;
    }
    
    /// <summary>
    /// Initialize model items, which can be created in job assignment
    /// </summary>
    public void InitModelFormers(ICamApiModelFormer modelFormersObj)
    {
        using var modelFormersCom = new ComWrapper<ICamApiModelFormer>(modelFormersObj);
        var modelFormers = modelFormersCom.Instance;
        
        if (modelFormers is not { SupportedItems: null })
            return;
        modelFormers.MakeSupportedItems(new ModelFormerMakeSupportedItems(), out var resultStatus);
        if (resultStatus.Code == TResultStatusCode.rsError)
            throw new Exception(resultStatus.Description);
    }
}