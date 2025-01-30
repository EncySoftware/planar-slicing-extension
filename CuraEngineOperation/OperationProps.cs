using System.Diagnostics.CodeAnalysis;
using CAMAPI.DotnetHelper;
using CAMAPI.Extensions;
using CAMAPI.TechOperation;
using CAMAPI.UIDialogs;
using CAMAPI.UIDialogs.DotnetHelper;
using CuraEngineParametersLibrary;
using STCustomPropTypes;
using STXMLPropTypes;

namespace CuraEngineOperation;

[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
public class OperationProps : IDisposable
{
    /// <summary>
    /// Object to get localized labels
    /// </summary>
    private readonly Localize _localize;

    /// <summary>
    /// Object to get paths to Cura library
    /// </summary>
    private readonly CuraLibraryPath _curaLibraryPath;

    private IExtensionInfo? Info;
    /// <summary>
    /// Properties of CuraEngine
    /// </summary>
    private readonly Parameters _curaParameters;

    /// <summary>
    /// Text, user entered in filter parameters field
    /// </summary>
    private string _searchFilter = "";
    
    /// <summary>
    /// Wrapper over additional iterator of operation properties. We need it to create dynamic properties
    /// </summary>
    private ComWrapper<IST_SimplePropIterator>? _additionalPropIteratorComWrapper;

    /// <summary>
    /// COM-object to iterate over operation properties
    /// </summary>
    private IST_SimplePropIterator? AdditionalPropIterator => _additionalPropIteratorComWrapper?.Instance;

    /// <summary>
    /// Wrapper over iterator of operation properties. We need it to create dynamic properties
    /// </summary>
    private ComWrapper<IST_SimplePropIterator>? _propIteratorComWrapper;
    
    /// <summary>
    /// COM-object to iterate over operation properties
    /// </summary>
    private IST_SimplePropIterator? PropIterator => _propIteratorComWrapper?.Instance;

    /// <summary>
    /// Wrapper over builder of dialog windows
    /// </summary>
    private readonly ComWrapper<IST_CustomPropHelpers>? _propHelpersComWrapper;

    /// <summary>
    /// COM-object to build dialog window
    /// </summary>
    private IST_CustomPropHelpers? PropHelpers => _propHelpersComWrapper?.Instance;
    
    /// <summary>
    /// Wrapper over operation properties
    /// </summary>
    private readonly ComWrapper<IST_XMLPropPointer>? _operationXmlPropComWrapper;
    
    /// <summary>
    /// COM-object to interact with operation properties from XML
    /// </summary>
    private IST_XMLPropPointer OperationXmlProp => _operationXmlPropComWrapper?.Instance;

    private bool _isOperationCreating = true;
    public bool IsAutoToolParameterization = true;
    private bool _isStrengthPropsExpanded = true;
    private bool _isSupportPropsExpanded = true;
    private bool _isAdhesionPropsExpanded = true;
    public ToolpathParsingMode Tpm = ToolpathParsingMode.tpmSimplified;
    
    private bool _isOutputAdditionalClDataParametersVisible;
    public bool IsOutputAdditionalClDataParameters;
    
    private bool _isOutputFilamentExtrudingVisible;
    public bool IsOutputFilamentExtruding;

    private bool _filamentExtrudingLengthVisible;
    public double FilamentExtrudingLength = 100;

    private bool _selectedSettingVisibilitiesVisible;

    public OperationProps(IExtensionInfo info,
        ICamApiTechOperation operation,
        Parameters curaParameters,
        Localize localize,
        CuraLibraryPath curaLibraryPath)
    {
        _curaParameters = curaParameters;
        _localize = localize;
        _curaLibraryPath = curaLibraryPath;

        // object to build dialog window
        _propHelpersComWrapper = SystemExtensionFactory.GetSingletonExtension<IST_CustomPropHelpers>("Extension.CustomPropHelpers", info);
        
        _operationXmlPropComWrapper = new ComWrapper<IST_XMLPropPointer>(operation.XMLProp);
        Info ??= info;
    }

    public void Dispose()
    {
        _propIteratorComWrapper?.Dispose();
        _additionalPropIteratorComWrapper?.Dispose();
        _propHelpersComWrapper?.Dispose();
        _operationXmlPropComWrapper?.Dispose();
        Info = null;
    }
    
    private ComWrapper<IExtensionManager> GetExtensionManager()
    {
        if (Info == null)
            throw new Exception("Failed to get ExtensionInfo");
        using var instanceInfoCom = new ComWrapper<IExtensionInstanceInfo>(Info.InstanceInfo);
        return new ComWrapper<IExtensionManager>(instanceInfoCom.Instance?.ExtensionManager);
    }
    
    public IST_CustomPropIterator CreateIterator(Parameters curaParameters)
    {
        // recreate iterator with mandatory disposing old one
        _propIteratorComWrapper?.Dispose();
        _propIteratorComWrapper = new ComWrapper<IST_SimplePropIterator>(PropHelpers?.CreateSimplePropIterator());
        if (PropIterator == null)
            throw new Exception("Failed to create SimplePropIterator");
        
        // fill iterator with properties
        curaParameters.UpdateAllParameters();
        foreach (var param in _curaParameters.GlobalParams.Values)
            param.ExistsInPropIterator = false;
        AddGeneralParameters(PropIterator);
        AddAutoToolParameterization(PropIterator);
        AddToolPathParsingMode(PropIterator);
        AddCustomParametersField(PropIterator);
            
        var ssv = curaParameters.SelectedSettingVisibilities;
        if (curaParameters.IsShowCustomParameters && !string.IsNullOrEmpty(ssv))
        {
            if (ssv == "all")
                FillPropIteratorByAllParams(PropIterator);
            else
                FillPropIteratorFromFile(PropIterator);
        }
        else
            FillSimplifiedPropIterator(PropIterator);
            
        PropIterator.MoveToRoot();
        return (IST_CustomPropIterator)PropIterator;
    }

    /// <summary>
    /// Check if parameter satisfies filter. We use this method, if field does not exist in
    /// GlobalParams of <see cref="_curaParameters"/>
    /// </summary>
    private bool AcceptedByFilter(string fieldCaption)
    {
        return string.IsNullOrEmpty(_searchFilter)
               || fieldCaption.Contains(_searchFilter, StringComparison.CurrentCultureIgnoreCase);
    }
    
    private bool IsParamVisible(Parameter param)
    {
        if (!param.ExistsInPropIterator)
            return false;
        if (!_curaParameters.IsParameterEnabled(param.id))
            return false;
        if (param.IsAcceptedByFilter)
            return true;
        return param.children?.Any(IsParamVisible) ?? false;
    }

    private bool IsParamVisible(string paramName)
    {
        if (!_curaParameters.GlobalParams.TryGetValue(paramName, out var param))
            return false;
        return IsParamVisible(param);
    }
    
    private void AddGeneralParameters(IST_SimplePropIterator iterator)
    {
        if (PropHelpers == null)
            return;
        
        var genParamsCaption = _localize.GetLabelTranslation("General parameters");
        using var genPropCom = new ComWrapper<IST_CustomComplexPropHelper>(PropHelpers.CreateComplexProp(genParamsCaption));
        var genProp = genPropCom.Instance;
        if (genProp == null)
            return;
        
        genProp.PropID = "_Cura_general_parameters";
        genProp.Hint = _localize.GetDescriptionTranslation(genProp.PropID, "General parameters", "");
        if (_curaParameters.GlobalParams.TryGetValue(genProp.PropID, out var param))
            param.ExistsInPropIterator = true;
        genProp.IconFile = "";
        genProp.Visible = new BooleanValueGetter(() => AcceptedByFilter(genParamsCaption));
        var textCaption = _localize.GetLabelTranslation("Click_to_open_general_parameters", "Click \"...\" to change");
        genProp.TextGetter = new StringValueGetter(() => textCaption);
        genProp.ButtonQuantity = 1;
        genProp.ButtonDisplayName[0] = "...";
        genProp.ButtonHelpText[0] = "";
        genProp.ButtonIconPath[0] = "";
        genProp.ButtonMode[0] = TCustomPropButtonState.cpbsClickable;
        genProp.ClickAction[0]  = new ButtonClickAction(delegate
        {
            using var extensionManagerCom = GetExtensionManager();
            var extensionManager = extensionManagerCom.Instance
                                   ?? throw new Exception("Failed to get ExtensionManager");

            using var dialogWindow = new CamApiInspectorWindow(extensionManager);
            dialogWindow.Caption = genParamsCaption;
            dialogWindow.SetPropIteratorGetter(new PropIteratorGetter(delegate()
            {
                _additionalPropIteratorComWrapper?.Dispose();
                _additionalPropIteratorComWrapper = new ComWrapper<IST_SimplePropIterator>(PropHelpers?.CreateSimplePropIterator());
                if (AdditionalPropIterator == null)
                    throw new Exception("Failed to create SimplePropIterator");
                FillGeneralParameters(AdditionalPropIterator);
                AdditionalPropIterator.MoveToRoot();
                return (IST_CustomPropIterator)AdditionalPropIterator;
            }));

            var buttons = TUIButtonTypeFlags.btfOk;
            dialogWindow.SetButtons((ushort)buttons);
            dialogWindow.Show();
        });
        iterator.AddNewProp(genProp, -1);
    }
    
    private void FillGeneralParameters(IST_SimplePropIterator iterator)
    {
        if (PropHelpers == null)
            return;
        var parentInd = -1;
        
        // machines brands
        var manufCaption = _localize.GetLabelTranslation("Manufacturer");
        using var manufPropCom = new ComWrapper<IST_CustomEnumWithIDPropHelper>(PropHelpers.CreateEnumWithIDProp(manufCaption));
        var manufProp = manufPropCom.Instance;
        if (manufProp != null)
        {
            manufProp.PropID = "_Cura_manufacturer";
            manufProp.Hint = _localize.GetDescriptionTranslation(manufProp.PropID, "Manufacturer", "");
            manufProp.IsStructural = new BooleanValueGetter(() => true);
            foreach (var manuf in _curaParameters.MachinesBrands)
                manufProp.Add(manuf.name, manuf.name, "");
            manufProp.ValueGetter = new StringValueGetter(() => _curaParameters.SelectedMachineBrand.name);
            manufProp.ValueSetter = new StringValueSetter(delegate (string v) 
            {
                _curaParameters.SetSelectedMachineBrand(v, true);
                SaveManufacturerToXml(OperationXmlProp);
            });
            iterator.AddNewProp(manufProp, parentInd);
        }
        
        // machines/printers
        var printCaption = _localize.GetLabelTranslation("Printers");
        using var printPropCom = new ComWrapper<IST_CustomEnumWithIDPropHelper>(PropHelpers.CreateEnumWithIDProp(printCaption));
        var machProp = printPropCom.Instance;
        if (machProp != null)
        {
            machProp.PropID = "_Cura_printers";
            machProp.Hint = _localize.GetDescriptionTranslation(machProp.PropID, "Printers", "");
            machProp.IsStructural = new BooleanValueGetter(() => true);
            foreach (var mach in _curaParameters.SelectedMachineBrand.Machines.Where(mach => mach.isVisible))
                machProp.Add(mach.id, mach.name, "");
            machProp.ValueGetter = new StringValueGetter(() => _curaParameters.SelectedMachine.id);
            machProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                _curaParameters.SetSelectedMachine(v);
                SaveMachineToXml(OperationXmlProp);
            });
            iterator.AddNewProp(machProp, parentInd);
        }
  
        // extruders
        var extrudersCaption = _localize.GetLabelTranslation("Extruders");
        using var extrudersPropCom = new ComWrapper<IST_CustomEnumWithIDPropHelper>(PropHelpers.CreateEnumWithIDProp(extrudersCaption));
        var extrudersProp = extrudersPropCom.Instance;
        if (extrudersProp != null)
        {
            extrudersProp.PropID = "_Cura_extruders";
            extrudersProp.IsStructural = new BooleanValueGetter(() => true);
            extrudersProp.Hint = _localize.GetDescriptionTranslation(extrudersProp.PropID, "Extruders", "");
            extrudersProp.Visible = new BooleanValueGetter(() => _curaParameters.SelectedMachine.Extruders.Count > 0);
            foreach (var extruder in _curaParameters.SelectedMachine.Extruders)
            {
                string caption;
                if (extruder.name.Contains("Extruder"))
                {
                    caption = extruder.name.Substring(0, 8);
                    caption = _localize.GetEnumsTranslation("_extruders", "Extruders", caption, caption);
                    var number = "";
                    if (8<extruder.name.Length)
                        number = extruder.name[8..];
                    caption += number;
                }
                else
                    caption = extruder.name;   
                   
                extrudersProp.Add(extruder.id, caption, "");
            }
            extrudersProp.ValueGetter = new StringValueGetter(() => _curaParameters.SelectedExtruder.id);
            extrudersProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                _curaParameters.SetSelectedExtruder(v);
                SaveExtruderToXml(OperationXmlProp);
            });
            iterator.AddNewProp(extrudersProp, parentInd);
        }
  
        // materials brands
        var mbCaption = _localize.GetLabelTranslation("Material brand");
        using var materialBrandPropCom = new ComWrapper<IST_CustomEnumWithIDPropHelper>(PropHelpers.CreateEnumWithIDProp(mbCaption));
        var materialBrandProp = materialBrandPropCom.Instance;
        if (materialBrandProp != null)
        {
            materialBrandProp.PropID = "_Cura_material_brand";
            materialBrandProp.Hint = _localize.GetDescriptionTranslation(materialBrandProp.PropID, "Material brand", "");
            materialBrandProp.IsStructural = new BooleanValueGetter(() => true);
            foreach (var mat in _curaParameters.MaterialsBrands)
            {
                var machine = _curaParameters.SelectedMachine;
                if (machine != null)
                {
                    if (machine.id.Contains("ultimaker"))
                    {
                      
                        if (mat.name.Contains("generic", StringComparison.CurrentCultureIgnoreCase) || mat.name.Contains("ultimaker", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (mat.name.ToLower().Contains("generic"))
                            {
                                var caption = _localize.GetEnumsTranslation("_material_brand", "Material brand", mat.name, mat.name);
                                materialBrandProp.Add(mat.name, caption, ""); 
                            }
                            else
                                materialBrandProp.Add(mat.name, mat.name, ""); 
                        }       
                    }
                    else
                    {
                        if (!mat.name.Contains("ultimaker", StringComparison.CurrentCultureIgnoreCase))
                            materialBrandProp.Add(mat.name, mat.name, ""); 
                    }
                }
                else
                    materialBrandProp.Add(mat.name, mat.name, ""); 
            }
            materialBrandProp.ValueGetter = new StringValueGetter(() => _curaParameters.SelectedMaterialBrand.name);
            materialBrandProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                _curaParameters.SetSelectedMaterialBrand(v, true);
                SaveMaterialBrandToXml(OperationXmlProp);
            });
            iterator.AddNewProp(materialBrandProp, parentInd);
        }
  
        // materials
        var matCaption = _localize.GetLabelTranslation("Materials");
        using var matPropCom = new ComWrapper<IST_CustomEnumWithIDPropHelper>(PropHelpers.CreateEnumWithIDProp(matCaption));
        var matProp = matPropCom.Instance;
        if (matProp != null)
        {
            matProp.PropID = "_Cura_materials";
            matProp.Hint = _localize.GetDescriptionTranslation(matProp.PropID, "Materials", "");
            matProp.IsStructural = new BooleanValueGetter(() => true);
            foreach (var mat in _curaParameters.SelectedMaterialBrand.Materials)
            {
                if (_curaParameters.ExcludedMaterials.Any(excludedMaterial => mat.id == excludedMaterial))
                    continue;
                if (mat.Color!="")
                    matProp.Add(mat.id, mat.GetCaption() + " (" + mat.Color + ", \u00F8"+ mat.Diameter +")", ""); 
                else
                    matProp.Add(mat.id, mat.GetCaption() + " (\u00F8"+ mat.Diameter +")", "");
            }
            matProp.ValueGetter = new StringValueGetter(() => _curaParameters.SelectedMaterial.id);
            matProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                _curaParameters.SetSelectedMaterial(v);
                SaveMaterialToXml(OperationXmlProp);
            });
            iterator.AddNewProp(matProp, parentInd);
        }   
  
        // variants
        var varCaption = _localize.GetLabelTranslation("Variants");
        using var varPropCom = new ComWrapper<IST_CustomEnumWithIDPropHelper>(PropHelpers.CreateEnumWithIDProp(varCaption));
        var varProp = varPropCom.Instance;
        if (varProp != null)
        {
            varProp.PropID = "_Cura_variants";
            varProp.Hint = _localize.GetDescriptionTranslation(varProp.PropID, "Variants", "");
            varProp.IsStructural = new BooleanValueGetter(() => true);
            varProp.Visible = new BooleanValueGetter(() => _curaParameters.Variants.Count > 0);
            foreach (var variant in _curaParameters.Variants)
                varProp.Add(variant.Name, variant.Name, "");
            varProp.ValueGetter = new StringValueGetter(() => _curaParameters.SelectedVariant?.Name ?? "");
            varProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                _curaParameters.SetSelectedVariant(v);
                SaveVariantToXml(OperationXmlProp);
            });
            iterator.AddNewProp(varProp, parentInd);
        }
  
        // profiles
        var profCaption = _localize.GetLabelTranslation("Profiles");
        using var profPropCom = new ComWrapper<IST_CustomEnumWithIDPropHelper>(PropHelpers.CreateEnumWithIDProp(profCaption));
        var profProp = profPropCom.Instance;
        if (profProp != null)
        {
            profProp.PropID = "_Cura_profiles";
            profProp.Hint = _localize.GetDescriptionTranslation(profProp.PropID, "Profiles", "");
            profProp.IsStructural = new BooleanValueGetter(() => true);
            profProp.Visible = new BooleanValueGetter(() => _curaParameters.IntentCategories.Count > 0);
            foreach (var pr in _curaParameters.IntentCategories)
            {
                var name = pr.IntentName;
                var caption = _localize.GetEnumsTranslation("_profiles", "Profiles", name, name);
                profProp.Add(pr.IntentCategoryName, caption, "");
            }
            profProp.ValueGetter = new StringValueGetter(() => _curaParameters.SelectedIntentCategory?.IntentCategoryName ?? "");
            profProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                _curaParameters.SetSelectedIntentCategory(v);
                SaveProfileToXml(OperationXmlProp);
            });
            iterator.AddNewProp(profProp, parentInd);
        }
      
        // resolution
        var resCaption = _localize.GetLabelTranslation("Resolutions");
        using var resPropCom = new ComWrapper<IST_CustomEnumWithIDPropHelper>(PropHelpers.CreateEnumWithIDProp(resCaption));
        var resProp = resPropCom.Instance;
        if (resProp != null)
        {
            resProp.PropID = "_Cura_resolutions";
            resProp.Hint = _localize.GetDescriptionTranslation(resProp.PropID, "Resolutions", "");
            resProp.IsStructural = new BooleanValueGetter(() => true);
            resProp.Visible = new BooleanValueGetter(() =>
                _curaParameters.SelectedIntentCategory != null &&
                _curaParameters.SelectedIntentCategory.QualityInstances.Count > 0);
            var selectedIntentCategory = _curaParameters.SelectedIntentCategory;
            if (selectedIntentCategory != null)
            {
                foreach (var res in selectedIntentCategory.QualityInstances)
                    resProp.Add(res.FileName, res.Caption, "");
            }
            resProp.ValueGetter = new StringValueGetter(() => _curaParameters.SelectedQuality?.FileName ?? "");
            resProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                _curaParameters.SetSelectedQuality(v);
                SaveQualityToXml(OperationXmlProp);
            }); 
            iterator.AddNewProp(resProp, parentInd);
        }
    }
    
    private void AddAutoToolParameterization(IST_SimplePropIterator simpleIterator)
    {
        if (PropHelpers == null)
            return;
        var atpCaption = _localize.GetLabelTranslation("Auto tool parameterization");
        using var atpPropCom = new ComWrapper<IST_CustomBooleanPropHelper>(PropHelpers.CreateBooleanProp(atpCaption));
        var atpProp = atpPropCom.Instance
            ?? throw new Exception("Failed to create BooleanProp for " + atpCaption);
        
        atpProp.PropID = "_Cura_auto_tool_parameterization";
        atpProp.Hint = _localize.GetDescriptionTranslation(atpProp.PropID, "Auto tool parameterization", "");
        if (_curaParameters.GlobalParams.TryGetValue(atpProp.PropID, out var param))
            param.ExistsInPropIterator = true;
        atpProp.Visible = new BooleanValueGetter(() => AcceptedByFilter(atpCaption));
        atpProp.ValueGetter = new BooleanValueGetter(() => IsAutoToolParameterization);
        atpProp.ValueSetter = new BooleanValueSetter(delegate (bool v)
        {
            IsAutoToolParameterization = v;
            if (v)
            {
                SetToolParameterization("layer_height");
                SetToolParameterization("line_width"); 
            }
            SaveAutoToolParameterizationToXml(OperationXmlProp);
        });
        simpleIterator.AddNewProp(atpProp, -1);
    }
    
    private void SetToolParameterization(string paramName)
    {
        if (!IsAutoToolParameterization)
            return;
        
        var paramNameInCamSystem = "";
        paramNameInCamSystem = paramName == "layer_height" ? "WorkingLength" : "Diameter";
        paramNameInCamSystem = "ToolSection.Tools(0).Properties." + paramNameInCamSystem;
        using var xmlParamCom = new ComWrapper<IST_XMLPropPointer>(OperationXmlProp.Ptr[paramNameInCamSystem]);
        var xmlParam = xmlParamCom.Instance
                        ?? throw new Exception("Failed to get XMLPropPointer for " + paramNameInCamSystem);
        
        var value = _curaParameters.GetValue(paramName, true);
        if (DoubleParser.DoubleTryParse(value, out var doubleValue))
            xmlParam.ValueAsDouble = doubleValue; 
    }
    
    private void AddToolPathParsingMode(IST_SimplePropIterator simpleIterator)
    {
        if (PropHelpers == null)
            return;
        
        var cldataModeCaption = _localize.GetLabelTranslation("Toolpath parsing mode");
        var cldataModeProp = PropHelpers.CreateEnumWithIDProp(cldataModeCaption)
            ?? throw new Exception("Failed to create EnumWithIDProp for " + cldataModeCaption);
        cldataModeProp.PropID = "_Cura_toolpath_parsing_mode";
        cldataModeProp.Hint = _localize.GetDescriptionTranslation(cldataModeProp.PropID, "Toolpath parsing mode", "");
        if (_curaParameters.GlobalParams.TryGetValue(cldataModeProp.PropID, out var param))
            param.ExistsInPropIterator = true;
        cldataModeProp.IsStructural = new BooleanValueGetter(() => true);

        var smplCaption = _localize.GetEnumsTranslation("_toolpath_parsing_mode", "Toolpath parsing mode", "Simplified", "Simplified");
        cldataModeProp.Add("Simplified", smplCaption, ""); 
        var gcodeCaption = _localize.GetEnumsTranslation("_toolpath_parsing_mode", "Toolpath parsing mode", "GCodeBased", "GCode based");
        cldataModeProp.Add("GCodeBased", gcodeCaption, ""); 
        cldataModeProp.ValueGetter = new StringValueGetter(() =>
            Tpm == ToolpathParsingMode.tpmSimplified ? "Simplified" : "GCodeBased");   
        cldataModeProp.ValueSetter = new StringValueSetter(delegate (string v)
        {
            Tpm = v=="Simplified" ? ToolpathParsingMode.tpmSimplified : ToolpathParsingMode.tpmGCodeBased;
            SaveToolpathParsingModeToXml(OperationXmlProp);
        });
        
        var parentIndex = simpleIterator.AddNewProp(cldataModeProp, -1);
        
        var atpCaption = _localize.GetLabelTranslation("Output additional parameters");
        using var atpPropCom = new ComWrapper<IST_CustomBooleanPropHelper>(PropHelpers.CreateBooleanProp(atpCaption));
        var atpProp = atpPropCom.Instance
            ?? throw new Exception("Failed to create BooleanProp for " + atpCaption);
        atpProp.PropID = "_Cura_output_additional_parameters";
        atpProp.Hint = _localize.GetDescriptionTranslation(atpProp.PropID, "Output additional parameters", "");
        if (_curaParameters.GlobalParams.TryGetValue(atpProp.PropID, out param))
            param.ExistsInPropIterator = true;
        atpProp.IsStructural = new BooleanValueGetter(() => true);
        var atpPropVisible = new BooleanValueGetter(delegate ()
        {
            _isOutputAdditionalClDataParametersVisible = AcceptedByFilter(atpCaption)
                                                        && Tpm == ToolpathParsingMode.tpmSimplified;
            return _isOutputAdditionalClDataParametersVisible;
        });
        atpProp.Visible = atpPropVisible;
        atpProp.ValueGetter = new BooleanValueGetter(() => IsOutputAdditionalClDataParameters);
        atpProp.ValueSetter = new BooleanValueSetter(delegate (bool v)
        {
            IsOutputAdditionalClDataParameters = v;
            SaveOutputAdditionalParametersToXml(OperationXmlProp);
        });
        simpleIterator.AddNewProp(atpProp, parentIndex);
        
        var ofeCaption = _localize.GetLabelTranslation("Output filament extruding");
        using var ofePropCom = new ComWrapper<IST_CustomBooleanPropHelper>(PropHelpers.CreateBooleanProp(ofeCaption));
        var ofeProp = ofePropCom.Instance
            ?? throw new Exception("Failed to create BooleanProp for " + ofeCaption);
        ofeProp.PropID = "_Cura_output_filament_extruding";
        ofeProp.Hint = _localize.GetDescriptionTranslation(ofeProp.PropID, "Output filament extruding", "");
        if (_curaParameters.GlobalParams.TryGetValue(ofeProp.PropID, out param))
            param.ExistsInPropIterator = true;
        ofeProp.IsStructural = new BooleanValueGetter(() => true);
        var ofePropVisible = new BooleanValueGetter(delegate ()
        {
            _isOutputFilamentExtrudingVisible = AcceptedByFilter(ofeCaption)
                                                && Tpm == ToolpathParsingMode.tpmGCodeBased;
            return _isOutputFilamentExtrudingVisible;
        });
        ofeProp.Visible = ofePropVisible;
        ofeProp.ValueGetter = new BooleanValueGetter(() => IsOutputFilamentExtruding);
        ofeProp.ValueSetter = new BooleanValueSetter(delegate (bool v)
        {
            IsOutputFilamentExtruding = v;
            SaveOutputFilamentExtrudingToXml(OperationXmlProp);
        });
        simpleIterator.AddNewProp(ofeProp, parentIndex);
        
        var felCaption = _localize.GetLabelTranslation("Filament extruding length per frame");
        using var felPropCom = new ComWrapper<IST_CustomDoublePropHelper>(PropHelpers.CreateDoubleProp(felCaption));
        var felProp = felPropCom.Instance
            ?? throw new Exception("Failed to create DoubleProp for " + felCaption);
        felProp.PropID = "_Cura_filament_extruding_length";
        felProp.Hint = _localize.GetDescriptionTranslation(felProp.PropID, "Filament extruding length per frame", "");
        if (_curaParameters.GlobalParams.TryGetValue(felProp.PropID, out param))
            param.ExistsInPropIterator = true;
        felProp.IsStructural = new BooleanValueGetter(() => true);
        felProp.UnitsStr = "mm";
        var felPropVisible = new BooleanValueGetter(delegate
        {
            _filamentExtrudingLengthVisible = AcceptedByFilter(felCaption)
                                             && Tpm == ToolpathParsingMode.tpmGCodeBased
                                             && IsOutputFilamentExtruding;
            return _filamentExtrudingLengthVisible;
        });
        felProp.Visible = felPropVisible;
        felProp.ValueGetter = new DoubleValueGetter(() => FilamentExtrudingLength);
        felProp.ValueSetter = new DoubleValueSetter(delegate (double v)
        {
            FilamentExtrudingLength = v;
            SaveFilamentExtrudingLengthToXml(OperationXmlProp);
        });
        simpleIterator.AddNewProp(felProp, parentIndex);

        cldataModeProp.Visible = new BooleanValueGetter(() => AcceptedByFilter(cldataModeCaption)
                                                        || atpPropVisible.GetValue()
                                                        || ofePropVisible.GetValue()
                                                        || felPropVisible.GetValue());
    }

    private void AddCustomParametersField(IST_SimplePropIterator simpleIterator)
    {
        if (PropHelpers == null)
            return;

        // show custom parameters
        var scpCaption = _localize.GetLabelTranslation("Show custom parameters");
        using var scpPropCom = new ComWrapper<IST_CustomBooleanPropHelper>(PropHelpers.CreateBooleanProp(scpCaption));
        var scpProp = scpPropCom.Instance
                      ?? throw new Exception("Failed to create BooleanProp for " + scpCaption);
        scpProp.PropID = "_Cura_show_custom_parameters";
        scpProp.Hint = _localize.GetDescriptionTranslation(scpProp.PropID, "Show custom parameters", "");
        if (_curaParameters.GlobalParams.TryGetValue(scpProp.PropID, out var param))
            param.ExistsInPropIterator = true;
        scpProp.IsStructural = new BooleanValueGetter(() => true);
        scpProp.ValueGetter = new BooleanValueGetter(() => _curaParameters.IsShowCustomParameters);
        scpProp.ValueSetter = new BooleanValueSetter(delegate (bool v)
        {
            _curaParameters.IsShowCustomParameters = v;
            SaveShowCustomParametersToXml(OperationXmlProp);
        });
        var parentInd = simpleIterator.AddNewProp(scpProp, -1);
        
        // setting visibility
        var svCaption = _localize.GetLabelTranslation("Setting visibility");
        using var spPropCom = new ComWrapper<IST_CustomEnumWithIDPropHelper>(PropHelpers.CreateEnumWithIDProp(svCaption));
        var svProp = spPropCom.Instance
            ?? throw new Exception("Failed to create EnumWithIDProp for " + svCaption);
        svProp.PropID = "_Cura_setting_visibility";
        svProp.Hint = _localize.GetDescriptionTranslation(svProp.PropID, "Setting visibility", "");
        if (_curaParameters.GlobalParams.TryGetValue(svProp.PropID, out param))
            param.ExistsInPropIterator = true;
        svProp.IsStructural = new BooleanValueGetter(() => true);
        var svPropVisible = new BooleanValueGetter(delegate
        {
            _selectedSettingVisibilitiesVisible = AcceptedByFilter(svCaption) && _curaParameters.IsShowCustomParameters;
            return _selectedSettingVisibilitiesVisible;
        });
        svProp.Visible = svPropVisible;
        foreach (var sv in _curaParameters.SettingVisibilities)
        {
            var svName = _localize.GetEnumsTranslation("_setting_visibility", "Setting visibility", sv, sv);
            svProp.Add(sv, svName, "");
        }
        
        svProp.ValueGetter = new StringValueGetter(() => _curaParameters.SelectedSettingVisibilities);
        svProp.ValueSetter = new StringValueSetter(delegate (string v)
        {
            _curaParameters.SelectedSettingVisibilities = v;
            SaveSettingVisibilityToXml(OperationXmlProp);
        });
        scpProp.Visible = new BooleanValueGetter(() =>
            AcceptedByFilter(scpCaption) || svPropVisible.GetValue());
        simpleIterator.AddNewProp(svProp, parentInd);
    }
    
    private void FillPropIteratorByAllParams(IST_SimplePropIterator simpleIterator)
    {
        var filePath = _curaParameters.CuraPath + @"share\cura\resources\setting_visibility\basic.cfg";
        if (!File.Exists(filePath))
            return;
        
        var sr = new StreamReader(filePath);
        var line = sr.ReadLine();
        while (line != null)
        {
            line = line.Trim();
            if (line.StartsWith("[") && line.EndsWith("]") && !line.Contains("general") && 
                !line.Contains("machine_settings") && !line.Contains("dual"))
            {
                line = line.Replace("[", "");
                line = line.Replace("]", "");
                if (_curaParameters.GlobalParams.TryGetValue(line, out var param))
                {
                    var prop = GetComplexProp(param);
                    param.indProp = simpleIterator.AddNewProp(prop, -1);
                    if (param.HasChildren())
                        AddChildrenProps(simpleIterator, param.children, param.indProp);
                }
            }
            line = sr.ReadLine();
        }
    }
    
    private void FillPropIteratorFromFile(IST_SimplePropIterator simpleIterator)
    {
        var filename = _curaParameters.CuraPath + @"share\cura\resources\setting_visibility\" + _curaParameters.SelectedSettingVisibilities + ".cfg";
        AddComplexProps(simpleIterator, filename);
        AddProps(simpleIterator, filename);
    }
    
    private void FillSimplifiedPropIterator(IST_SimplePropIterator simpleIterator)
    {
        if (PropHelpers == null)
            return;
        var parentInd = -1;
  
        // strength
        var strengthCaption = _localize.GetLabelTranslation("Strength");
        using var strengthPropCom = new ComWrapper<IST_CustomComplexPropHelper>(PropHelpers.CreateComplexProp(strengthCaption));
        var strengthProp = strengthPropCom.Instance
            ?? throw new Exception("Failed to create ComplexProp for " + strengthCaption);
        strengthProp.PropID = "_Cura_strength";
        strengthProp.Hint = _localize.GetDescriptionTranslation(strengthProp.PropID, "Strength", "");
        strengthProp.IconFile = @"$(SUPPLEMENT_FOLDER)\operations\TypeImages\MeasuringItem.bmp";
        strengthProp.PropIsExpandedGetter = new BooleanValueGetter(() => _isStrengthPropsExpanded);
        strengthProp.PropIsExpandedSetter = new BooleanValueSetter(v => _isStrengthPropsExpanded = v);
        strengthProp.Visible = new BooleanValueGetter(() => AcceptedByFilter(strengthCaption)
                                                            || IsParamVisible("infill_sparse_density")
                                                            || IsParamVisible("infill_pattern")
                                                            || IsParamVisible("wall_thickness")
                                                            || IsParamVisible("top_bottom_thickness"));
        parentInd = simpleIterator.AddNewProp(strengthProp, -1);

        if (_curaParameters.GlobalParams.TryGetValue("infill_sparse_density", out var param))
        {
            var infillProp = (IST_CustomDoublePropHelper)GetFloatProp(param);
            simpleIterator.AddNewProp(infillProp, parentInd);
        }

        if (_curaParameters.GlobalParams.TryGetValue("infill_pattern", out param))
        {
            var infillPatternProp = (IST_CustomEnumWithIDPropHelper)GetEnumProp(param);
            infillPatternProp.IsStructural = new BooleanValueGetter(() => true);
            simpleIterator.AddNewProp(infillPatternProp, parentInd);
        }

        if (_curaParameters.GlobalParams.TryGetValue("wall_thickness", out param))
        {
            var wallProp = (IST_CustomDoublePropHelper)GetFloatProp(param);
            wallProp.IsStructural = new BooleanValueGetter(() => true);
            simpleIterator.AddNewProp(wallProp, parentInd);
        }

        if (_curaParameters.GlobalParams.TryGetValue("top_bottom_thickness", out param))
        {
            var topBottomProp = (IST_CustomDoublePropHelper)GetFloatProp(param);
            topBottomProp.IsStructural = new BooleanValueGetter(() => true);
            simpleIterator.AddNewProp(topBottomProp, parentInd);
        }
  
        // support
        var supportCaption = _localize.GetLabelTranslation("Support");
        using var supportPropCom = new ComWrapper<IST_CustomComplexPropHelper>(PropHelpers.CreateComplexProp(supportCaption));
        var supportProp = supportPropCom.Instance
            ?? throw new Exception("Failed to create ComplexProp for " + supportCaption);
        supportProp.PropID = "_Cura_support";
        supportProp.Hint = _localize.GetDescriptionTranslation(supportProp.PropID, "Support", "");
        supportProp.IconFile = @"$(SUPPLEMENT_FOLDER)\operations\TypeImages\MeasuringItem.bmp";
        supportProp.PropIsExpandedGetter = new BooleanValueGetter(() => _isSupportPropsExpanded);
        supportProp.PropIsExpandedSetter = new BooleanValueSetter(v => _isSupportPropsExpanded = v);
        supportProp.Visible = new BooleanValueGetter(() => AcceptedByFilter(supportCaption)
                                                           || IsParamVisible("support_enable")
                                                           || IsParamVisible("support_structure")
                                                           || IsParamVisible("support_type"));
        parentInd = simpleIterator.AddNewProp(supportProp, -1);
        
        if (_curaParameters.GlobalParams.TryGetValue("support_enable", out var seParam))
        {
            var supportEnabledProp = (IST_CustomBooleanPropHelper)GetBoolProp(seParam);
            supportEnabledProp.IsStructural = new BooleanValueGetter(() => true);
            simpleIterator.AddNewProp(supportEnabledProp, parentInd);
        }

        if (_curaParameters.GlobalParams.TryGetValue("support_structure", out var ssParam))
        {
            var supportStructureProp = (IST_CustomEnumWithIDPropHelper)GetEnumProp(ssParam);
            supportStructureProp.IsStructural = new BooleanValueGetter(() => true);
            simpleIterator.AddNewProp(supportStructureProp, parentInd);
        }

        if (_curaParameters.GlobalParams.TryGetValue("support_type", out var stParam))
        {
            var supportTypeProp = (IST_CustomEnumWithIDPropHelper)GetEnumProp(stParam);
            supportTypeProp.IsStructural = new BooleanValueGetter(() => true);
            simpleIterator.AddNewProp(supportTypeProp, parentInd);
        }
        
        // adhesion
        if (!_curaParameters.GlobalParams.TryGetValue("adhesion_type", out param))
            return;
        param.ExistsInPropIterator = true;
        var propName = _localize.GetLabelTranslation("Adhesion");
        using var adhesionCom = new ComWrapper<IST_CustomBooleanPropHelper>(PropHelpers.CreateBooleanProp(propName));
        var adhesion = adhesionCom.Instance
            ?? throw new Exception("Failed to create BooleanProp for " + propName);
        adhesion.PropID = "_Cura_Adhesion";
        adhesion.Hint = _localize.GetDescriptionTranslation(strengthProp.PropID, "Adhesion", "");
        adhesion.IsStructural = new BooleanValueGetter(() => true);
        adhesion.Visible = new BooleanValueGetter(() => AcceptedByFilter(propName) && IsParamVisible(param));
        adhesion.PropIsExpandedGetter = new BooleanValueGetter(() => _isAdhesionPropsExpanded);
        adhesion.PropIsExpandedSetter = new BooleanValueSetter((v) => _isAdhesionPropsExpanded = v);
        adhesion.ValueGetter = new BooleanValueGetter(delegate ()
        {
            var value = _curaParameters.GetValue(param.id, true);
            return value != "none";
        });
        adhesion.ValueSetter = new BooleanValueSetter(delegate (bool v)
        {
            if (v)
            {
                _curaParameters.SetValue(param.id, "brim", true);
                AddUserParameter(param.id, "brim", true); 
            }
            else
            {
                _curaParameters.SetValue(param.id, "none", true);
                AddUserParameter(param.id, "none", true); 
            }
        });
        
        simpleIterator.AddNewProp(adhesion, -1);
    }
    
    private IST_CustomProp GetStringProp(Parameter param)
    {
        if (PropHelpers == null)
            throw new Exception("PropHelpers is null");
        var label = _localize.GetLabelTranslation(param.id, param.label);
        var stringProp = PropHelpers.CreateStringProp(label)
            ?? throw new Exception("Failed to create StringProp for " + label);
        if (param.icon != null && param.icon != "")
            stringProp.IconFile = _curaParameters.CuraPath + @"share\cura\resources\themes\cura-light\icons\default\" + param.icon + ".svg";
        param.ExistsInPropIterator = true;
        stringProp.PropID = param.id;
        stringProp.UnitsStr = param.unit;
        stringProp.Hint = _localize.GetDescriptionTranslation(param.id, param.label, param.description);
        stringProp.PropIsExpandedGetter = new BooleanValueGetter(() => param.IsExpanded);
        stringProp.PropIsExpandedSetter = new BooleanValueSetter((v) => param.IsExpanded = v);
  
        stringProp.RestoreValueProc = new DefaultPropValue(delegate()
        {
            if (_curaParameters.UserParameters.TryGetValue(param.id, out var _))
            {
                RemoveUserParameter(param.id);
                _curaParameters.UpdateAllParameters();
            }
        });
        stringProp.IsStructural = new BooleanValueGetter(() => false);
        stringProp.Visible = new BooleanValueGetter(() => IsParamVisible(param));
        stringProp.ValueGetter = new StringValueGetter(() => _curaParameters.GetValue(param.id, true));
        stringProp.ValueSetter = new StringValueSetter(delegate (string v)
        {
            if (v != null)
            {
                _curaParameters.SetValue(param.id, v, true);
                AddUserParameter(param.id, v, true);
            }
            else
            {
                _curaParameters.SetValue(param.id, "", true);
                AddUserParameter(param.id, "", true);
            }
            _curaParameters.UpdateAllParameters();
               
        });
        return stringProp;
    }
  
    private IST_CustomProp GetIntegerProp(Parameter param)
    {
        if (PropHelpers == null)
            throw new Exception("PropHelpers is null");
        var label = _localize.GetLabelTranslation(param.id, param.label);
        var integerProp = PropHelpers.CreateIntegerProp(label)
            ?? throw new Exception("Failed to create IntegerProp for " + label);
        if (param.icon != null && param.icon != "")
            integerProp.IconFile = _curaParameters.CuraPath + "share\\cura\\resources\\themes\\cura-light\\icons\\default\\" + param.icon + ".svg";
        param.ExistsInPropIterator = true;
        integerProp.PropID = param.id;
        integerProp.UnitsStr = param.unit;
        integerProp.Hint = _localize.GetDescriptionTranslation(param.id, param.label, param.description);
        integerProp.PropIsExpandedGetter = new BooleanValueGetter(() => param.IsExpanded);
        integerProp.PropIsExpandedSetter = new BooleanValueSetter((v) => param.IsExpanded = v);
        integerProp.IsStructural = new BooleanValueGetter(() => false);
        integerProp.RestoreValueProc = new DefaultPropValue(delegate()
        {
            String userParam;
            if (_curaParameters.UserParameters.TryGetValue(param.id, out userParam))
            {
                RemoveUserParameter(param.id);
                _curaParameters.UpdateAllParameters();
            }
        });
        integerProp.Visible = new BooleanValueGetter(() => IsParamVisible(param));
        integerProp.ValueGetter = new IntegerValueGetter(delegate
        {
            var value = _curaParameters.GetValue(param.id, true);
            if (int.TryParse(value, out var intValue))
            {
                return intValue; 
            }
            return 0;
        });
        integerProp.ValueSetter = new IntegerValueSetter(delegate (int v)
        {
            _curaParameters.SetValue(param.id, v.ToString(), true);
            AddUserParameter(param.id, v.ToString(), true); 
            _curaParameters.UpdateAllParameters();
        });
        return integerProp;
    }
  
    private IST_CustomProp GetFloatProp(Parameter param)
    {
        if (PropHelpers == null)
            throw new Exception("PropHelpers is null");
        var label = _localize.GetLabelTranslation(param.id, param.label);
        var floatProp = PropHelpers.CreateDoubleProp(label)
            ?? throw new Exception("Failed to create DoubleProp for " + label);
        if (param.icon != null && param.icon != "")
            floatProp.IconFile = _curaParameters.CuraPath + "share\\cura\\resources\\themes\\cura-light\\icons\\default\\" + param.icon + ".svg";
        param.ExistsInPropIterator = true;
        floatProp.PropID = param.id;
        floatProp.UnitsStr = param.unit;
        floatProp.Hint = _localize.GetDescriptionTranslation(param.id, param.label, param.description);
        floatProp.PropIsExpandedGetter = new BooleanValueGetter(() => param.IsExpanded);
        floatProp.PropIsExpandedSetter = new BooleanValueSetter((v) => param.IsExpanded = v);
        floatProp.IsStructural = new BooleanValueGetter(() => false);
        floatProp.RestoreValueProc = new DefaultPropValue(delegate()
        {
            String userParam;
            if (_curaParameters.UserParameters.TryGetValue(param.id, out userParam))
            {
                RemoveUserParameter(param.id);
                _curaParameters.UpdateAllParameters();
            }
        });
        floatProp.Visible = new BooleanValueGetter(() => IsParamVisible(param));
        floatProp.ValueGetter = new DoubleValueGetter(delegate ()
        {
            var value = _curaParameters.GetValue(param.id, true);
            double doubleValue;
            if (DoubleParser.DoubleTryParse(value, out doubleValue))
            {
                return doubleValue; 
            }
            return 0;
        });
        floatProp.ValueSetter = new DoubleValueSetter(delegate (double v)
        {
            _curaParameters.SetValue(param.id, v.ToString(), true);
            AddUserParameter(param.id, v.ToString(), true); 
            _curaParameters.UpdateAllParameters();
            if (param.id=="layer_height")
                SetToolParameterization("layer_height");
            if (param.id=="line_width")
                SetToolParameterization("line_width");
        });
        return floatProp;
    }

    private IST_CustomProp GetBoolProp(Parameter param)
    {
        if (PropHelpers == null)
            throw new Exception("PropHelpers is null");
        var label = _localize.GetLabelTranslation(param.id, param.label);
        var boolProp = PropHelpers.CreateBooleanProp(label)
            ?? throw new Exception("Failed to create BooleanProp for " + label);
        if (param.icon != null && param.icon != "")
            boolProp.IconFile = _curaParameters.CuraPath + "share\\cura\\resources\\themes\\cura-light\\icons\\default\\" + param.icon + ".svg";
        param.ExistsInPropIterator = true;
        boolProp.PropID = param.id;
        boolProp.Hint = _localize.GetDescriptionTranslation(param.id, param.label, param.description);
        boolProp.PropIsExpandedGetter = new BooleanValueGetter(() => param.IsExpanded);
        boolProp.PropIsExpandedSetter = new BooleanValueSetter((v) => param.IsExpanded = v);
        boolProp.IsStructural = new BooleanValueGetter(() => false);
        boolProp.RestoreValueProc = new DefaultPropValue(delegate()
        {
            String userParam;
            if (_curaParameters.UserParameters.TryGetValue(param.id, out userParam))
            {
                RemoveUserParameter(param.id);
                _curaParameters.UpdateAllParameters();
            }
        });
        boolProp.Visible = new BooleanValueGetter(() => IsParamVisible(param));
  
        boolProp.ValueGetter = new BooleanValueGetter(delegate ()
        {
            var value = _curaParameters.GetValue(param.id, true);
            if (bool.TryParse(value, out var boolValue))
            {
                return boolValue; 
            }
            return true;
        });
        boolProp.ValueSetter = new BooleanValueSetter(delegate (bool v)
        {
            _curaParameters.SetValue(param.id, v.ToString(), true);
            AddUserParameter(param.id, v.ToString(), true);
            _curaParameters.UpdateAllParameters(); 
        });
        return boolProp;
    }

    private IST_CustomProp GetEnumProp(Parameter param)
    {
        if (PropHelpers == null)
            throw new Exception("PropHelpers is null");
        var label = _localize.GetLabelTranslation(param.id, param.label);
        var enumProp = PropHelpers.CreateEnumWithIDProp(label)
            ?? throw new Exception("Failed to create EnumWithIDProp for " + label);
        if (param.icon != null && param.icon != "")
            enumProp.IconFile = _curaParameters.CuraPath + "share\\cura\\resources\\themes\\cura-light\\icons\\default\\" + param.icon + ".svg";
        param.ExistsInPropIterator = true;
        enumProp.PropID = param.id;
        enumProp.Hint = _localize.GetDescriptionTranslation(param.id, param.label, param.description);
        enumProp.IsStructural = new BooleanValueGetter(() => false);
        enumProp.PropIsExpandedGetter = new BooleanValueGetter(() => param.IsExpanded);
        enumProp.PropIsExpandedSetter = new BooleanValueSetter((v) => param.IsExpanded = v);
        enumProp.RestoreValueProc = new DefaultPropValue(delegate()
        {
            String userParam;
            if (_curaParameters.UserParameters.TryGetValue(param.id, out userParam))
            {
                RemoveUserParameter(param.id);
                _curaParameters.UpdateAllParameters();
            }
        });
        enumProp.Visible = new BooleanValueGetter(() => AcceptedByFilter(label) && IsParamVisible(param));
        foreach (EnumItem ei in param.options) 
        {
            var Caption = _localize.GetEnumsTranslation(param.id, param.label, ei.id, ei.name);
            enumProp.Add(ei.id, Caption, ""); 
        }
        enumProp.ValueGetter = new StringValueGetter(() => _curaParameters.GetValue(param.id, true));
        enumProp.ValueSetter = new StringValueSetter(delegate (string v)
        {
            _curaParameters.SetValue(param.id, v, true);
            AddUserParameter(param.id, v, true);  
            _curaParameters.UpdateAllParameters();           
        });
        return enumProp;
    }
  
    private IST_CustomProp GetComplexProp(Parameter param)
    {
        if (PropHelpers == null)
            throw new Exception("PropHelpers is null");
        var label = _localize.GetLabelTranslation(param.id, param.label);
        var complexProp = PropHelpers.CreateComplexProp(label)
            ?? throw new Exception("Failed to create ComplexProp for " + label);
        if (param.icon != null && param.icon != "")
            complexProp.IconFile = "$(SUPPLEMENT_FOLDER)\\operations\\TypeImages\\MeasuringItem.bmp";
        param.ExistsInPropIterator = true;
        complexProp.PropID = param.id;
        complexProp.PropIsExpandedGetter = new BooleanValueGetter(() => param.IsExpanded);
        complexProp.PropIsExpandedSetter = new BooleanValueSetter((v) => param.IsExpanded = v);
        complexProp.Visible = new BooleanValueGetter(() => IsParamVisible(param));
        return complexProp;
    }
    
    private void AddComplexProps(IST_SimplePropIterator simpleIterator, string filename)
    {
        if (!File.Exists(filename))
            return;
        var sr = new StreamReader(filename);
        var line = sr.ReadLine();
        while (line != null)
        {
            line = line.Trim();
            var line2 = sr.ReadLine();
            if (line.StartsWith("[") && line.EndsWith("]") && !line.Contains("dual"))
            {
                if (line2 != null && line2.Trim() != "" && !line2.Contains('='))
                {
                    line = line.Replace("[", "");
                    line = line.Replace("]", "");
                    if (_curaParameters.GlobalParams.TryGetValue(line, out var param))
                    {
                        var prop = GetComplexProp(param);
                        param.indProp = simpleIterator.AddNewProp(prop, -1);
                    }
                }
            }
            line = line2;
        }
    }
    
    private void AddProps(IST_SimplePropIterator simpleIterator, string filename)
    {
        if (!File.Exists(filename))
            return;
        var sr = new StreamReader(filename);
        var line = sr.ReadLine();
        while (line != null)
        {
            line = line.Trim();
            var line2 = sr.ReadLine();
            if (line != "" && !line.Contains('=') && _curaParameters.GlobalParams.TryGetValue(line, out var param))
            {
                var needAddParam = true;
                var parent = param.parent;
                while (parent != null)
                {
                    if (parent.id == "dual")
                    {
                        needAddParam = false;
                        break;
                    }
                    parent = parent.parent;
                }
                
                if (needAddParam)
                {
                    var prop = param.paramType switch
                    {
                        ParameterType.ptStr => GetStringProp(param),
                        ParameterType.ptArrayofInt => GetStringProp(param),
                        ParameterType.ptInt => GetIntegerProp(param),
                        ParameterType.ptFloat => GetFloatProp(param),
                        ParameterType.ptBool => GetBoolProp(param),
                        ParameterType.ptEnum => GetEnumProp(param),
                        _ => null
                    };
                    if (prop != null)
                        param.indProp = simpleIterator.AddNewProp(prop, param.parent?.indProp ?? -1);
                }
            }
            
            line = line2;
        }
    }
  
    private void AddChildrenProps(IST_SimplePropIterator simpleIterator, List<Parameter>? children, int parentIndex)
    {
        foreach (var childParam in children ?? [])
        {
            var prop = childParam.paramType switch
            {
                ParameterType.ptStr => GetStringProp(childParam),
                ParameterType.ptArrayofInt => GetStringProp(childParam),
                ParameterType.ptInt => GetIntegerProp(childParam),
                ParameterType.ptFloat => GetFloatProp(childParam),
                ParameterType.ptBool => GetBoolProp(childParam),
                ParameterType.ptEnum => GetEnumProp(childParam),
                _ => null
            };
            if (prop == null)
                continue;
            childParam.indProp = simpleIterator.AddNewProp(prop, parentIndex);
            if (childParam.HasChildren())
                AddChildrenProps(simpleIterator, childParam.children, childParam.indProp);
        }
    }
    
    public string GetGCodeCommandTranslation(string label = "")
    {
        var localLabel = "";
        if (label != "")
            localLabel = label + " CLData_Command";
        var caption = _localize.GetTranslation(localLabel);
        if (!string.IsNullOrEmpty(caption))
            return caption;
        if (label != "")
            caption = label;
        return caption;
    }
    
    private void AddUserParameter(string paramId, string value, bool isGlobalParameter)
    {
        _curaParameters.AddUserParameter(paramId, value, isGlobalParameter);
        SaveUserParameterToXml(OperationXmlProp);
    }
    
    private void RemoveUserParameter(string paramId)
    {
        _curaParameters.UserParameters.Remove(paramId);
        SaveUserParameterToXml(OperationXmlProp);
    }
    
    public void SaveUserParameterToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
        using var paramArrayCom = new ComWrapper<IST_XMLPropArray>(xmlProp.Arr["CuraUserParameterArray"]);
        var paramArray = paramArrayCom.Instance;
        if (paramArray == null)
            return;
        
        paramArray.Clear();
        foreach (var keyValueElement in _curaParameters.UserParameters)
        {
            using var pointCom = new ComWrapper<IST_XMLPropPointer>(paramArray.CreateNewItem("Parameter"));
            var point = pointCom.Instance;
            if (point == null)
                continue;
            point.Str["Name"] = keyValueElement.Key;
            point.Str["Value"] = keyValueElement.Value;
            paramArray.AddItem(point);
        }
    }

    /// <summary>
    /// Analyze all parameters and fill the property, that indicates if parameter is accepted by search filter
    /// </summary>
    public void FillAcceptedByFilter(string? value)
    {
        if (_searchFilter.Equals(value, StringComparison.CurrentCultureIgnoreCase))
            return;
        _searchFilter = value ?? "";
        
        foreach (var param in _curaParameters.GlobalParams.Values)
        {
            var label = _localize.GetLabelTranslation(param.id, param.label);
            param.IsAcceptedByFilter = AcceptedByFilter(label);
        }
    }
    
    public void SaveManufacturerToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
        if (xmlProp.Str["GeneralParameters.Manufacturer"] != null &&_curaParameters.SelectedMachineBrand != null)
            xmlProp.Str["GeneralParameters.Manufacturer"] = _curaParameters.SelectedMachineBrand.name;  
        else
            xmlProp.Str["GeneralParameters.Manufacturer"] = "";
    }
  
    public void SaveMachineToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
        if (xmlProp.Str["GeneralParameters.Machine"] != null && _curaParameters.SelectedMachine != null)
            xmlProp.Str["GeneralParameters.Machine"] = _curaParameters.SelectedMachine.id;  
        else
            xmlProp.Str["GeneralParameters.Machine"] = "";
    }

    public void SaveExtruderToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
        if (xmlProp.Str["GeneralParameters.Extruder"] != null &&_curaParameters.SelectedExtruder != null)
            xmlProp.Str["GeneralParameters.Extruder"] = _curaParameters.SelectedExtruder.id; 
        else
            xmlProp.Str["GeneralParameters.Extruder"] = "";
    }

    public void SaveMaterialBrandToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
        if (xmlProp.Str["GeneralParameters.MaterialBrand"] != null &&_curaParameters.SelectedMaterialBrand != null)
            xmlProp.Str["GeneralParameters.MaterialBrand"] = _curaParameters.SelectedMaterialBrand.name;    
        else
            xmlProp.Str["GeneralParameters.MaterialBrand"] = "";
    }

    public void SaveMaterialToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
        if (xmlProp.Str["GeneralParameters.Material"] != null &&_curaParameters.SelectedMaterial != null)
            xmlProp.Str["GeneralParameters.Material"] = _curaParameters.SelectedMaterial.id;    
        else
            xmlProp.Str["GeneralParameters.Material"] = "";
    }

    public void SaveVariantToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
        if (xmlProp.Str["GeneralParameters.Variant"] != null && _curaParameters.SelectedVariant != null)
            xmlProp.Str["GeneralParameters.Variant"] = _curaParameters.SelectedVariant.Name; 
        else
            xmlProp.Str["GeneralParameters.Variant"] = "";
    }

    public void SaveProfileToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
        if (xmlProp.Str["GeneralParameters.Profile"] != null && _curaParameters.SelectedIntentCategory != null)
            xmlProp.Str["GeneralParameters.Profile"] = _curaParameters.SelectedIntentCategory.IntentCategoryName;
        else    
            xmlProp.Str["GeneralParameters.Profile"] = "";
    }

    public void SaveQualityToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
        if (xmlProp.Str["GeneralParameters.Quality"] != null && _curaParameters.SelectedQuality != null)
            xmlProp.Str["GeneralParameters.Quality"] = _curaParameters.SelectedQuality.FileName;
        else    
            xmlProp.Str["GeneralParameters.Quality"] = "";
    }

    public void SaveShowCustomParametersToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
        xmlProp.Bol["GeneralParameters.ShowCustomParameters"] = _curaParameters.IsShowCustomParameters;
    }

    public void SaveSettingVisibilityToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
        if (xmlProp.Str["GeneralParameters.SettingVisibility"] != null && _curaParameters.SelectedSettingVisibilities!="")
            xmlProp.Str["GeneralParameters.SettingVisibility"] = _curaParameters.SelectedSettingVisibilities;
        else    
            xmlProp.Str["GeneralParameters.SettingVisibility"] = "";
    }

    public void SaveAutoToolParameterizationToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
      
        xmlProp.Bol["GeneralParameters.AutoToolParameterization"] = IsAutoToolParameterization;
        if (!_isOperationCreating || !IsAutoToolParameterization)
            return;
        _isOperationCreating = false;
        SetToolParameterization("layer_height");
        SetToolParameterization("line_width");
    }

    public void SaveFilamentExtrudingLengthToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
      
        xmlProp.Flt["GeneralParameters.FilamentExtrudingLength"] = FilamentExtrudingLength;
    }

    public void SaveOutputAdditionalParametersToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
      
        xmlProp.Bol["GeneralParameters.OutputAdditionalParameters"] = IsOutputAdditionalClDataParameters;
    }

    public void SaveOutputFilamentExtrudingToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
        xmlProp.Bol["GeneralParameters.OutputFilamentExtruding"] = IsOutputFilamentExtruding;
    }

    public void SaveToolpathParsingModeToXml(IST_XMLPropPointer xmlProp)
    {
        if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            return;
  
         if (xmlProp.Str["GeneralParameters.ToolpathParsingMode"] == null)
            xmlProp.Str["GeneralParameters.ToolpathParsingMode"] = Tpm == ToolpathParsingMode.tpmSimplified ? "Simplified" : "GCodeBased";
    }
}