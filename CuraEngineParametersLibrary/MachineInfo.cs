namespace CuraEngineParametersLibrary;

public record Extruder
{
    public string id;
    public string name;
    
}
public class MachineInfo
{
    public MachineInfo()
    {
        Extruders = new List<Extruder>();
    }
    public string id = "";
    public string name = "";
    public string Manufacturer = "";
    public string Inherited = "";
    public bool isVisible = true;
    public List<Extruder> Extruders;
}
public class MachineBrand
{
    public MachineBrand(string brandName)
    {
        name = brandName;
        Machines = new List<MachineInfo>();
    }
    public string name = "";
    public List<MachineInfo> Machines;
}
