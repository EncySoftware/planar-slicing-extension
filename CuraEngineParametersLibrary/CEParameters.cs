using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
namespace CuraEngineParametersLibrary;
using System.Globalization;
public class Parameters
{
    //Global parameters of selected machine which have linked parameters with containers (Variants, Qualities, Intents)
    //These parameters are used for toolpath calculation
    public Dictionary<string, Parameter> GlobalParams; 

    //Parameters of selected extruder on selected machine which have linked parameters with containers (Variants, Qualities, Intents)
    //These parameters are used for toolpath calculation
    public Dictionary<string, Parameter> ExtruderParams; 

    public Dictionary<string, string> UserParameters;
    public List<MachineInfo> MachinesList; //list of all available machines
    public Dictionary<string, string> MachineMetadatas; //metadata of selected machine
    public List<Container> Variants;  //all variants for selected machine
    public List<Container> Qualities; //all qualities for selected machine
    public List<Container> Intents;   //all intents for selected machine
    public List<Material> Materials;    //all materials
    public Container SelectedVariant;  
    public Container SelectedQuality;  
    public Container SelectedIntent;  
    public Material SelectedMaterial;  
    public MachineInfo SelectedMachine;
    public Extruder SelectedExtruder;
    public List<IntentCategory> IntentCategories;
    public IntentCategory SelectedIntentCategory;  
    public List<string> SettingVisibilities;
    public string SelectedSettingVisibilities;
    public bool IsShowCustomParameters;
    public List<MachineBrand> MachinesBrands;
    public MachineBrand SelectedMachineBrand;
    public List<MaterialBrand> MaterialsBrands;
    public MaterialBrand SelectedMaterialBrand;
    public List<string> ExcludedMaterials;
    public List<string> MachineInherites;

