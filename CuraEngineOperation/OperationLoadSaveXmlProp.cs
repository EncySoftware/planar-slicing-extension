using CAMAPI.DotnetHelper;
using CAMAPI.EventHandler;
using CAMAPI.TechOperation;
using STXMLPropTypes;

namespace CuraEngineOperation;

public class OperationLoadSaveXmlProp : ICamApiEventHandler,
    ICamApiHandlerTechOperationLoadFromXmlProp,
    ICamApiHandlerTechOperationSaveToXmlProp
{
    private readonly ParamsReceiver _curaParamsReceiver;
    private readonly OperationProps _operationProps;
    private readonly CuraLibraryPath _curaLibraryPath;
    
    public OperationLoadSaveXmlProp(ParamsReceiver curaParamsReceiver, OperationProps operationProps, CuraLibraryPath curaLibraryPath) 
    {
        _curaParamsReceiver = curaParamsReceiver;
        _operationProps = operationProps;
        _curaLibraryPath = curaLibraryPath;
    }
    
    /// <summary>
    /// We always return false, because only one event is supported
    /// </summary>
    public bool GetAsyncMode(string interfaceUid)
    {
        return false;
    }
    
    public void LoadFromXmlProp(IST_XMLPropPointer xmlProp)
    {
        if (_curaParamsReceiver == null)
            throw new Exception("_curaParamsReceiver is null");
        if (_operationProps == null)
            throw new Exception("_operationProps is null");
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
        
        using var xmlPropCom = new ComWrapper<IST_XMLPropPointer>(xmlProp);
        var xmlPropObj = xmlPropCom.Instance;
        if (xmlPropObj == null)
            return;
        
        using var xmlPropCuraPathSetPathCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["CuraPath.SetPath"]);
        _curaLibraryPath.UserPathEnabled = xmlPropCuraPathSetPathCom.Instance?.ValueAsBoolean ?? false;
        
        using var xmlPropCuraPathPathCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["CuraPath.Path"]);
        _curaLibraryPath.UserCuraPath = xmlPropCuraPathPathCom.Instance?.ValueAsString ?? "";
        
        using var xmlPropCuraUserParameterArrayCom = new ComWrapper<IST_XMLPropArray>(xmlPropObj.Arr["CuraUserParameterArray"]);
        var curaUserParameterArray = xmlPropCuraUserParameterArrayCom.Instance;
        if (curaUserParameterArray != null)
        {
            _curaParamsReceiver.CEParameters.UserParameters.Clear();
            LoadAdhesionValueFromXml(xmlPropObj);
            for (var i = 0; i < curaUserParameterArray.TopItem + 1; i++)
            {
                using var paramCom = new ComWrapper<IST_XMLPropPointer>(curaUserParameterArray[i]);
                var name = paramCom.Instance?.Str["Name"] ?? "";
                var value = paramCom.Instance?.Str["Value"] ?? "";
                _curaParamsReceiver.CEParameters.UserParameters[name] = value;
            }        
        }
        else    
            LoadAdhesionValueFromXml(xmlPropObj);

        using var manufacturerCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.Manufacturer"]);
        var manufacturer = manufacturerCom.Instance; 
        if (manufacturer is { ValueAsString: not null } && manufacturer.ValueAsString != "")
            _curaParamsReceiver.CEParameters.SetSelectedMachineBrand(manufacturer.ValueAsString);
  
        using var machineCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.Machine"]);
        var machine = machineCom.Instance;
        if (machine is { ValueAsString: not null } && machine.ValueAsString != "")
            _curaParamsReceiver.CEParameters.SetSelectedMachine(machine.ValueAsString);
  
        using var extruderCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.Extruder"]);
        var extruder = extruderCom.Instance;
        if (extruder is { ValueAsString: not null } && extruder.ValueAsString != "")
            _curaParamsReceiver.CEParameters.SetSelectedExtruder(extruder.ValueAsString);
  
        using var materialBrandCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.MaterialBrand"]);
        var materialBrand = materialBrandCom.Instance;
        if (materialBrand is { ValueAsString: not null } && materialBrand.ValueAsString != "")
            _curaParamsReceiver.CEParameters.SetSelectedMaterialBrand(materialBrand.ValueAsString);
  
        using var materialCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.Material"]);
        var material = materialCom.Instance;
        if (material is { ValueAsString: not null } && material.ValueAsString != "")
            _curaParamsReceiver.CEParameters.SetSelectedMaterial(material.ValueAsString);
  
        using var variantCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.Variant"]);
        var variant = variantCom.Instance;
        if (variant is { ValueAsString: not null } && variant.ValueAsString != "")
            _curaParamsReceiver.CEParameters.SetSelectedVariant(variant.ValueAsString);

        using var profileCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.Profile"]);
        var profile = profileCom.Instance;
        if (profile is { ValueAsString: not null } && profile.ValueAsString != "")
            _curaParamsReceiver.CEParameters.SetSelectedIntentCategory(profile.ValueAsString);
  
        using var qualityCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.Quality"]);
        var quality = qualityCom.Instance;
        if (quality is { ValueAsString: not null } && quality.ValueAsString != "")
            _curaParamsReceiver.CEParameters.SetSelectedQuality(quality.ValueAsString);
  
        using var showCustomParametersCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.ShowCustomParameters"]);
        var showCustomParameters = showCustomParametersCom.Instance;
        if (showCustomParameters != null)
            _curaParamsReceiver.CEParameters.IsShowCustomParameters = showCustomParameters.ValueAsBoolean;
  
        using var settingVisibilityCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.SettingVisibility"]);
        var settingVisibility = settingVisibilityCom.Instance;
        if (settingVisibility is { ValueAsString: not null } && settingVisibility.ValueAsString != "")
            _curaParamsReceiver.CEParameters.SelectedSettingVisibilities = settingVisibility.ValueAsString;

        using var autoToolParameterizationCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.AutoToolParameterization"]);
        var autoToolParameterization = autoToolParameterizationCom.Instance;
        if (autoToolParameterization != null)
            _operationProps.IsAutoToolParameterization = autoToolParameterization.ValueAsBoolean;
  
        using var tpModeCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.ToolpathParsingMode"]);
        var tpMode = tpModeCom.Instance;
        if (tpMode != null)
            _operationProps.Tpm = tpMode.ValueAsString=="Simplified" ? ToolpathParsingMode.tpmSimplified : ToolpathParsingMode.tpmGCodeBased;
  
        using var outputAdditionalParametersCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.OutputAdditionalParameters"]);
        var outputAdditionalParameters = outputAdditionalParametersCom.Instance;
        if (outputAdditionalParameters != null)
            _operationProps.IsOutputAdditionalClDataParameters = outputAdditionalParameters.ValueAsBoolean;

        using var outputFilamentExtrudingCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.OutputFilamentExtruding"]);
        var outputFilamentExtruding = outputFilamentExtrudingCom.Instance;
        if (outputFilamentExtruding != null)
            _operationProps.IsOutputFilamentExtruding = outputFilamentExtruding.ValueAsBoolean;
  
        using var felCom = new ComWrapper<IST_XMLPropPointer>(xmlPropObj.Ptr["GeneralParameters.FilamentExtrudingLength"]);
        var fel = felCom.Instance;
        if (fel != null)
            _operationProps.FilamentExtrudingLength = fel.ValueAsDouble;
  
        _curaParamsReceiver.CEParameters.UpdateAllParameters();
    }
    
    private void LoadAdhesionValueFromXml(IST_XMLPropPointer xmlProp)
    {
        if (_curaParamsReceiver == null)
            throw new Exception("_curaParamsReceiver is null");
        using var adhesionCom = new ComWrapper<IST_XMLPropPointer>(xmlProp.Ptr["GeneralParameters.Adhesion"]);
        var adhesion = adhesionCom.Instance;
        if (adhesion == null)
            return;
        
        var value = adhesion.ValueAsBoolean;
        if (value)
            return;
        
        if (!_curaParamsReceiver.CEParameters.GlobalParams.TryGetValue("adhesion_type", out var param))
            return;
        _curaParamsReceiver.CEParameters.UserParameters[param.id] = "none";
    }

    public void SaveToXmlProp(IST_XMLPropPointer xmlProp)
    {
        using var xmlPropCom = new ComWrapper<IST_XMLPropPointer>(xmlProp);
        var xmlPropObj = xmlPropCom.Instance;
        if (xmlPropObj == null)
            return;
        
        _operationProps.SaveUserParameterToXml(xmlPropObj);
        _operationProps.SaveManufacturerToXml(xmlPropObj);
        _operationProps.SaveMachineToXml(xmlPropObj);
        _operationProps.SaveExtruderToXml(xmlPropObj);
        _operationProps.SaveMaterialBrandToXml(xmlPropObj);
        _operationProps.SaveMaterialToXml(xmlPropObj);
        _operationProps.SaveVariantToXml(xmlPropObj);
        _operationProps.SaveProfileToXml(xmlPropObj);
        _operationProps.SaveQualityToXml(xmlPropObj);
        _operationProps.SaveShowCustomParametersToXml(xmlPropObj);
        _operationProps.SaveSettingVisibilityToXml(xmlPropObj);
        _operationProps.SaveAutoToolParameterizationToXml(xmlPropObj);
        _operationProps.SaveToolpathParsingModeToXml(xmlPropObj);
        _operationProps.SaveOutputAdditionalParametersToXml(xmlPropObj);
        _operationProps.SaveOutputFilamentExtrudingToXml(xmlPropObj);
        _operationProps.SaveFilamentExtrudingLengthToXml(xmlPropObj);
        xmlPropObj.Ptr["CuraPath.SetPath"].ValueAsBoolean = _curaLibraryPath.UserPathEnabled;
        xmlPropObj.Ptr["CuraPath.Path"].ValueAsString = _curaLibraryPath.CuraPath;
    }
}