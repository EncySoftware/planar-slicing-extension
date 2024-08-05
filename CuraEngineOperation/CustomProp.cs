namespace CuraEngineOperation;

using STCustomPropTypes;

public class CustomProp : IST_CustomProp, IST_CustomPropID, IST_CustomPropCaption, IST_CustomPropIcon, IST_CustomDoubleProp
{
    private double DoubleValue = 10;
    public TCustomPropType PropType => TCustomPropType.ctDouble;

    public string PropID => "Test";

    public string Caption => "Test Parameter";

    public string IconFile => "Images\\Mill_3Axis_ico.bmp";

    public double FltValue { get => DoubleValue; set => DoubleValue = value; }
}
public delegate double OnGetDoubleValue();
public delegate void OnSetDoubleValue(double value);
public class DoubleValueGetter : IDoubleValueGetter
{
    public DoubleValueGetter(OnGetDoubleValue getter)
    {
        fOnGetDoubleValue = getter;
    }   
    OnGetDoubleValue fOnGetDoubleValue;
    public double GetValue()
    {
        return(fOnGetDoubleValue());
    }
}
public class DoubleValueSetter : IDoubleValueSetter
{
    public DoubleValueSetter(OnSetDoubleValue setter)
    {
        fOnSetDoubleValue = setter;
    }   
    OnSetDoubleValue fOnSetDoubleValue;

    public void SetValue(double Value)
    {
        fOnSetDoubleValue(Value);
    }
}

public delegate string OnGetStringValue();
public delegate void OnSetStringValue(string value);
public class StringValueGetter : IStringValueGetter
{
    public StringValueGetter(OnGetStringValue getter)
    {
        fOnGetStringValue = getter;
    }   
    OnGetStringValue fOnGetStringValue;
    public string GetValue()
    {
        return(fOnGetStringValue());
    }
}
public class StringValueSetter : IStringValueSetter
{
    public StringValueSetter(OnSetStringValue setter)
    {
        fOnSetStringValue = setter;
    }   
    OnSetStringValue fOnSetStringValue;

    public void SetValue(string Value)
    {
        fOnSetStringValue(Value);
    }
}

public delegate int OnGetIntegerValue();
public delegate void OnSetIntegerValue(int value);
public class IntegerValueGetter : IIntegerValueGetter
{
    public IntegerValueGetter(OnGetIntegerValue getter)
    {
        fOnGetIntegerValue = getter;
    }   
    OnGetIntegerValue fOnGetIntegerValue;
    public int GetValue()
    {
        return(fOnGetIntegerValue());
    }
}
public class IntegerValueSetter : IIntegerValueSetter
{
    public IntegerValueSetter(OnSetIntegerValue setter)
    {
        fOnSetIntegerValue = setter;
    }   
    OnSetIntegerValue fOnSetIntegerValue;

    public void SetValue(int Value)
    {
        fOnSetIntegerValue(Value);
    }
}

public delegate bool OnGetBooleanValue();
public delegate void OnSetBooleanValue(bool value);
public class BooleanValueGetter : IBooleanValueGetter
{
    public BooleanValueGetter(OnGetBooleanValue getter)
    {
        fOnGetBooleanValue = getter;
    }   
    OnGetBooleanValue fOnGetBooleanValue;
    public bool GetValue()
    {
        return(fOnGetBooleanValue());
    }
}
public class BooleanValueSetter : IBooleanValueSetter
{
    public BooleanValueSetter(OnSetBooleanValue setter)
    {
        fOnSetBooleanValue = setter;
    }   
    OnSetBooleanValue fOnSetBooleanValue;

    public void SetValue(bool Value)
    {
        fOnSetBooleanValue(Value);
    }
}

public delegate void OnRestoreDefaultPropValue();
public class DefaultPropValue : IST_CustomPropDefaultable
{
    public DefaultPropValue(OnRestoreDefaultPropValue RestoreDefaultProp)
    {
        fOnRestoreDefaultPropValue = RestoreDefaultProp;
    }   
    OnRestoreDefaultPropValue fOnRestoreDefaultPropValue;

    public void RestoreDefaultValue()
    {
        fOnRestoreDefaultPropValue();
    }
}