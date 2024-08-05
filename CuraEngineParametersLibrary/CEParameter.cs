namespace CuraEngineParametersLibrary;
public enum ParameterType
{
    ptStr,
    ptBool,
    ptFloat,
    ptEnum,
    ptInt,
    ptArrayofInt,
    ptPolygon,
    ptPolygons,
    ptOptionalExtruder,
    ptExtruder,
    ptCategory
}
public enum ParameterValueType
{
    pvtValue,
    pvtMaximumValue,
    pvtMaximumValueWarning,
    pvtMinimumValue,
    pvtMinimumValueWarning,
    pvtEnabledValue
}
public record EnumItem
{
    public EnumItem(string ID, string Name)
    {
        id = ID;
        name = Name;
    }
    public string id;
    public string name;
}
public class Parameter
{
    public Parameter()
    {
        value = "0";
        OriginalValue = "0";
        enabled = "True";
        minimumValue = "0";
        maximumValue = "0";
        HasMinimumValue = false;
        HasMaximumValue = false;
        minimumValueWarning = "0";
        maximumValueWarning = "0";
        HasMinimumValueWarning = false;
        HasMaximumValueWarning = false;
        calculatedValue = "0";
    }
    public string id;
    public string description;
    public string label;
    public string icon;
    public string value;
    public string calculatedValue;
    public string OriginalValue;
    public string unit;
    public ParameterType paramType;
    public List<EnumItem> options;
    public string enabled;
    public string minimumValue;
    public string maximumValue;
    public string minimumValueWarning;
    public string maximumValueWarning;
    public Parameter parent;
    public List<Parameter> children;
    public bool HasMinimumValue;
    public bool HasMaximumValue;
    public bool HasMinimumValueWarning;
    public bool HasMaximumValueWarning;
    public int indProp; //save index from cfg settings to add to PropIterator
    public bool IsGlobalParameter;
    public bool IsVisibleInInspector = false;
    public bool IsExpanded = true;
    public void RestoreValue()
    {
        value = OriginalValue;
    }
    public void SetMinimumValue(string value)
    {
        HasMinimumValue = true;
        minimumValue = value;
    }
    public void SetMaximumValue(string value)
    {
        HasMaximumValue = true;
        maximumValue = value;
    }
    public void SetMinimumValueWarning(string value)
    {
        HasMinimumValueWarning = true;
        minimumValueWarning = value;
    }
    public void SetMaximumValueWarning(string value)
    {
        HasMaximumValueWarning = true;
        maximumValueWarning = value;
    }
    public void AddEnumItem(EnumItem item)
    {
        if (options==null)
        {
            options = new List<EnumItem>();
        }
        options.Add(item);
    }
    public bool HasEnumItems()
    {
        if (options != null && options.Count>0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void AddChild(Parameter child)
    {
        if (children==null)
        {
            children = new List<Parameter>();
        }
        children.Add(child);
    }
    public bool HasChildren()
    {
        if (children != null && children.Count>0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public string GetValueByType(ParameterValueType valueType)
    {
        switch (valueType)
        {
           case ParameterValueType.pvtEnabledValue: return enabled;
           case ParameterValueType.pvtMaximumValue: return maximumValue;
           case ParameterValueType.pvtMaximumValueWarning: return maximumValueWarning;
           case ParameterValueType.pvtMinimumValue: return minimumValue;
           case ParameterValueType.pvtMinimumValueWarning: return minimumValueWarning;
           default: return value;
        }
    }
    public void SetValueByType(string _value, ParameterValueType valueType)
    {
        switch (valueType)
        {
           case ParameterValueType.pvtEnabledValue: 
           {
                enabled = _value;
                break;
           } ;
           case ParameterValueType.pvtMaximumValue: 
           {
                maximumValue = _value;
                break;
           } 
           case ParameterValueType.pvtMaximumValueWarning: 
           {
                maximumValueWarning = _value;
                break;
           } 
           case ParameterValueType.pvtMinimumValue:
           {
                minimumValue = _value;
                break;
           } 
           case ParameterValueType.pvtMinimumValueWarning: 
           {
                minimumValueWarning = _value;
                break;
           } 
           default: 
           {
                value = _value;
                break;   
           } 
        }
    }
}