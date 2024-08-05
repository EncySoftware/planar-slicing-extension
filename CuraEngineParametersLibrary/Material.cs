namespace CuraEngineParametersLibrary;

public class MaterialSetting
{
    public MaterialSetting()  
    {
        IsCuraSetting = false;
    }
    public string key;
    public string value;
    public bool IsCuraSetting;
}
public class HotendInfo
{
    public string id;
    public List<MaterialSetting> Settings;
    public HotendInfo()
    {
       Settings = new List<MaterialSetting>();
    }
}
public class MachineMaterialInfo
{
    public string Manufacturer;
    public string Name; 
    public bool IsCuraSetting;
    public List<MaterialSetting> Settings;
    public List<HotendInfo> Hotends;
    public MachineMaterialInfo()
    {
        Settings = new List<MaterialSetting>();
        Hotends = new List<HotendInfo>();
    }
}
public class Material
{
    public Material()
    {
        Metadatas = new Dictionary<string, string>();
        Settings = new List<MaterialSetting>();

        ////first parameter in Lower registry because different definition/configs have different registry in letter
        // (example: in machine definition - UltiMaker S5; in material - Ultimaker S5)
        MachineInfos = new Dictionary<string, MachineMaterialInfo>(); 
    }
    public string id;
    public string MaterialName = "";
    public string Brand = "";
    public string Label = "";
    public string Color = "";
    public string GUID = "";
    public Dictionary<string, string> Metadatas;
    public string Density;
    public string Diameter;
    public string Weight;
    public List<MaterialSetting> Settings;
    public Dictionary<string, MachineMaterialInfo> MachineInfos;
    public string GetCaption()
    {
        if (Label!="")
            return Label;
        else
            return MaterialName;
    }
    public string BrandWithMaterial()
    {
        var bm = Brand + "_" + MaterialName;
        return bm.ToLower();
    }
    public string GenericMaterial()
    {
        var bm = "generic_" + MaterialName;
        return bm.ToLower();
    }
}
public class MaterialBrand
{
    public MaterialBrand(string brandName)
    {
        name = brandName;
        Materials = new List<Material>();
    }
    public string name = "";
    public List<Material> Materials;
}