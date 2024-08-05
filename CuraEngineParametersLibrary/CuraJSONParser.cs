using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
namespace CuraEngineParametersLibrary;
public class CuraSettingsJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("settings")]
    public SettingsJson Settings { get; set; }
}

public class SettingsJson
{
    [JsonPropertyName("PathToCura")]
    public string PathToCura { get; set; }
}
public class CuraJSONParser
{
    private Dictionary<string, Parameter> GlobalParams;
    private Dictionary<string, Parameter> ExtruderParams;
    public Dictionary<string, string> MachineMetadatas;

    public List<string> ExcludedMaterials;
    public List<string> MachineInherites;

    public string CuraPath = "";
    public CuraJSONParser(string pathToCura)
    {
        GlobalParams = new Dictionary<string, Parameter>();
        ExtruderParams = new Dictionary<string, Parameter>();
        MachineMetadatas = new Dictionary<string, string>();
        ExcludedMaterials = new List<string>();
        MachineInherites = new List<string>();
        CuraPath = pathToCura;
    }
    private void AddParam(string name, bool IsGlobalParams, string ParentName)
    {
        var parameter = new Parameter();
        var ParentParameter = new Parameter();
        if (IsGlobalParams)
        {
            if (!GlobalParams.TryGetValue(name, out parameter))
            {
                GlobalParams[name] = new Parameter{id = name};
                if (GlobalParams.TryGetValue(ParentName, out ParentParameter))
                {
                    GlobalParams[name].parent = ParentParameter; 
                    ParentParameter.AddChild(GlobalParams[name]);
                }   
            }
        }
        else
        {
            if (!ExtruderParams.TryGetValue(name, out parameter))
            {
                ExtruderParams[name] = new Parameter{id = name};
                if (ExtruderParams.TryGetValue(ParentName, out ParentParameter))
                {
                    ExtruderParams[name].parent = ParentParameter;
                    ParentParameter.AddChild(ExtruderParams[name]); 
                } 
            }
        }
    }
    private void AddValue(string name, string value, bool IsGlobalParams)
    {
        value = ValueFormatter.FormatValue(value);

        if (IsGlobalParams)
        {
            GlobalParams[name].value = value;
            GlobalParams[name].OriginalValue = value;
        }
        else
        {
            ExtruderParams[name].value = value; 
            ExtruderParams[name].OriginalValue = value;
        }
    }
    private void AddDescription(string name, string value, bool IsGlobalParams)
    {
        value = Regex.Unescape(value);
        if (IsGlobalParams)
        {
            GlobalParams[name].description = value;
        }
        else
        {
            ExtruderParams[name].description = value; 
        }
    }
    private void AddLabel(string name, string value, bool IsGlobalParams)
    {
        if (IsGlobalParams)
        {
            GlobalParams[name].label = value;

        }
        else
        {
            ExtruderParams[name].label = value; 
        }
    }
    private void AddIcon(string name, string value, bool IsGlobalParams)
    {
        if (IsGlobalParams)
        {
            GlobalParams[name].icon = value;

        }
        else
        {
            ExtruderParams[name].icon = value; 
        }
    }
    private void AddUnit(string name, string value, bool IsGlobalParams)
    {  
        value = value.Replace("\"", "");
        value = Regex.Unescape(value);

        if (IsGlobalParams)
        {
            GlobalParams[name].unit = value;
        }
        else
        {
            ExtruderParams[name].unit = value; 
        }
    }
    private ParameterType GetType(string value)
    {
        value = value.Replace("\"", "");
        if (value == "str")
        {
            return ParameterType.ptStr;
        }
        else if (value == "bool")
        {
            return ParameterType.ptBool;
        }
        else if (value == "float")
        {
            return ParameterType.ptFloat;
        }
        else if (value == "enum")
        {
            return ParameterType.ptEnum;
        }
        else if (value == "int")
        {
            return ParameterType.ptInt;
        }
        else if (value == "[int]")
        {
            return ParameterType.ptArrayofInt;
        }
        else if (value == "polygon")
        {
            return ParameterType.ptPolygon;
        }
        else if (value == "polygons")
        {
            return ParameterType.ptPolygons;
        }
        else if (value == "optional_extruder")
        {
            return ParameterType.ptOptionalExtruder;
        }
        else if (value == "extruder")
        {
            return ParameterType.ptExtruder;
        }
        else if (value == "category")
        {
            return ParameterType.ptCategory;
        }
        else
        {
            return ParameterType.ptStr;
        }
    }
    private void AddType(string name, string value, bool IsGlobalParams)
    {
        var pt = GetType(value);
        if (IsGlobalParams)
        {
            GlobalParams[name].paramType = pt;
            if (pt==ParameterType.ptCategory)
                GlobalParams[name].IsExpanded = false;
        }
        else
        {
            ExtruderParams[name].paramType = pt; 
            if (pt==ParameterType.ptCategory)
                ExtruderParams[name].IsExpanded = false;
        }
    }
    private void AddEnabled(string name, string value, bool IsGlobalParams)
    {
        value = ValueFormatter.FormatValue(value);

        if (IsGlobalParams)
        {
            GlobalParams[name].enabled = value;
        }
        else
        {
            ExtruderParams[name].enabled = value; 
        }
    }
    private void AddMinimumValue(string name, string value, bool IsGlobalParams)
    {
        value = ValueFormatter.FormatValue(value);

        if (IsGlobalParams)
        {
            GlobalParams[name].SetMinimumValue(value);
        }
        else
        {
            ExtruderParams[name].SetMinimumValue(value); 
        }
    }
    private void AddMaximumValue(string name, string value, bool IsGlobalParams)
    {
        value = ValueFormatter.FormatValue(value);

        if (IsGlobalParams)
        {
            GlobalParams[name].SetMaximumValue(value);
        }
        else
        {
            ExtruderParams[name].SetMaximumValue(value); 
        }
    }
    private void AddMinimumValueWarning(string name, string value, bool IsGlobalParams)
    {
        value = ValueFormatter.FormatValue(value);

        if (IsGlobalParams)
        {
            GlobalParams[name].SetMinimumValueWarning(value);
        }
        else
        {
            ExtruderParams[name].SetMinimumValueWarning(value); 
        }
    }
    private void AddMaximumValueWarning(string name, string value, bool IsGlobalParams)
    {
        value = ValueFormatter.FormatValue(value);

        if (IsGlobalParams)
        {
            GlobalParams[name].SetMaximumValueWarning(value);
        }
        else
        {
            ExtruderParams[name].SetMaximumValueWarning(value); 
        }
    }
    private void AddOptions(string name, JsonElement optionsElement, bool IsGlobalParams)
    {
        foreach (JsonProperty childProperty in optionsElement.EnumerateObject())
        {   
            if (optionsElement.TryGetProperty(childProperty.Name, out JsonElement ChildElement))
            {
                EnumItem enumItem = new EnumItem(childProperty.Name, childProperty.Value.ToString());        
                if (IsGlobalParams)
                {
                    GlobalParams[name].AddEnumItem(enumItem);
                }
                else
                {
                    ExtruderParams[name].AddEnumItem(enumItem);
                }
            }
        }   
    }
    private void FillProperties(JsonProperty childProperty, JsonElement ChildElement, bool IsGlobalParams, string ParentName)
    {
        var name = childProperty.Name;
        var value = "";
        AddParam(name, IsGlobalParams, ParentName);
        if (ChildElement.TryGetProperty("value", out JsonElement DefaultValuElement))
        {
            value = ChildElement.GetProperty("value").GetRawText();
            AddValue(name, value, IsGlobalParams);
        }
        else if (ChildElement.TryGetProperty("default_value", out JsonElement Valuelement))
        {
            value = ChildElement.GetProperty("default_value").GetRawText();
            AddValue(name, value, IsGlobalParams);
        }
        if (ChildElement.TryGetProperty("description", out JsonElement descriptionElement))
        {
            value = ChildElement.GetProperty("description").GetRawText();
            AddDescription(name, value, IsGlobalParams);
        }
        if (ChildElement.TryGetProperty("label", out JsonElement labelElement))
        {
            value = ChildElement.GetProperty("label").GetRawText();
            value = value.Replace("\"", "");
            AddLabel(name, value, IsGlobalParams);
        }
        if (ChildElement.TryGetProperty("icon", out JsonElement iconElement))
        {
            value = ChildElement.GetProperty("icon").GetRawText();
            value = value.Replace("\"", "");
            AddIcon(name, value, IsGlobalParams);
        }
        if (ChildElement.TryGetProperty("unit", out JsonElement unitElement))
        {
            value = ChildElement.GetProperty("unit").GetRawText();
            AddUnit(name, value, IsGlobalParams);
        }
        if (ChildElement.TryGetProperty("type", out JsonElement typeElement))
        {
            value = ChildElement.GetProperty("type").GetRawText();
            AddType(name, value, IsGlobalParams);
        }
        if (ChildElement.TryGetProperty("enabled", out JsonElement enabledElement))
        {
            value = ChildElement.GetProperty("enabled").GetRawText();
            AddEnabled(name, value, IsGlobalParams);
        }
        if (ChildElement.TryGetProperty("minimum_value", out JsonElement minimumValueElement))
        {
            value = ChildElement.GetProperty("minimum_value").GetRawText();
            AddMinimumValue(name, value, IsGlobalParams);
        }
        if (ChildElement.TryGetProperty("maximum_value", out JsonElement maximumValueElement))
        {
            value = ChildElement.GetProperty("maximum_value").GetRawText();
            AddMaximumValue(name, value, IsGlobalParams);
        }
        if (ChildElement.TryGetProperty("minimum_value_warning", out JsonElement minimumValueWarningElement))
        {
            value = ChildElement.GetProperty("minimum_value_warning").GetRawText();
            AddMinimumValueWarning(name, value, IsGlobalParams);
        }
        if (ChildElement.TryGetProperty("maximum_value_warning", out JsonElement maximumValueWarningElement))
        {
            value = ChildElement.GetProperty("maximum_value_warning").GetRawText();
            AddMaximumValueWarning(name, value, IsGlobalParams);
        }
        if (ChildElement.TryGetProperty("options", out JsonElement optionsElement))
        {
            AddOptions(name, optionsElement, IsGlobalParams);
        }
        if (IsGlobalParams)
        {
            GlobalParams[name].IsGlobalParameter = IsGlobalParams;
        }
        else
        {
            ExtruderParams[name].IsGlobalParameter = IsGlobalParams;
        }
    }
    private bool GetChildrenParams(JsonElement ChildrenElement, bool IsGlobalParams, string ParentName)
    {
        foreach (JsonProperty childProperty in ChildrenElement.EnumerateObject())
        {   
            if (ChildrenElement.TryGetProperty(childProperty.Name, out JsonElement ChildElement))
            {
                FillProperties(childProperty, ChildElement, IsGlobalParams, ParentName);
                if (ChildElement.TryGetProperty("children", out JsonElement ChildrenNode))
                {
                    GetChildrenParams(ChildrenNode, IsGlobalParams, childProperty.Name);
                }  
            }
        }     
        return true;
    }
    private bool ReadParams(string JSONFile, bool IsGlobalParams)
    {
        string jsonContent = File.ReadAllText(JSONFile);
        JsonDocument document = JsonDocument.Parse(jsonContent);
        JsonElement root = document.RootElement;

        if (root.TryGetProperty("inherits", out JsonElement InheritElement))  
        {
            var inheritName = root.GetProperty("inherits").GetString();
            var inheritpath = "";
            if (inheritName == "fdmextruder")
                inheritpath = Path.GetDirectoryName(JSONFile) + "\\..\\definitions\\" + inheritName + ".def.json";
            else
                inheritpath = Path.GetDirectoryName(JSONFile) + "\\" + inheritName + ".def.json";
            ReadParams(inheritpath, IsGlobalParams);
            if (IsGlobalParams)
                MachineInherites.Add(inheritName);
        }

        if (root.TryGetProperty("name", out JsonElement name))  
        {
            var value = name.GetRawText().Replace("\"", "");
            MachineMetadatas["name"] = value;
        }

        if (root.TryGetProperty("metadata", out JsonElement metadatas))  
        {
            foreach (JsonProperty childProperty in metadatas.EnumerateObject())
            {   if (childProperty.Name=="exclude_materials")
                {
                    var value = childProperty.Value.GetRawText().Replace("\"", "");
                    value = value.Replace("\r\n", "");
                    value = value.Replace("[", "");
                    value = value.Replace("]", "");
                    string[] materialArray = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string material in materialArray)
                    {
                        ExcludedMaterials.Add(material.Trim()); // Remove spaces at the beginning and end of each line
                    }
                }
                else
                {
                    var value = childProperty.Value.GetRawText().Replace("\"", "");
                    MachineMetadatas[childProperty.Name] = value;
                }           
            }
        }

