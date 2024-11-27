using System.Runtime.InteropServices;
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
    ~OperationLoadSaveXmlProp()
    {

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
        using var xmlPropCom = new ComWrapper<IST_XMLPropPointer>(xmlProp);
        var xmlPropObj = xmlPropCom.Instance;
        if (xmlPropObj == null)
            return;

        if (_curaParamsReceiver == null)
            throw new Exception("_curaParamsReceiver is null");
        if (_operationProps == null)
            throw new Exception("_operationProps is null");

        if (!_curaLibraryPath.CheckLibraryExists(xmlPropCom.Instance))
            return;

        _curaLibraryPath.UserPathEnabled = xmlPropObj.Bol["CuraPath.SetPath"];
        
        _curaLibraryPath.UserCuraPath = xmlPropObj.Str["CuraPath.Path"];
        
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

        var manufacturer = xmlPropObj.Str["GeneralParameters.Manufacturer"]; 
        if (manufacturer != null && manufacturer != "")
            _curaParamsReceiver.CEParameters.SetSelectedMachineBrand(manufacturer);
  
        var machine = xmlPropObj.Str["GeneralParameters.Machine"];
        if (machine != null && machine != "")
            _curaParamsReceiver.CEParameters.SetSelectedMachine(machine);
  
        var extruder = xmlPropObj.Str["GeneralParameters.Extruder"];
        if (extruder != null && extruder != "")
            _curaParamsReceiver.CEParameters.SetSelectedExtruder(extruder);
  
        var materialBrand = xmlPropObj.Str["GeneralParameters.MaterialBrand"];
        if (materialBrand != null && materialBrand != "")
            _curaParamsReceiver.CEParameters.SetSelectedMaterialBrand(materialBrand);
  
        var material = xmlPropObj.Str["GeneralParameters.Material"];
        if (material != null && material != "")
            _curaParamsReceiver.CEParameters.SetSelectedMaterial(material);
  
        var variant = xmlPropObj.Str["GeneralParameters.Variant"];
        if (variant != null && variant != "")
            _curaParamsReceiver.CEParameters.SetSelectedVariant(variant);

        var profile = xmlPropObj.Str["GeneralParameters.Profile"];
        if (profile != null && profile != "")
            _curaParamsReceiver.CEParameters.SetSelectedIntentCategory(profile);
  
        var quality = xmlPropObj.Str["GeneralParameters.Quality"];
        if (quality != null && quality != "")
            _curaParamsReceiver.CEParameters.SetSelectedQuality(quality);
  
        _curaParamsReceiver.CEParameters.IsShowCustomParameters = xmlPropObj.Bol["GeneralParameters.ShowCustomParameters"];
  
        var settingVisibility = xmlPropObj.Str["GeneralParameters.SettingVisibility"];
        if (settingVisibility != null && settingVisibility != "")
            _curaParamsReceiver.CEParameters.SelectedSettingVisibilities = settingVisibility;

        _operationProps.IsAutoToolParameterization = xmlPropObj.Bol["GeneralParameters.AutoToolParameterization"];
  
        var tpMode = xmlPropObj.Str["GeneralParameters.ToolpathParsingMode"];
        if (tpMode != null)
            _operationProps.Tpm = tpMode=="Simplified" ? ToolpathParsingMode.tpmSimplified : ToolpathParsingMode.tpmGCodeBased;
  
        _operationProps.IsOutputAdditionalClDataParameters = xmlPropObj.Bol["GeneralParameters.OutputAdditionalParameters"];

        _operationProps.IsOutputFilamentExtruding = xmlPropObj.Bol["GeneralParameters.OutputFilamentExtruding"];
  
         var fel = xmlPropObj.Flt["GeneralParameters.FilamentExtrudingLength"];
         if (fel != null)
            _operationProps.FilamentExtrudingLength = fel;
  
        _curaParamsReceiver.CEParameters.UpdateAllParameters();
    }
    
    private void LoadAdhesionValueFromXml(IST_XMLPropPointer xmlProp)
    {
        if (_curaParamsReceiver == null)
            throw new Exception("_curaParamsReceiver is null");
        
        var value = xmlProp.Bol["GeneralParameters.Adhesion"];
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
        xmlPropObj.Bol["CuraPath.SetPath"] = _curaLibraryPath.UserPathEnabled;
        xmlPropObj.Str["CuraPath.Path"] = _curaLibraryPath.CuraPath;
    }
}