    public string CuraPath = "";
    public Parameters()
    {
        Variants = new List<Container>();
        Qualities = new List<Container>();
        Intents = new List<Container>();
        Materials = new List<Material>();
        IntentCategories = new List<IntentCategory>();
        MachinesList = new List<MachineInfo>();
        UserParameters = new Dictionary<string, string>();
        SettingVisibilities = new List<string>();
        IsShowCustomParameters = false;
        MachinesBrands = new List<MachineBrand>();
        MaterialsBrands = new List<MaterialBrand>();
        ExcludedMaterials = new List<string>();
        MachineInherites = new List<string>();
    }
    public void GetSettingVisibilities()
    {
        string[] cfgFiles = Directory.GetFiles(CuraPath + "share\\cura\\resources\\setting_visibility\\", "*.cfg", SearchOption.AllDirectories);
        foreach (string filePath in cfgFiles)
        {
            var name = Path.GetFileNameWithoutExtension(filePath);
            if (name=="basic")
                SettingVisibilities.Insert(0, name);
            else
                SettingVisibilities.Add(Path.GetFileNameWithoutExtension(filePath));
        }
        SettingVisibilities.Add("all");
        SelectedSettingVisibilities = SettingVisibilities[0];
    }
    private void AddAdditionalParams() 
    {
        // parameters that may be missing, but CuraEngine requires (some parameters are in the extruder, but required in global ones)
        if (!GlobalParams.ContainsKey("machine_extruder_start_code_duration"))
        {
            var newParameter = new Parameter();
            newParameter.value = "0";
            GlobalParams["machine_extruder_start_code_duration"] = newParameter;
        }
        if (!GlobalParams.ContainsKey("machine_nozzle_offset_x"))
        {
            var newParameter = new Parameter();
            newParameter.value = "0";
            GlobalParams["machine_nozzle_offset_x"] = newParameter;
        }
        if (!GlobalParams.ContainsKey("machine_nozzle_offset_y"))
        {
            var newParameter = new Parameter();
            newParameter.value = "0";
            GlobalParams["machine_nozzle_offset_y"] = newParameter;
        }
        if (!GlobalParams.ContainsKey("machine_extruder_cooling_fan_number"))
        {
            var newParameter = new Parameter();
            newParameter.value = "1";
            GlobalParams["machine_extruder_cooling_fan_number"] = newParameter;
        }
        if (!GlobalParams.ContainsKey("machine_extruder_start_code"))
        {
            var newParameter = new Parameter();
            newParameter.value = "T1";
            GlobalParams["machine_extruder_start_code"] = newParameter;
        }
        if (!GlobalParams.ContainsKey("brim_outside_only"))
        {
            var newParameter = new Parameter();
            newParameter.value = "True";
            GlobalParams["brim_outside_only"] = newParameter;
        }
        foreach (KeyValuePair<string, Parameter> keyValueElement in ExtruderParams)
        {
            if (!GlobalParams.ContainsKey(keyValueElement.Key))
            {
                GlobalParams[keyValueElement.Key] = keyValueElement.Value;
            }
        }
        // if (!GlobalParams.ContainsKey("machine_extruder_start_pos_y") && ExtruderParams.ContainsKey("machine_extruder_start_pos_y"))
        // {
        //     GlobalParams["machine_extruder_start_pos_y"] = ExtruderParams["machine_extruder_start_pos_y"];
        // }
        // if (!GlobalParams.ContainsKey("machine_extruder_start_pos_y") && ExtruderParams.ContainsKey("machine_extruder_start_pos_y"))
        // {
        //     GlobalParams["machine_extruder_start_pos_y"] = ExtruderParams["machine_extruder_start_pos_y"];
        // }
        //removed settings from Cura
        if (!GlobalParams.ContainsKey("support_interface_skip_height"))
        {
            var sishParam = new Parameter();
            sishParam.value = "0.2";
            GlobalParams["support_interface_skip_height"] = sishParam;
        }

        // disabling parameters for dual extrusion
        if (GlobalParams.ContainsKey("prime_tower_enable"))
        {
            GlobalParams["prime_tower_enable"].value = "false";
        }

        if (GlobalParams.ContainsKey("prime_tower_mode"))
        {
            GlobalParams["prime_tower_mode"].value = "normal";
        }

        if (GlobalParams.ContainsKey("ooze_shield_enabled"))
        {
            GlobalParams["ooze_shield_enabled"].value = "false";
        }
        
        //set the parameter to create the toolpath correctly
        if (GlobalParams.ContainsKey("machine_center_is_zero"))
        {
            GlobalParams["machine_center_is_zero"].value = "true";
        }
    }
    private void AddFDMInstances(string InstanceName)
    {
        string rootDirectory = CuraPath + "share\\cura\\resources\\"+InstanceName;
        if (Directory.Exists(rootDirectory))
        {
            string[] cfgFiles = Directory.GetFiles(rootDirectory, "*.cfg", SearchOption.TopDirectoryOnly);
            string line;
            foreach (string filePath in cfgFiles)
            {
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(filePath);
                    Container con = new Container(InstanceName);
                    con.FileName = Path.GetFileName(filePath).Replace(".inst.cfg", "");
                    line = sr.ReadLine();
                    var SectionName = "";
                    var isCorrectFile = true;
                    while (line != null)
                    {
                        line = line.Trim();
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            SectionName = line;
                            SectionName = SectionName.Replace("[", "");
                            SectionName = SectionName.Replace("]", "");
                        }
                        else if (line != "" && line.Contains("="))
                        {
                            var ind = line.IndexOf("=");
                            var paramName = line.Substring(0, ind).Trim();
                            var paramValue = line.Substring(ind+1, line.Length-ind-1).Trim();
                            if (paramName=="definition" && paramValue!="fdmprinter")
                            {
                                isCorrectFile = false;
                                break;
                            }
                            if (paramName=="name")
                            {
                                var cap = paramValue;
                                if (InstanceName=="intent")
                                {
                                    if (cap=="Accurate")
                                        cap = "Engineering";
                                    if (cap=="Quick")
                                        cap = "Draft"; 
                                }                                
                                con.Name = paramValue; 
                                con.Caption = cap; 
                            }   
                            con.isGlobal = true;                                          
                            con.AddParameter(SectionName, paramName, paramValue);
                        }
                        line = sr.ReadLine();
                    }
                    if (isCorrectFile)
                    {
                        if (InstanceName=="quality")
                            Qualities.Add(con);
                        else if (InstanceName=="intent")
                            Intents.Add(con);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(filePath + ":" +  ex.Message);
                }
                finally
                {
                    if (sr!=null)
                        sr.Close();
                }
            }
        }
    }
    private string GetCorrectInstacesFolder(string InstanceName, string MachineID)
    {
        string instanceFolder = ""; 
        string rootDirectory = CuraPath + "share\\cura\\resources\\"+InstanceName;
        if (Directory.Exists(rootDirectory))
        {
            string[] folders = Directory.GetDirectories(rootDirectory, "*", SearchOption.AllDirectories);   
            foreach (string folder in folders)
            {
                if (folder.Contains(MachineID))
                {
                    instanceFolder = folder;
                    break;
                }
            }
 
            if (instanceFolder=="")
            {
                folders = Directory.GetDirectories(rootDirectory, "*", SearchOption.TopDirectoryOnly);  
                var shortID = MachineID;
                var ind = shortID.IndexOf("_");
                if (ind>0)
                    shortID = shortID.Substring(0, ind);

                foreach (string folder in folders)
                {
                    var folderName = Path.GetFileName(folder);
                    if (folderName.ToLower()==shortID.ToLower())
                    {
                        instanceFolder = folder;
                        break;
                    }
                }
            }
        }
        return instanceFolder;
    }
    private bool UpdateInstanceContainer(string InstanceName, string MachineID, bool addOnlyNewInstances = false)
    {
        if (!addOnlyNewInstances)
        {
            if (InstanceName=="quality")
                Qualities.Clear();
            else if (InstanceName=="intent")
                Intents.Clear();
            AddFDMInstances(InstanceName);  
        }
        
        var isCorrect = false;
        var instanceFolder = GetCorrectInstacesFolder(InstanceName, MachineID);
        if (instanceFolder!="" && Directory.Exists(instanceFolder))
        {
            string[] cfgFiles = Directory.GetFiles(instanceFolder, "*.cfg", SearchOption.AllDirectories);
            string line;
            foreach (string filePath in cfgFiles)
            {
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(filePath);
                    Container con = new Container(InstanceName);
                    con.FileName = Path.GetFileName(filePath).Replace(".inst.cfg", "");
                    line = sr.ReadLine();
                    var SectionName = "";
                    var isCorrectFile = true;
                    while (line != null)
                    {
                        line = line.Trim();
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            SectionName = line;
                            SectionName = SectionName.Replace("[", "");
                            SectionName = SectionName.Replace("]", "");
                        }
                        else if (line != "" && line.Contains("="))
                        {
                            var ind = line.IndexOf("=");
                            var paramName = line.Substring(0, ind).Trim();
                            var paramValue = line.Substring(ind+1, line.Length-ind-1).Trim();
                            if (paramName=="definition" && paramValue!=MachineID && paramValue!="fdmprinter")
                            {
                                isCorrectFile = false;
                                break;
                            }
                            if (paramName=="name")
                            {
                                var cap = paramValue;
                                if (InstanceName=="intent")
                                {
                                    if (cap=="Accurate")
                                        cap = "Engineering";
                                    if (cap=="Quick")
                                        cap = "Draft"; 
                                }                                
                                con.Name = paramValue; 
                                con.Caption = cap; 
                            }   
                            if (paramName=="global_quality")
                            {
                                var isGlobalValue = false;
                                if (bool.TryParse(paramValue.ToLower(), out isGlobalValue))
                                    con.isGlobal = isGlobalValue;
                            }
                                        
                            con.AddParameter(SectionName, paramName, paramValue);
                        }
                        line = sr.ReadLine();
                    }
                    if (isCorrectFile)
                    {
                        if (InstanceName=="quality")
                        {
                            Qualities.Add(con);
                            isCorrect = true;
                        }
                        else if (InstanceName=="intent")
                        {
                            Intents.Add(con);
                            isCorrect = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(filePath + ":" +  ex.Message);
                }
                finally
                {
                    if (sr!=null)
                        sr.Close();
                }
            }
        }
        return isCorrect;
    }
    private void UpdateVariantInstanceContainer()
    {
        Variants.Clear();
        string rootDirectory = CuraPath + "share\\cura\\resources\\variants";

        if (Directory.Exists(rootDirectory))
        {
            string[] cfgFiles = Directory.GetFiles(rootDirectory, "*.cfg", SearchOption.AllDirectories);
            string line;
            foreach (string filePath in cfgFiles)
            {
                var fileName = Path.GetFileName(filePath).Replace(".inst.cfg", "");
                if (fileName.Contains(MachineMetadatas["id"]))
                {
                    StreamReader sr = null;
                    try
                    {
                        sr = new StreamReader(filePath);
                        Container con = new Container("variants");
                        con.FileName = fileName;
                        line = sr.ReadLine();
                        var SectionName = "";
                        var isCorrectFile = true;
                        while (line != null)
                        {
                            line = line.Trim();
                            if (line.StartsWith("[") && line.EndsWith("]"))
                            {
                                SectionName = line;
                                SectionName = SectionName.Replace("[", "");
                                SectionName = SectionName.Replace("]", "");
                            }
                            else if (line != "" && line.Contains("="))
                            {
                                var ind = line.IndexOf("=");
                                var paramName = line.Substring(0, ind).Trim();
                                var paramValue = line.Substring(ind+1, line.Length-ind-1).Trim();
                                if (paramName=="definition" && paramValue!=MachineMetadatas["id"])
                                {
                                    isCorrectFile = false;
                                    break;
                                }
                                if (paramName=="hardware_type" && paramValue=="buildplate")
                                {
                                    isCorrectFile = false;
                                    break;
                                }
                                if (paramName=="name")
                                {
                                    var cap = paramValue;                           
                                    con.Name = paramValue; 
                                    con.Caption = cap; 
                                }                                               
                                con.AddParameter(SectionName, paramName, paramValue);
                            }
                            line = sr.ReadLine();
                        }
                        if (isCorrectFile)
                        {
                            Variants.Add(con);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(filePath + ":" +  ex.Message);
                    }
                    finally
                    {
                        if (sr!=null)
                            sr.Close();
                    }
                }
            }
        }
        if (Variants.Count>1)
            Variants.Sort((v1, v2) => v1.Name.CompareTo(v2.Name));
    }
    public void SetSelectedVariant(string variantName, bool IsUpdateIntentCategories = true)
    {
        for (int i=0; i<Variants.Count; i++)
        {
            var varName = "";
            var variant = Variants[i];
            if (variant.Name==variantName)
            {
                SelectedVariant = variant;
                if (IsUpdateIntentCategories)
                    UpdateIntentCategories();
                return;
            }
        }
        if (Variants.Count>0)
        {
            SelectedVariant = Variants[0];
            if (IsUpdateIntentCategories)
                UpdateIntentCategories();
        }
    }
    public void UpdateVariants()
    {
        UpdateVariantInstanceContainer();
        var varName = "";
        if (MachineMetadatas.TryGetValue("preferred_variant_name", out varName))
        {
            SetSelectedVariant(varName, false);
        }
        else
        {
            if (Variants.Count>0)
                SelectedVariant = Variants[0];
            else
                SelectedVariant = null;    
        }
    }   
    public bool UpdateIntents(string MachineID, bool addOnlyNewInstances = false)
    {
        return UpdateInstanceContainer("intent", MachineID, addOnlyNewInstances);
    }    
    private void UpdateQuailitiesCaptions()
    {
        for (var i=0; i<Qualities.Count; i++)
        {
            if (Qualities[i].isGlobal)
            {
                var lh = "";
                if (Qualities[i].ValuesSection.TryGetValue("layer_height", out lh))
                {
                    Qualities[i].Caption = Qualities[i].Name + " - " + lh + "mm"; 
                }
                else if (Qualities[i].Name=="Fine")
                {
                    Qualities[i].ValuesSection["layer_height"] = "0.1";
                    Qualities[i].Caption = Qualities[i].Name + " - 0.1mm"; 
                }
            }
            else
            {
                var qualityType1 = "";
                if (Qualities[i].MetadataSection.TryGetValue("quality_type", out qualityType1))
                {
                    for (var j=0; j<Qualities.Count; j++)
                    {
                        var qualityType2 = "";
                        if (i!=j && Qualities[j].MetadataSection.TryGetValue("quality_type", out qualityType2) && qualityType1==qualityType2)
                        {
                            Qualities[i].LinkedGlobalInstances.Add(Qualities[j]);
                            var lh = "";
                            if (Qualities[j].ValuesSection.TryGetValue("layer_height", out lh))
                            {
                                Qualities[i].Caption = Qualities[j].Name + " - " + lh + "mm"; 
                            }
                            else if (Qualities[i].Name=="Fine")
                            {
                                Qualities[i].ValuesSection["layer_height"] = "0.1";
                                Qualities[i].Caption = Qualities[j].Name + " - 0.1mm"; 
                            }
                        }
                    }
                }
                
                // var ind = Qualities[i].FileName.LastIndexOf("_");
                // var qualityType = Qualities[i].FileName.Substring(ind+1);
                // for (var j=0; j<Qualities.Count; j++)
                // {
                //     if (i!=j)
                //     {
                //         var ind2 = Qualities[j].FileName.LastIndexOf("_");
                //         var qualityType2 = Qualities[j].FileName.Substring(ind+1);

                //         if (Qualities[j].isGlobal && qualityType==qualityType2)
                //         {
                //             Qualities[i].LinkedGlobalInstances.Add(Qualities[j]);
                //             Qualities[i].Caption = Qualities[j].Name + " - " + lh + "mm"; 
                //         }
                //         else 
                //         {
                //             var ind = Qualities[i].FileName.LastIndexOf("_");
                //         }
                //     }
                // }
                // var ind = Qualities[i].FileName.LastIndexOf("_");
                // var layerHeight = Qualities[i].FileName.Substring(ind+1).Replace("mm", "");
                // for (var j=0; j<Qualities.Count; j++)
                // {
                //     if (i!=j)
                //     {
                //         var lh = "";
                //         if (Qualities[j].isGlobal && Qualities[j].ValuesSection.TryGetValue("layer_height", out lh) && lh==layerHeight)
                //         {
                //             Qualities[i].LinkedGlobalInstances.Add(Qualities[j]);
                //             Qualities[i].Caption = Qualities[j].Name + " - " + lh + "mm"; 
                //         }
                //         else 
                //         {
                //             var ind = Qualities[i].FileName.LastIndexOf("_");
                //         }
                //     }
                // }
            }
        }
    }
    public bool UpdateQualities(string MachineID, bool addOnlyNewInstances = false)
    {
        return UpdateInstanceContainer("quality", MachineID, addOnlyNewInstances);
    }    
    public void UpdateAllContainers()
    {
        UpdateVariants();       
        var isIntentsCorrect = UpdateIntents(MachineMetadatas["id"]);
        var isQuailitiesCorrect = UpdateQualities(MachineMetadatas["id"]);
        if (!isIntentsCorrect && !isQuailitiesCorrect && Variants.Count!=0)
        {
            for (var i=0; i<MachineInherites.Count; i++)
            {
                var machInherite = MachineInherites[i];
                if (machInherite!="fdmprinter")
                {
                    isIntentsCorrect = UpdateIntents(machInherite, true);
                    isQuailitiesCorrect = UpdateQualities(machInherite, true);
                    if (isIntentsCorrect || isQuailitiesCorrect)
                    {   
                        break;
                    }
                }
            }
        }
        UpdateQuailitiesCaptions();
    }    
    private MaterialSetting CreateMaterialSettingFromXML(XElement SettingElement)
    {
        var set = new MaterialSetting();
        set.key = SettingElement.Attribute("key").Value;
        set.value = ValueFormatter.FormatValue(SettingElement.Value);
        if (SettingElement.Name.LocalName=="cura:setting")
            set.IsCuraSetting = true; 
        return set;
    }
    public void SetSelectedMaterial(string MaterialId, bool IsUpdateIntentCategories = true)
    {
        Material mat = null;
        for (int i=0; i<Materials.Count; i++)
        {
            if (Materials[i].id==MaterialId) 
            {
                SelectedMaterial = Materials[i];
                if (IsUpdateIntentCategories)
                    UpdateIntentCategories();
                break;
            }
        }
    }
    public void SetDefaultMaterial()
    {
        var matID = "";
        var hasMaterials = "true";
        var brand = "";
        if (!MachineMetadatas.TryGetValue("preferred_material", out matID))
            matID = "generic_abs";
        for (var i=0; i<Materials.Count; i++)
        {
            if (Materials[i].id==matID)
            {
                brand = Materials[i].Brand;
                break;
            }
        }
        if (brand==null || brand=="")
            brand = "Custom";
        SetSelectedMaterialBrand(brand);
        SetSelectedMaterial(matID, false); 
        // if (MachineMetadatas.TryGetValue("preferred_material", out matID))
        //     SetSelectedMaterial(matID);
        // else if (MachineMetadatas.TryGetValue("has_materials", out hasMaterials) && hasMaterials=="true")
        //     SetSelectedMaterial("generic_abs"); 
    }
    private double DoubleParse(string valueStr)
    {
        return double.Parse(valueStr.Replace(",", "."), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
    }
    private void AddBalancedProfileWithGlobalQuality()
    {
        IntentCategory BalancedCat = new IntentCategory("default");
        BalancedCat.IntentName = "Balanced";
        IntentCategories.Add(BalancedCat);
        for (var i=0; i<Qualities.Count; i++)
        {
            var q = Qualities[i];
            var definition = "";
            if (q.isGlobal && q.GeneralSection.TryGetValue("definition", out definition) && definition==MachineMetadatas["id"])
                BalancedCat.QualityInstances.Add(Qualities[i]);    
        }
        if (BalancedCat.QualityInstances.Count==0)
        {
             for (var i=0; i<Qualities.Count; i++)
            {
                var q = Qualities[i];
                var definition = "";
                if (q.isGlobal && q.GeneralSection.TryGetValue("definition", out definition) && definition=="fdmprinter")
                    BalancedCat.QualityInstances.Add(Qualities[i]);    
            }
        }

        BalancedCat.QualityInstances.Sort((v1, v2) => 
         {
            var xStr = v1.Caption.Split('-')[1].Replace("mm", "").Trim();
            var xNumeric = DoubleParse(xStr);
            var yStr = v2.Caption.Split('-')[1].Replace("mm", "").Trim();
            var yNumeric = DoubleParse(yStr);

            return xNumeric.CompareTo(yNumeric);
        });
    }
    private bool AddBalancedProfile(bool isUseGenericMaterial)
    {
        IntentCategory BalancedCat = null;
        var mat = "";
        if (isUseGenericMaterial)
            mat = SelectedMaterial.GenericMaterial();
        else    
            mat = SelectedMaterial.BrandWithMaterial();
        for (var i=0; i<Qualities.Count; i++)
        {
            var quality = Qualities[i]; 
            var matId = "";
            var variant = "";
            if (quality.MetadataSection.TryGetValue("material", out matId) && 
                mat == matId &&
                quality.MetadataSection.TryGetValue("variant", out variant) &&
                (SelectedVariant==null || SelectedVariant.Name==variant))
            {
                if (BalancedCat==null)
                {
                    if (IntentCategories.Count>0 && IntentCategories[0].IntentCategoryName=="default")
                    {
                        BalancedCat = IntentCategories[0];
                    }   
                    else       
                    {
                        BalancedCat = new IntentCategory("default");
                        BalancedCat.IntentName = "Balanced";
                        IntentCategories.Insert(0, BalancedCat);
                    }
                }
                BalancedCat.QualityInstances.Add(quality);
            }             
        }
        if (BalancedCat==null)
            return false;
        else
            return true;
    }
    private bool AddProfiles(bool isUseGenericMaterial)
    {
        var mat = "";
        if (isUseGenericMaterial)
            mat = SelectedMaterial.GenericMaterial();
        else    
            mat = SelectedMaterial.BrandWithMaterial();
        for (var i=0; i<Intents.Count; i++)
        {
            var Intent = Intents[i]; 
            var matId = "";
            var variant = "";
            if (Intent.MetadataSection.TryGetValue("material", out matId) && 
                mat == matId &&
                Intent.MetadataSection.TryGetValue("variant", out variant) &&
                (SelectedVariant==null || SelectedVariant.Name==variant))
            {
                var intCatName = "";
                if (Intent.MetadataSection.TryGetValue("intent_category", out intCatName))
                {
                    IntentCategory intCategory = null;
                    for (var j=0; j<IntentCategories.Count; j++)
                    {
                        if (IntentCategories[j].IntentCategoryName == intCatName)
                        {
                            intCategory = IntentCategories[j];
                            break;   
                        }
                    }
                    if (intCategory==null)
                    {
                        intCategory = new IntentCategory(intCatName);
                        intCategory.IntentName = Intent.Caption;
                        IntentCategories.Add(intCategory);
                    }
                    intCategory.IntentInstances.Add(Intent);
                    var intentMaterial = "";
                    var intentVariant = "";
                    var intentQualityType = "";
                    if (Intent.MetadataSection.TryGetValue("quality_type", out intentQualityType) &&
                        Intent.MetadataSection.TryGetValue("variant", out intentVariant) &&
                        Intent.MetadataSection.TryGetValue("material", out intentMaterial))
                    {
                       for (int j=0; j<Qualities.Count; j++)
                        {
                            var quality = Qualities[j];
                            var qualityMaterial = "";
                            var qualityVariant = "";
                            var qualityQualityType = "";
                            if (quality.MetadataSection.TryGetValue("quality_type", out qualityQualityType) &&
                                quality.MetadataSection.TryGetValue("variant", out qualityVariant) &&
                                quality.MetadataSection.TryGetValue("material", out qualityMaterial))
                            {
                                if (intentMaterial==qualityMaterial && intentVariant==qualityVariant && intentQualityType==qualityQualityType)
                                {
                                    intCategory.QualityInstances.Add(quality); 
                                    break;
                                }
                            // if (Intent.FileName.Contains(quality.FileName))
                            // {
                            //     intCategory.QualityInstances.Add(quality); 
                            //     break;
                            // }
                            }
                        } 
                    }
                    
                }    
            }             
        }
        if (IntentCategories.Count<1)
            return false;
        else
            return true;
    }
    public void UpdateIntentCategories()
    {
        IntentCategories.Clear();
        if (!AddProfiles(false))
            AddProfiles(true);    
        if (!AddBalancedProfile(false))
            AddBalancedProfile(true);
        if (IntentCategories.Count==0 && Variants.Count==0)
            AddBalancedProfileWithGlobalQuality();
        if (IntentCategories.Count>0)
        {
            SelectedIntentCategory = IntentCategories[0];
            if (SelectedIntentCategory.IntentInstances.Count>0)
                SelectedIntent = SelectedIntentCategory.IntentInstances[0];
            else
                SelectedIntent = null;
            if (SelectedIntentCategory.QualityInstances.Count>0)
                SelectedQuality = SelectedIntentCategory.QualityInstances[0];
            else
                SelectedQuality = null;
        }   
        else
        {
            SelectedIntentCategory = null;
            SelectedIntent = null;
            SelectedQuality = null;
        }  
    }
    public void LoadMaterials()
    {
        Materials.Clear();
        string rootDirectory = CuraPath + "share\\cura\\resources\\materials";       
        if (Directory.Exists(rootDirectory))
        {
            string[] cfgFiles = Directory.GetFiles(rootDirectory, "*.xml.fdm_material", SearchOption.AllDirectories);
            foreach (string filePath in cfgFiles)
            {
                var id = Path.GetFileName(filePath).Replace(".xml.fdm_material", "");
                var mat = new Material();
                mat.id = id;
                XDocument doc = XDocument.Load(filePath);
                XElement root = doc.Root;
                XElement metadata = root.Element("{http://www.ultimaker.com/material}metadata");
                if (metadata!=null)
                {
                    foreach (XElement element in metadata.Elements())
                    {
                        if (element.Name.LocalName=="name" && element.HasElements)
                        {
                            foreach (XElement elementOfName in element.Elements())
                            {
                                mat.Metadatas.Add(elementOfName.Name.LocalName, elementOfName.Value);
                                if (elementOfName.Name.LocalName=="brand")
                                    mat.Brand = elementOfName.Value; 
                                if (elementOfName.Name.LocalName=="material")
                                    mat.MaterialName = elementOfName.Value;  
                                if (elementOfName.Name.LocalName=="label")
                                    mat.Label = elementOfName.Value;    
                                if (elementOfName.Name.LocalName=="color" && elementOfName.Value.ToLower()!="generic")
                                    mat.Color = elementOfName.Value;   
                            }
                        }
                        else
                        {
                            if (element.Name.LocalName=="GUID")
                                mat.GUID = element.Value;
                            mat.Metadatas.Add(element.Name.LocalName, element.Value);
                        }
                    }
                }
                XElement properties = root.Element("{http://www.ultimaker.com/material}properties");
                if (properties!=null)
                {
                    foreach (XElement element in properties.Elements())
                    {
                        if (element.Name.LocalName=="diameter")
                            mat.Diameter = element.Value; 
                        else if (element.Name.LocalName=="density")
                            mat.Density = element.Value;  
                        else if (element.Name.LocalName=="weight")
                            mat.Weight = element.Value;                  
                    }
                }
                XElement settings = root.Element("{http://www.ultimaker.com/material}settings");
                if (settings!=null)
                {
                    foreach (XElement element in settings.Elements())
                    {
                        if (element.Name.LocalName=="setting" || element.Name.LocalName=="cura:setting")
                        {
                            var set = CreateMaterialSettingFromXML(element);   
                            mat.Settings.Add(set); 
                        }
                        else if (element.Name.LocalName=="machine" && element.HasElements)
                        {
                            List<MachineMaterialInfo> MachineInfoList = new List<MachineMaterialInfo>();
                            foreach (XElement elementOfMachine in element.Elements())
                            {
                                if (elementOfMachine.Name.LocalName=="machine_identifier")
                                {
                                    var MachineInfo = new MachineMaterialInfo();
                                    var manufAttr = elementOfMachine.Attribute("manufacturer");
                                    if (manufAttr!=null)
                                       MachineInfo.Manufacturer = manufAttr.Value; 
                                    var productAttr = elementOfMachine.Attribute("product");
                                    if (productAttr!=null)
                                       MachineInfo.Name = productAttr.Value; 
                                    MachineInfoList.Add(MachineInfo);
                                }
                                else if (elementOfMachine.Name.LocalName=="setting" || elementOfMachine.Name.LocalName=="cura:setting")
                                {
                                    var set = CreateMaterialSettingFromXML(elementOfMachine); 
                                    for (int i=0; i<MachineInfoList.Count; i++)
                                    {
                                        MachineInfoList[i].Settings.Add(set); 
                                    }                    
                                }
                                else if (elementOfMachine.Name.LocalName=="hotend")
                                {
                                    var hotend = new HotendInfo();
                                    var idAttr = elementOfMachine.Attribute("id");
                                    if (idAttr!=null)
                                        hotend.id = idAttr.Value;
                                    foreach (XElement elementOfHotend in elementOfMachine.Elements())
                                    {      
                                        if (elementOfHotend.Name.LocalName=="setting" || elementOfHotend.Name.LocalName=="cura:setting")
                                        {
                                            var set = CreateMaterialSettingFromXML(elementOfHotend);
                                            hotend.Settings.Add(set);
                                        }
                                    }
                                    for (int i=0; i<MachineInfoList.Count; i++)
                                    {
                                        MachineInfoList[i].Hotends.Add(hotend); 
                                    }                    
                                }
                            } 
                            for (int i=0; i<MachineInfoList.Count; i++)
                            {
                                mat.MachineInfos.Add(MachineInfoList[i].Name.ToLower(), MachineInfoList[i]); //toLower() because in machine definition name has another regisrty (example: in definition - UltiMaker S5; in material - Ultimaker S5)
                            }             
                        }                 
                    }
                }
                Materials.Add(mat);
            }
        }
        Materials.Sort((m1, m2) => (m1.Brand + " " + m1.GetCaption() + " (" + m1.Color + "D=" + m1.Diameter + ")").CompareTo(
            m2.Brand + " " + m2.GetCaption() + " (" + m2.Color + "D=" + m2.Diameter + ")"));
    }
    public void FillParametersFromJSON(string GlobalJSONFile, string ExtruderJSONFile)
    {
        FillAllMachineParametersFromJSON(GlobalJSONFile);
        FillExtruderParametersFromJSON(ExtruderJSONFile);
    }

    //Others parameters depend on this parameter, especially their visibility. And since we always have 1 extruder, we set it to 1
    private void SetOnlyOneExtruderByDefault()
    {
        Parameter param = null;
        if (GlobalParams.TryGetValue("extruders_enabled_count", out param))
        {
            param.value = "1";
            param.OriginalValue = "1";
        }
        //maybe need to delete it
        if (GlobalParams.TryGetValue("machine_extruder_count", out param))
        {
            param.value = "1";
            param.OriginalValue = "1";
        }
    } 
    public void FillAllMachineParametersFromJSON(string GlobalJSONFile)
    {
        if (GlobalParams!=null)
            GlobalParams.Clear();
        var fileParser = new CuraJSONParser(CuraPath);
        if (!GlobalJSONFile.Contains(".def.json"))
            GlobalJSONFile = CuraPath + "share\\cura\\resources\\definitions\\" + GlobalJSONFile + ".def.json";    
        fileParser.ParseJSONFile(GlobalJSONFile, true, out GlobalParams);
        SetOnlyOneExtruderByDefault();
        MachineMetadatas = fileParser.MachineMetadatas;
        ExcludedMaterials = fileParser.ExcludedMaterials;
        MachineInherites = fileParser.MachineInherites;
        SetDefaultMaterial();
        UpdateAllContainers();
        UpdateIntentCategories();
    }
    public void FillExtruderParametersFromJSON(string ExtruderJSONFile)
    {
        if (ExtruderParams!=null)
            ExtruderParams.Clear();
        var fileParser = new CuraJSONParser(CuraPath);
        if (!ExtruderJSONFile.Contains(".def.json"))
            ExtruderJSONFile = CuraPath + "share\\cura\\resources\\extruders\\" + ExtruderJSONFile + ".def.json";  
        fileParser.ParseJSONFile(ExtruderJSONFile, false, out ExtruderParams);
    }
    public void ParseAllParameters()
    {
        AddAdditionalParams(); //or transfer to FillParametersFromJSON
        var paramsParser = new ParametersParser();
        paramsParser.ParseParameters(false, ref GlobalParams, ref ExtruderParams);
        paramsParser.ParseParameters(true, ref GlobalParams, ref ExtruderParams);
    }
    public string GetValue(string ParameterName, bool isGlobalParameter)
    {
        var paramsParser = new ParametersParser();
        var paramValue = "";
        if (isGlobalParameter)
        {
            paramValue = paramsParser.ParseParameter(ParameterName, ref GlobalParams, ref ExtruderParams, false);
        }
        else
        {
            paramValue = paramsParser.ParseParameter(ParameterName, ref ExtruderParams, ref GlobalParams, false);
        }
        // if (parameter.paramType==ParameterType.ptFloat)
        //     result = result.Replace(".", ",");   
        return paramValue;
    }

    public void SetValue(string ParameterName, string ParamValue, bool isGlobalParameter)
    {
        Parameter param = null;
        if (isGlobalParameter)
            GlobalParams.TryGetValue(ParameterName, out param);
        else
            ExtruderParams.TryGetValue(ParameterName, out param);
        if (param!=null)
        {
            if (param.paramType==ParameterType.ptFloat)
                ParamValue = ParamValue.Replace(",", ".");
            param.value = ParamValue; 
        }
    }

    public bool IsParameterEnabled(string ParameterName, bool isGlobalParameter)
    {
        var paramsParser = new ParametersParser();
        if (isGlobalParameter)
        {
            return paramsParser.IsParameterEnabled(ParameterName, ref GlobalParams, ref ExtruderParams);
        }
        else
        {
            return paramsParser.IsParameterEnabled(ParameterName, ref ExtruderParams, ref GlobalParams);
        }
    }
    public double MinimumValue(string ParameterName, bool isGlobalParameter)
    {
        var paramsParser = new ParametersParser();
        if (isGlobalParameter)
        {
            return paramsParser.GetMinimumValue(ParameterName, ref GlobalParams, ref ExtruderParams);
        }
        else
        {
            return paramsParser.GetMinimumValue(ParameterName, ref ExtruderParams, ref GlobalParams);
        }
    }
    public double MinimumValueWarning(string ParameterName, bool isGlobalParameter)
    {
        var paramsParser = new ParametersParser();
        if (isGlobalParameter)
        {
            return paramsParser.GetMinimumValueWarning(ParameterName, ref GlobalParams, ref ExtruderParams);
        }
        else
        {
            return paramsParser.GetMinimumValueWarning(ParameterName, ref ExtruderParams, ref GlobalParams);
        }
    }
    public double MaximumValue(string ParameterName, bool isGlobalParameter)
    {
        var paramsParser = new ParametersParser();
        if (isGlobalParameter)
        {
            return paramsParser.GetMaximumValue(ParameterName, ref GlobalParams, ref ExtruderParams);
        }
        else
        {
            return paramsParser.GetMaximumValue(ParameterName, ref ExtruderParams, ref GlobalParams);
        }
    }
    public double MaximumValueWarning(string ParameterName, bool isGlobalParameter)
    {
        var paramsParser = new ParametersParser();
        if (isGlobalParameter)
        {
            return paramsParser.GetMaximumValueWarning(ParameterName, ref GlobalParams, ref ExtruderParams);
        }
        else
        {
            return paramsParser.GetMaximumValueWarning(ParameterName, ref ExtruderParams, ref GlobalParams);
        }
    }
    public void FillAndParseParameters(string GlobalJSONFile, string ExtruderJSONFile)
    {
        FillParametersFromJSON(GlobalJSONFile, ExtruderJSONFile);
        ParseAllParameters();
    }
    private string GetBrandFromInheriteMachine(string inheriteName)
    {
        for (var i=0; i<MachinesList.Count; i++)
        {
            if (MachinesList[i].id.ToLower()==inheriteName.ToLower())
            {
                if (MachinesList[i].Manufacturer!="")
                {
                    return MachinesList[i].Manufacturer;
                }
                else
                {
                    return GetBrandFromInheriteMachine(MachinesList[i].Inherited);
                }
            }
        }
        return "";
    }
    private void FillMachinesBrands()
    {
        MachineBrand ultimakerBrand = null;
        MachineBrand customBrand = null;
        for (var i=0; i<MachinesList.Count; i++)
        {
            MachineBrand brand = null;
            var brandName = MachinesList[i].Manufacturer;
            if (brandName=="")
            {
                brandName = GetBrandFromInheriteMachine(MachinesList[i].Inherited);
            }
            if (brandName=="")
                brandName = "Custom";
            for (var j=0; j<MachinesBrands.Count; j++)
            {
                if (MachinesBrands[j].name.ToLower()==brandName.ToLower())
                {
                    brand = MachinesBrands[j];
                    break;
                }
            }
            if (brand==null)
            {
                brand = new MachineBrand(brandName);
                if (brandName=="Ultimaker B.V.")
                    ultimakerBrand = brand;
                if (brandName=="Custom")
                    customBrand = brand;    
                MachinesBrands.Add(brand); 
            }
            brand.Machines.Add(MachinesList[i]);
        }
        MachinesBrands.Sort((m1, m2) => m1.name.CompareTo(m2.name));
        if (customBrand!=null)
        {
            MachinesBrands.Remove(customBrand);   
            MachinesBrands.Insert(0, customBrand);
        } 
        if (ultimakerBrand!=null)
        {
            MachinesBrands.Remove(ultimakerBrand);   
            MachinesBrands.Insert(0, ultimakerBrand);
        }             
    }
    private void FillMaterialsBrands()
    {
        MaterialBrand genericBrand = null;
        MaterialBrand customBrand = null;
        for (var i=0; i<Materials.Count; i++)
        {
            MaterialBrand brand = null;
            var brandName = Materials[i].Brand;
            if (brandName=="")
                brandName = "Custom";
            for (var j=0; j<MaterialsBrands.Count; j++)
            {
                if (MaterialsBrands[j].name.ToLower()==brandName.ToLower())
                {
                    brand = MaterialsBrands[j];
                    break;
                }
            }
            if (brand==null)
            {
                brand = new MaterialBrand(brandName);
                if (brandName=="Generic")
                    genericBrand = brand;   
                if (brandName=="Custom")
                    customBrand = brand;   
                MaterialsBrands.Add(brand); 
            }
            brand.Materials.Add(Materials[i]);
        }
        MaterialsBrands.Sort((m1, m2) => m1.name.CompareTo(m2.name));
        if (customBrand!=null)
        {
            MaterialsBrands.Remove(customBrand);   
            MaterialsBrands.Insert(0, customBrand);
        }  
        if (genericBrand!=null)
        {
            MaterialsBrands.Remove(genericBrand);   
            MaterialsBrands.Insert(0, genericBrand);
        }          
    }
    public void ReadAllParametersAndConfigs()
    {
        GetSettingVisibilities();
        var fileParser = new CuraJSONParser(CuraPath);
        MachinesList = fileParser.GetAllMachinesFromJSONFiles();  
        MachinesList.Sort((m1, m2) => m1.name.CompareTo(m2.name));
        FillMachinesBrands();
        LoadMaterials();
        FillMaterialsBrands();
        SetSelectedMachineBrand("Ultimaker B.V.");
        SetSelectedMachine("ultimaker_s5"); 
        UpdateAllParameters();     
    }

    public void SetSelectedMachine(string id)
    {
        for (int i=0; i<MachinesList.Count; i++)
        {
            if (MachinesList[i].id==id)
            {   
                SelectedMachine = MachinesList[i];
                if (SelectedMachine.Extruders != null && SelectedMachine.Extruders.Count>0)
                    SelectedExtruder = SelectedMachine.Extruders[0];
                FillParametersFromJSON(SelectedMachine.id, SelectedExtruder.id);
                break;
            }
        }
    }
    public void SetSelectedExtruder(string id)
    {
        for (int i=0; i<SelectedMachine.Extruders.Count; i++)
        {
            if (SelectedMachine.Extruders[i].id==id)
            {   
                SelectedExtruder = SelectedMachine.Extruders[i];
                FillExtruderParametersFromJSON(SelectedExtruder.id);
                break;
            }
        }
    }
    public void SetSelectedIntentCategory(string name)
    {
        for (int i=0; i<IntentCategories.Count; i++)
        {
            if (IntentCategories[i].IntentCategoryName==name)
            {   
                SelectedIntentCategory = IntentCategories[i];
                var flg = false;
                for (var j=0; j<SelectedIntentCategory.QualityInstances.Count; j++)
                {
                    if (SelectedQuality.Name==SelectedIntentCategory.QualityInstances[j].Name)
                    {
                        SelectedQuality = SelectedIntentCategory.QualityInstances[j];
                        if (SelectedIntentCategory.IntentInstances.Count==SelectedIntentCategory.QualityInstances.Count)
                            SelectedIntent = SelectedIntentCategory.IntentInstances[j];
                        else
                            SelectedIntent = null;
                        flg = true;
                        break;
                    }
                }
                if (!flg)
                {
                    if (SelectedIntentCategory.IntentInstances.Count>0)
                        SelectedIntent = SelectedIntentCategory.IntentInstances[0];
                    else
                        SelectedIntent = null;
                    if (SelectedIntentCategory.QualityInstances.Count>0)
                        SelectedQuality = SelectedIntentCategory.QualityInstances[0]; 
                    else
                        SelectedQuality = null;     
                }
                break;
            }
        }
    }
    public void SetSelectedQuality(string name)
    {
        for (int i=0; i<SelectedIntentCategory.QualityInstances.Count; i++)
        {
            if (SelectedIntentCategory.QualityInstances[i].FileName==name)
            {   
                SelectedQuality = SelectedIntentCategory.QualityInstances[i];
                if (SelectedIntentCategory.IntentInstances.Count==SelectedIntentCategory.QualityInstances.Count)
                    SelectedIntent = SelectedIntentCategory.IntentInstances[i];
                else
                    SelectedIntent = null;
                break;
            }
        }
    }   

    private void SetToParameters(string key, string value)
    {
        Parameter param = null;
        if (!GlobalParams.TryGetValue(key, out param))
            ExtruderParams.TryGetValue(key, out param);    
        if (param!=null)
        {
            param.value = value; 
        }
    }
    private void RestoreMachineOriginalValues(bool isGlobalParameters)
    {
        if (isGlobalParameters)
        {
            foreach (KeyValuePair<string, Parameter> keyValueElement in GlobalParams)
            {
                keyValueElement.Value.RestoreValue();
            }
        }
        else
        {
            foreach (KeyValuePair<string, Parameter> keyValueElement in ExtruderParams)
            {
                keyValueElement.Value.RestoreValue();
            }
        }
    }
    public void AcceptDefaultParametersFromConfigs()
    {
        RestoreMachineOriginalValues(true);
        RestoreMachineOriginalValues(false);
        var machineName = "";
        if (!MachineMetadatas.TryGetValue("name", out machineName))
            machineName = "";
        
        //set parameters from Material
        if (SelectedMaterial!=null)
        {
            Parameter param = null;
            if (GlobalParams.TryGetValue("material_diameter", out param))
            {
                param.value = SelectedMaterial.Diameter;
            }
            if (GlobalParams.TryGetValue("material_brand", out param))
            {
                param.value = SelectedMaterial.Brand;
            }
            if (GlobalParams.TryGetValue("material_type", out param))
            {
                param.value = SelectedMaterial.MaterialName;
            }
            if (GlobalParams.TryGetValue("material_guid", out param))
            {
                param.value = SelectedMaterial.GUID;
            }

            for (int i=0; i<SelectedMaterial.Settings.Count; i++)
            {
                if (SelectedMaterial.Settings[i].IsCuraSetting)
                {
                    var key = SelectedMaterial.Settings[i].key;
                    var value = SelectedMaterial.Settings[i].key;
                    SetToParameters(key, value);
                }
            }
            if (machineName!="")
            {
                MachineMaterialInfo mmi = null;
                if (SelectedMaterial.MachineInfos.TryGetValue(machineName.ToLower(), out mmi))
                {
                    for (var i=0; i<mmi.Settings.Count; i++)
                    {
                        if (mmi.Settings[i].IsCuraSetting)
                        {
                            var key = mmi.Settings[i].key;
                            var value = mmi.Settings[i].key;
                            SetToParameters(key, value);
                        }
                    }
                    for (var i=0; i<mmi.Hotends.Count; i++)
                    {
                        var he = mmi.Hotends[i];
                        for (var j=0; j<he.Settings.Count; j++)
                        {
                            if (he.Settings[j].IsCuraSetting)
                            {
                                var key = he.Settings[j].key;
                                var value = he.Settings[j].key;
                                SetToParameters(key, value);  
                            }                    
                        }
                    }
                }
            }
        }
        
        //set parameters from Variant
        if (SelectedVariant!=null)
        {
            foreach (KeyValuePair<string, string> keyValueElement in SelectedVariant.ValuesSection)
            {
                SetToParameters(keyValueElement.Key, keyValueElement.Value);  
            }
        }
        
        //set parameters from Intent
        if (SelectedIntent!=null)
        {
            foreach (KeyValuePair<string, string> keyValueElement in SelectedIntent.ValuesSection)
            {
                SetToParameters(keyValueElement.Key, keyValueElement.Value);  
            }
        }
        
        //set parameters from Quality
        if (SelectedQuality!=null)
        {
            for (int i=0; i<SelectedQuality.LinkedGlobalInstances.Count; i++)
            {
                var LinkedGlobalInstance = SelectedQuality.LinkedGlobalInstances[i];
                foreach (KeyValuePair<string, string> keyValueElement in LinkedGlobalInstance.ValuesSection)
                {
                    SetToParameters(keyValueElement.Key, keyValueElement.Value);  
                }   
            }
            foreach (KeyValuePair<string, string> keyValueElement in SelectedQuality.ValuesSection)
            {
                SetToParameters(keyValueElement.Key, keyValueElement.Value);  
            }
        }      
    }
    public void AcceptUserParameters()
    {
        foreach (KeyValuePair<string, string> keyValueElement in UserParameters)
        {
            SetToParameters(keyValueElement.Key, keyValueElement.Value);  
        } 
    }
    public void UpdateAllParameters()
    {
        AcceptDefaultParametersFromConfigs();
        AcceptUserParameters();     
    }
    public void AddUserParameter(string key, string value, bool isGlobalParameter)
    {
        Parameter param = null;
        if (isGlobalParameter)
            GlobalParams.TryGetValue(key, out param);
        else
            ExtruderParams.TryGetValue(key, out param);
        if (param!=null)
        {
            if (param.paramType==ParameterType.ptFloat)
                value = value.Replace(",", ".");
            UserParameters[key] = value;
        }
    }
    public void SetSelectedMachineBrand(string brandName, bool isSelectMachine = false)
    {
        for (int i=0; i<MachinesBrands.Count; i++)
        {
            if (MachinesBrands[i].name==brandName)
            {
                SelectedMachineBrand = MachinesBrands[i];
                if (isSelectMachine)
                {
                    for (int j=0; j<SelectedMachineBrand.Machines.Count; j++)
                    {
                        if (SelectedMachineBrand.Machines[j].isVisible)
                        {
                            SetSelectedMachine(SelectedMachineBrand.Machines[j].id); 
                            break;  
                        }                              
                    }
                }
                break;
            }
        }
    }
    public void SetSelectedMaterialBrand(string brandName, bool isSelectMaterial = false)
    {
        for (int i=0; i<MaterialsBrands.Count; i++)
        {
            if (MaterialsBrands[i].name==brandName)
            {
                SelectedMaterialBrand = MaterialsBrands[i];
                if (isSelectMaterial)
                    SetSelectedMaterial(SelectedMaterialBrand.Materials[0].id);
                break;
            }
        }
    }
}