        if (root.TryGetProperty("settings", out JsonElement settings))  
        {
            foreach (JsonProperty childProperty in settings.EnumerateObject())
            {   
                if (settings.TryGetProperty(childProperty.Name, out JsonElement ChildElement))
                {
                    if (ChildElement.TryGetProperty("children", out JsonElement ChildrenElement))
                    {   
                        FillProperties(childProperty, ChildElement, IsGlobalParams, "");
                        GetChildrenParams(ChildrenElement, IsGlobalParams, childProperty.Name);
                    }     
                }        
            }
        }
        if (root.TryGetProperty("overrides", out JsonElement overrides))  
        {
            foreach (JsonProperty childProperty in overrides.EnumerateObject())
            {   
                var ChildElement = overrides.GetProperty(childProperty.Name);
                FillProperties(childProperty, ChildElement, IsGlobalParams, "");
            }
        }

        return true;
    }
    public void ParseJSONFile(string JSONFile, bool IsGlobalParams, out Dictionary<string, Parameter> parameters)
    {
        ReadParams(JSONFile, IsGlobalParams);
        if (IsGlobalParams)
        {
            var MachineID = Path.GetFileName(JSONFile).Replace(".def.json", "");
            MachineMetadatas.Add("id", MachineID);
            parameters = GlobalParams;
        }   
        else
        {
            parameters = ExtruderParams;
        }
    }
    // private bool ReadMetadatas(string JSONFile)
    // {
    //     string jsonContent = File.ReadAllText(JSONFile);
    //     JsonDocument document = JsonDocument.Parse(jsonContent);
    //     JsonElement root = document.RootElement;

    //     if (root.TryGetProperty("name", out JsonElement name))  
    //     {
    //         var value = name.GetRawText().Replace("\"", "");
    //         MachineMetadatas.Add("name", value);
    //     }
    //     if (root.TryGetProperty("metadata", out JsonElement metadata))  
    //     {
    //         foreach (JsonProperty childProperty in metadata.EnumerateObject())
    //         {   
    //             var value = childProperty.Value.GetRawText().Replace("\"", "");
    //             MachineMetadatas.Add(childProperty.Name, value);
    //         }
    //     }
    //     return true;
    // }
    // public void ParseMetadataJSONFile(string JSONFile, out Dictionary<string, string> metadatas)
    // {
    //     ReadMetadatas(JSONFile);
    //     var MachineID = Path.GetFileName(JSONFile).Replace(".def.json", "");
    //     MachineMetadatas.Add("id", MachineID);
    //     metadatas = MachineMetadatas;
    // }
    private Extruder GetExtruderInfo(string extrId)
    {
        Extruder extr = new Extruder();
        extr.id = extrId;
        string extrJsonContent = "";
        if (extrId == "fdmextruder")
            extrJsonContent = File.ReadAllText(CuraPath + "share\\cura\\resources\\definitions\\"+extrId+".def.json");
        else
            extrJsonContent = File.ReadAllText(CuraPath + "share\\cura\\resources\\extruders\\"+extrId+".def.json");
        JsonDocument extrDocument = JsonDocument.Parse(extrJsonContent);
        JsonElement extrRoot = extrDocument.RootElement;
        if (extrRoot.TryGetProperty("name", out JsonElement extrNameElement))  
        {
            var extrName = extrNameElement.GetRawText().Replace("\"", "");
            extr.name = extrName;
        }
        return extr;
    }
    private List<Extruder> GetExtrudersFromInherit(string inheritName)
    {
        List<Extruder> Extruders = null;
        var JsonContent = File.ReadAllText(CuraPath + "share\\cura\\resources\\definitions\\"+inheritName+".def.json");
        JsonDocument document = JsonDocument.Parse(JsonContent);
        JsonElement root = document.RootElement;
        if (root.TryGetProperty("metadata", out JsonElement metadata))  
        {
            if (metadata.TryGetProperty("machine_extruder_trains", out JsonElement extrudersElement))  
            {
                Extruders = new List<Extruder>();
                foreach (JsonProperty childProperty in extrudersElement.EnumerateObject())
                {  
                    var value = childProperty.Value.GetRawText().Replace("\"", "");
                    var extruderJSONFile = "";
                    if (value == "fdmextruder")
                        extruderJSONFile = CuraPath + "share\\cura\\resources\\definitions\\"+value+".def.json";
                    else
                        extruderJSONFile = CuraPath + "share\\cura\\resources\\extruders\\"+value+".def.json";
                    //for some reason the extruder file may not exist
                    if (File.Exists(extruderJSONFile))
                    {
                        Extruder extr = GetExtruderInfo(value);
                        Extruders.Add(extr);
                    }   
                }
                return Extruders;
            }
            else if (root.TryGetProperty("inherits", out JsonElement inheritsElement))  
            {
                var value = inheritsElement.GetRawText().Replace("\"", "");
                Extruders = GetExtrudersFromInherit(value);            
            }
        }
        return Extruders;
    }
    public List<MachineInfo> GetAllMachinesFromJSONFiles()
    {
        var MachinesInfos = new List<MachineInfo>();
        string rootDirectory = CuraPath + "share\\cura\\resources\\definitions";
        if (Directory.Exists(rootDirectory))
        {
            string[] cfgFiles = Directory.GetFiles(rootDirectory, "*.def.json", SearchOption.AllDirectories);
            foreach (string filePath in cfgFiles)
            {
                var machInfo = new MachineInfo
                {
                    id = Path.GetFileName(filePath).Replace(".def.json", "")
                };
                string jsonContent = File.ReadAllText(filePath);
                JsonDocument document = JsonDocument.Parse(jsonContent);
                JsonElement root = document.RootElement;
                
                if (root.TryGetProperty("name", out JsonElement name))  
                {
                    var value = name.GetRawText().Replace("\"", "");
                    machInfo.name = value;
                }
                if (root.TryGetProperty("inherits", out JsonElement inheriteElement))  
                {
                    var value = inheriteElement.GetRawText().Replace("\"", "");  
                    machInfo.Inherited = value;
                }
                if (root.TryGetProperty("metadata", out JsonElement metadata))  
                {
                    if (metadata.TryGetProperty("visible", out JsonElement visibleElement))  
                    {
                        var value = visibleElement.GetRawText().Replace("\"", "");
                        if (value.ToLower()=="false")
                            machInfo.isVisible = false;
                    }
                    else
                    {
                        machInfo.isVisible = false;
                    }
                    if (metadata.TryGetProperty("manufacturer", out JsonElement manufElement))  
                    {
                        var value = manufElement.GetRawText().Replace("\"", "");  
                        machInfo.Manufacturer = value;
                    }
                    if (metadata.TryGetProperty("machine_extruder_trains", out JsonElement extrudersElement))  
                    {
                        foreach (JsonProperty childProperty in extrudersElement.EnumerateObject())
                        {  
                            var value = childProperty.Value.GetRawText().Replace("\"", "");
                            var extruderJSONFile = "";
                            if (value == "fdmextruder")
                                extruderJSONFile = CuraPath + "share\\cura\\resources\\definitions\\"+value+".def.json";
                            else
                                extruderJSONFile = CuraPath + "share\\cura\\resources\\extruders\\"+value+".def.json";
                            //for some reason the extruder file may not exist
                            if (File.Exists(extruderJSONFile))
                            {
                                Extruder extr = GetExtruderInfo(value);
                                machInfo.Extruders.Add(extr);
                            }
                        }
                    }
                    else if (machInfo.Inherited!="")  
                    {
                        List<Extruder> extruders = GetExtrudersFromInherit(machInfo.Inherited);
                        if (extruders != null)
                        {
                            machInfo.Extruders = extruders;
                        }
                    }
                }
                MachinesInfos.Add(machInfo);
            }
        }

        return MachinesInfos;
    }
}
