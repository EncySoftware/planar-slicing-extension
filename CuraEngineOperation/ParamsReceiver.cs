using CuraConnectionInterface;
using CuraEngineParametersLibrary;

namespace CuraEngineOperation;

public class ParamsReceiver : IParamsReceiver
{
    public Parameters CEParameters;
    public ParamsReceiver()
    {
        CEParameters = new Parameters();
    }
    public int GlobalParamsSize()
    {
        return CEParameters.GlobalParams.Count;
    }
    public string GlobalParamName(int index)
    {
        // string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        // string path = Path.GetDirectoryName(assemblyLocation)+"\\GlobalParameters.txt";
        // StreamWriter swGP = new StreamWriter(path, true);
        // swGP.WriteLine(CEParameters.GlobalParams.ElementAt(index).Key+":"+CEParameters.GlobalParams.ElementAt(index).Value.calculatedValue);
        // swGP.Close();
        return CEParameters.GlobalParams.ElementAt(index).Key;
    }
    public string GlobalParamValue(int index)
    {
        var param = CEParameters.GlobalParams.ElementAt(index).Value;
        var value = param.calculatedValue;
        return value;
    }
    public int ExtruderParamsSize()
    {
        return CEParameters.ExtruderParams.Count;
    }
    public string ExtruderParamName(int index)
    {
        // string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        // string path = Path.GetDirectoryName(assemblyLocation)+"\\ExtruderParameters.txt";
        // StreamWriter swEP = new StreamWriter(path, true);
        // swEP.WriteLine(CEParameters.ExtruderParams.ElementAt(index).Key+":"+CEParameters.ExtruderParams.ElementAt(index).Value.calculatedValue);
        // swEP.Close();
        return CEParameters.ExtruderParams.ElementAt(index).Key;
    }
    public string ExtruderParamValue(int index)
    {
        var param = CEParameters.ExtruderParams.ElementAt(index).Value;
        var value = param.calculatedValue;
        return value;
    }
}