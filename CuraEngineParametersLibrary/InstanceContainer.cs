namespace CuraEngineParametersLibrary;
public enum InstanceContainerType
{
    ictVariant,
    ictIntent,
    ictQuality,
    ictMaterial
}

public class Container
{
    public string FileName;
    public string Name = "";
    public string Caption = "";
    public bool isGlobal= false;
    public List<Container> LinkedGlobalInstances;
    public Dictionary<string, string> GeneralSection;
    public Dictionary<string, string> MetadataSection;
    public Dictionary<string, string> ValuesSection;
    public InstanceContainerType InstanceType;
    public Container(string InstanceName)
    {
        GeneralSection = new Dictionary<string, string>();
        MetadataSection = new Dictionary<string, string>();
        ValuesSection = new Dictionary<string, string>();
        LinkedGlobalInstances = new List<Container>();
        if (InstanceName=="variants")
            InstanceType = InstanceContainerType.ictVariant;
        else if (InstanceName=="quality")
            InstanceType = InstanceContainerType.ictQuality;
        else if (InstanceName=="intent")
            InstanceType = InstanceContainerType.ictIntent;

    }   
    public void AddParameter(string SectionName, string ParamName, string ParamValue)
    {
        if (SectionName=="general")
        {
            if (ParamName=="name")
                Name = ParamValue;
            GeneralSection.Add(ParamName, ParamValue);
        }
        else if (SectionName=="metadata")
            MetadataSection.Add(ParamName, ParamValue);
        else if (SectionName=="values")
        {
            if (ParamValue.Length>0 && ParamValue.IndexOf("=")==0)
                ParamValue = ParamValue.Substring(1);      
            ValuesSection.Add(ParamName, ValueFormatter.FormatValue(ParamValue));
        }
    }
}
public class IntentCategory
{
    public IntentCategory(string Name)
    {
        IntentInstances = new List<Container>();
        QualityInstances = new List<Container>();
        IntentCategoryName = Name;
    }
    public string IntentName;
    public string IntentCategoryName;
    // The index must match the index of the corresponding quaility in QualityInstances (IntentInstances.Count=QualityInstances)
    // But IntentInstances will empty for Balanced intent category
    public List<Container> IntentInstances; 
    public List<Container> QualityInstances; //the index must match the index of the corresponding intent in IntentInstances (IntentInstances.Count=QualityInstances)
}