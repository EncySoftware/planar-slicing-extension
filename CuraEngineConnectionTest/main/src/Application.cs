using CuraConnectionInterface;
using CuraEngineParametersLibrary;
using CuraEngineNetWrapper;
namespace CuraEngineConnectionTest;

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
        return CEParameters.GlobalParams.ElementAt(index).Key;
    }
    public string GlobalParamValue(int index)
    {
        return CEParameters.GlobalParams.ElementAt(index).Value.value;
    }
    public int ExtruderParamsSize()
    {
        return CEParameters.ExtruderParams.Count;
    }
    public string ExtruderParamName(int index)
    {
        return CEParameters.ExtruderParams.ElementAt(index).Key;
    }
    public string ExtruderParamValue(int index)
    {
        return CEParameters.ExtruderParams.ElementAt(index).Value.value;
    }
}
public class CuraEngineControlProcess : ICuraEngineControlProcess
{
    bool startPathSegment = false;
    float currentLayerHeight = 0;
    int cutInc = 0;
    int LayerInc = -1;
    int segmentInc = 0;
    float prevX = 0;
    float prevY = 0;
    public void OnProgress(double progress)
    {
        try
        {

        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }

    public void OnGCode(string GCode)
    {
        try
        {

        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }

    public void OnPrintTimeMaterialEstimates(string name, double value)
    {
        try
        {

        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }

    public void OnFinish(string msg)
    {
        try
        {

        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }
    public void OnError(string msg)
    {
        try
        {

        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }

    public void OnGCodePrefix(string GCodePrefix)
    {
        try
        {

        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }
    public void OnSliceUUID(string SliceUUID)
    {
        try
        {

        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }
    
    public void OnStartLayersOptimized(int id, double height, double thickness)
    {
        try
        {
            LayerInc++;  
            currentLayerHeight = (float)height;
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }
    public void OnStopLayersOptimized()
    {
        try
        {
            if (LayerInc < 50)
            {
                segmentInc = 0;
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }
    public void OnStartPathSegment(int extruderID, int point_type)
    {
        try
        {   
            if (LayerInc < 50)
            {
                segmentInc++;
                startPathSegment = true;
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }

    public void OnStopPathSegment()
    {
        try
        {
            if (LayerInc < 50)
            {
                cutInc = 0;
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }

    public void Add2SPoint(TCE2SPoint p, int lineType, float lineWidth, float lineThickness, float lineFeedrate)
    {
        try
        {
             if (LayerInc < 50)
             {
                if (startPathSegment)
                {             
                    prevX = p.X;
                    prevY = p.Y;
                    startPathSegment = false;
                }
                else
                {
                    cutInc ++;
                    prevX = p.X;
                    prevY = p.Y;
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }

    public void Add3SPoint(TCE3SPoint p, int lineType, float lineWidth, float lineThickness, float lineFeedrate)
    {
        try
        {

        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }

    void ICuraEngineControlProcess.OnLogMessage(int messageType, string messageString)
    {
        
    }

    bool ICuraEngineControlProcess.IsProcessCancelled()
    {
        return false;
    }
}
class Application
{
    private CuraEngineControlProcess CEControlProcess = null;
    private ParamsReceiver CEParamsReceiver = null;
    private static TCE3SPoint P3S(double Px, double Py, double Pz)
    {
        return new TCE3SPoint{
            X = (float)Px,
            Y = (float)Py,
            Z = (float)Pz
        }; 
    }

    TCE3SPoint[] points = new TCE3SPoint[36]{
            P3S(25.5, 27.5, 56.0), P3S(25.5, 27.5, 0.0), P3S(-25.5, 27.5, 0.0),
            P3S(25.5, 27.5, 56.0), P3S(-25.5, 27.5, 0.0), P3S(-25.5, 27.5, 56.0),
            P3S(25.5, 27.5, 56.0), P3S(25.5, 27.5, 56.0), P3S(-25.5, -27.5, 56.0),
            P3S(25.5, 27.5, 56.0), P3S(-25.5, -27.5, 56.0), P3S(25.5, -27.5, 56.0),
            P3S(-25.5, 27.5, 56.0), P3S(-25.5, 27.5, 0.0), P3S(-25.5, -27.5, 0.0),
            P3S(-25.5, 27.5, 56.0), P3S(-25.5, -27.5, 0.0), P3S(-25.5, -27.5, 56.0),
            P3S(25.5, -27.5, 0.0), P3S(-25.5, -27.5, 0.0), P3S(-25.5, 27.5, 0.0),
            P3S(25.5, -27.5, 0.0), P3S(-25.5, 27.5, 0.0), P3S(25.5, 27.5, 0.0),
            P3S(-25.5, -27.5, 56.0), P3S(-25.5, -27.5, 0.0), P3S(25.5, -27.5, 0.0),
            P3S(-25.5, -27.5, 56.0), P3S(25.5, -27.5, 0.0), P3S(25.5, -27.5, 56.0),
            P3S(25.5, -27.5, 56.0), P3S(25.5, -27.5, 0.0), P3S(25.5, 27.5, 0.0),
            P3S(25.5, -27.5, 56.0), P3S(25.5, 27.5, 0.0), P3S(25.5, 27.5, 56.0)};

    
    private void FillPoints(ITrianglesReciever r)
    {
        r.BeginTransfer();
        try
        {
            r.BeginModel();
            try
            {
                r.BeginMesh("Model1 - Mesh1", 12);
                try
                {
                    for (var i=0; i<12; i++)
                    {
                        r.AddTriangle(points[i*3], points[i*3+1], points[i*3+2]);
                    }
                }
                finally
                {
                    r.EndMesh();
                }
                r.BeginMesh("Model1 - Mesh2", 12);
                try
                {
                    for (var i=0; i<12; i++)
                    {
                        var p1 = points[i*3];
                        p1.X = p1.X + 100;
                        var p2 = points[i*3+1];
                        p2.X = p2.X + 100;
                        var p3 = points[i*3+2];
                        p3.X = p3.X + 100;
                        r.AddTriangle(p1, p2, p3);
                    }
                }
                finally
                {
                    r.EndMesh();
                }
            }
            finally
            {
                r.EndModel();
            }   
            r.BeginModel();
            try
            {
                r.BeginMesh("Model2 - Mesh1", 12);
                try
                {
                    for (var i=0; i<12; i++)
                    {
                        var p1 = points[i*3];
                        var p2 = points[i*3+1];
                        var p3 = points[i*3+2];
                        p1.Y = p1.Y + 100;
                        p2.Y = p2.Y + 100;
                        p3.Y = p3.Y + 100;
                        r.AddTriangle(p1, p2, p3);
                    }
                }
                finally
                {
                    r.EndMesh();
                }
                r.BeginMesh("Model2 - Mesh2", 12);
                try
                {
                    for (var i=0; i<12; i++)
                    {
                        var p1 = points[i*3];
                        p1.X = p1.X + 100;
                        var p2 = points[i*3+1];
                        p2.X = p2.X + 100;
                        var p3 = points[i*3+2];
                        p3.X = p3.X + 100;
                        p1.Y = p1.Y + 100;
                        p2.Y = p2.Y + 100;
                        p3.Y = p3.Y + 100;
                        r.AddTriangle(p1, p2, p3);
                    }
                }
                finally
                {
                    r.EndMesh();
                }
            }
            finally
            {
                r.EndModel();
            }   
        }
        finally
        {
            r.EndTransfer();
        }  
    }
    public void FillParametersAndCalculate()
    {
        CEParamsReceiver = new ParamsReceiver();
        CEParamsReceiver.CEParameters.FillAndParseParameters("C:\\Program Files\\UltiMaker Cura 5.7.1\\share\\cura\\resources\\definitions\\ultimaker_s5.def.json", 
                                                            "C:\\Program Files\\UltiMaker Cura 5.7.1\\share\\cura\\resources\\extruders\\ultimaker_s5_extruder_left.def.json");
        CEControlProcess = new CuraEngineControlProcess(); 
        var lib = CuraEngineConnectionHelper.LoadNativeLib("C:\\Program Files\\UltiMaker Cura 5.7.1\\");
        var tr = lib.TrianglesReciever;
        FillPoints(tr);
        lib.Slice(CEControlProcess, CEParamsReceiver, "C:\\Program Files\\UltiMaker Cura 5.7.1\\");
    }
    public void StartCalculateInCuraEngine()
    {
        FillParametersAndCalculate();
        CuraEngineConnectionHelper.FinalizeLib(); 
    }
    public void Run ()
    {   
        StartCalculateInCuraEngine();
    }
}