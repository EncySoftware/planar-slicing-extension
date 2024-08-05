using System.Text.Json;
using System.Text.RegularExpressions;

namespace CuraEngineParametersLibrary;

public class ValueFormatter
{
    public static string ReplaceBrackets(string value)
    {
        string pattern = @"(\bin\s+\[.*?\])";
        value = Regex.Replace(value, pattern, match => match.Value.Replace('[', '(').Replace(']', ')'));
        return value;
    }
    public static string ReplaceFunctionName(string value)
    {
        value = value.Replace("max(", "Max(");
        value = value.Replace("min(", "Min(");
        value = value.Replace("round(", "Round(");
        value = value.Replace("math.ceil(", "Ceiling(");
        value = value.Replace("math.floor(", "Floor(");
        value = value.Replace("math.log(", "Log(");
        value = value.Replace("math.sqrt(", "Sqrt(");
        value = value.Replace("math.pi", "Pi");
        value = value.Replace("math.cos(", "Cos(");
        value = value.Replace("math.tan(", "Tan(");
        value = value.Replace("math.sin(", "Sin(");
        value = value.Replace("math.radians(", "Radians(");
        return value;
    }   
    public static string FormatValue(string value)
        {
            value = value.Replace("\\\"", "'");
            value = value.Replace("\"", "");
            value = value.Replace("\n", "");
            value = Regex.Replace(value, @"\s+", " ");
            if (value.StartsWith("'") && !value.Contains(" ")) //if this is just an enum value, then remove the single brackets
            {
                value = value.Replace("'", "");
            }
            if (value.Contains("in ["))
            {
                value = ReplaceBrackets(value);
            }
            value = ReplaceFunctionName(value);
            return value;
    }
}