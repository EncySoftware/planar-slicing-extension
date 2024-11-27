using System;
using CAMAPI.DotnetHelper;
using CAMAPI.Extensions;
using CAMAPI.Logger;
using CAMAPI.MCDFormerTypes;
using CAMAPI.TechOperation;
using CuraConnectionInterface;
using STTypes;

namespace CuraEngineOperation;

public enum ToolpathParsingMode
{
    tpmSimplified,
    tpmGCodeBased
}
public delegate string GCodeCommandTranslation(string label = "");

public class CuraEngineControlProcess : ICuraEngineControlProcess, IDisposable
{
    public double FilamentExtrudingLength = 100; 
    public ToolpathParsingMode tpm = ToolpathParsingMode.tpmSimplified;
    public bool IsOutputAdditionalCLDataParameters = false;
    public bool IsOutputFilamentExtruding = false;
    bool startPathSegment = false;
    double currentLayerHeight = 0;
    public TST3DBox boundingBox;
    int LayerInc = 0;
    float prevX = 0;
    float prevY = 0;
    int prevLineType = 0;
    float prevLineWidth = 0;
    float prevLineThickness = 0;
    float prevLineFeedrate = 0;
    bool IsStartCreateToolpath = true;
    public  GCodeLayout GCLayout = null;
    public ICamApiCLDReceiver? clf;
    bool isFeedSectionOpened = false;
    int LastLineSection = 0;
    public GCodeCommandTranslation OnGCodeCommandTranslation;
    
    /// <summary>
    /// Wrapper over object to interact with CAM API - progress of calculating toolpath
    /// </summary>
    private readonly ComWrapper<ICamApiTechOperationProgressUpdateHandler> _updateHandlerComWrapper;

    /// <summary>
    /// Object to interact with CAM API - progress of calculating toolpath
    /// </summary>
    private ICamApiTechOperationProgressUpdateHandler UpdateHandler => _updateHandlerComWrapper.Instance;

    /// <summary>
    /// Wrapper over object to interact with CAM API - logger
    /// </summary>
    private readonly ComWrapper<IExtensionLogger> _loggerComWrapper;
    
    /// <summary>
    /// Logger object
    /// </summary>
    public IExtensionLogger? Logger => _loggerComWrapper?.Instance;

    public CuraEngineControlProcess(ICamApiTechOperationProgressUpdateHandler updateHandler, IExtensionInfo? info)
    {
        _updateHandlerComWrapper = new ComWrapper<ICamApiTechOperationProgressUpdateHandler>(updateHandler);

        // save logger
        if (info == null)
            return;
        using var instanceInfoCom = new ComWrapper<IExtensionInstanceInfo>(info.InstanceInfo);
        using var extensionManagerCom = new ComWrapper<IExtensionManager>(instanceInfoCom.Instance?.ExtensionManager);
        _loggerComWrapper = new ComWrapper<IExtensionLogger>(extensionManagerCom.Instance?.Logger);
    }

    public void Dispose()
    {
        _updateHandlerComWrapper.Dispose();
        _loggerComWrapper?.Dispose();
    }

    public void OnProgress(double progress)
    {
        try
        {
            var intProgress = Convert.ToInt32(Math.Floor(progress*100));
            if (intProgress>100)
                intProgress = 100;
            UpdateHandler.SetProgressStatus("", intProgress);
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
            if (tpm==ToolpathParsingMode.tpmGCodeBased)
            {
                if (GCLayout == null)
                    GCLayout = new GCodeLayout();
                GCLayout.AddGCodeBlock(GCode);  
            }
                   
            // StreamWriter swGCode = new StreamWriter("C:\\Users\\Andrew\\Desktop\\NewGCode.txt", true);
            // swGCode.WriteLine("\n------GCODE-------");
            // swGCode.WriteLine(GCode);
            // swGCode.Close();
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
            if (tpm==ToolpathParsingMode.tpmGCodeBased)
            {
                if (GCLayout != null)
                {
                    GCLayout.OnGCodeCommandTranslation = OnGCodeCommandTranslation;
                    GCLayout.AddToCLData(clf, boundingBox, IsOutputFilamentExtruding, FilamentExtrudingLength);
                }
                    
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
        finally
        {
            if (tpm==ToolpathParsingMode.tpmGCodeBased)
            {
                if (GCLayout != null)
                {
                    GCLayout.Clear();
                    GCLayout = null;
                }
                else
                {
                    IsStartCreateToolpath = true;
                    LayerInc = 0;
                    isFeedSectionOpened = false;
                }  
            }
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
            if (tpm==ToolpathParsingMode.tpmGCodeBased)
            {
                if (GCLayout == null)
                    GCLayout = new GCodeLayout();
                GCLayout.AddGCodeBlock(GCodePrefix); 
            }
            else
            {
                IsStartCreateToolpath = true;
                LayerInc = 0;
            }
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
            if (tpm==ToolpathParsingMode.tpmSimplified)
            {
                LayerInc++;
                currentLayerHeight = height;
                var LayerCaption = OnGCodeCommandTranslation("Layer"); 
                var group = LayerCaption + ": " + LayerInc.ToString(); 
                clf.BeginItem(TCLDItemType.aitGroup, group, group);
            }
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
            if (tpm==ToolpathParsingMode.tpmSimplified)
            {
                if (isFeedSectionOpened)
                {
                    clf.EndItem();
                    isFeedSectionOpened = false;
                    LastLineSection = 0;
                }
                clf.EndItem();
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
            if (tpm==ToolpathParsingMode.tpmSimplified)
                startPathSegment = true;
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
            if (tpm==ToolpathParsingMode.tpmSimplified)
            {
                if (isFeedSectionOpened)
                {
                    clf.EndItem();
                    isFeedSectionOpened = false;
                    LastLineSection = 0;
                }
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
            if (tpm==ToolpathParsingMode.tpmSimplified)
            {
                if (IsStartCreateToolpath)
                {
                    IsStartCreateToolpath = false;
                }
                else
                {
                    if (startPathSegment)
                    {           
                        if (lineType!=0 && lineType!=8 && lineType!=9)
                        {
                            LastLineSection = lineType;
                            var feedName = FeedConverter.GetFeedName((int)lineType); 
                            var captionFeed = OnGCodeCommandTranslation(feedName);
                            clf.BeginItem(TCLDItemType.aitGroup, feedName, captionFeed);
                            isFeedSectionOpened = true;
                        }
                              
                        lineFeedrate = lineFeedrate*60;  
                        if (IsOutputAdditionalCLDataParameters) 
                        {
                            var LineTypeCaption = OnGCodeCommandTranslation("LineType");
                            clf.AddPrint("#CuraEngine: " + LineTypeCaption + "=" + lineType.ToString());
                            var LineWidthCaption = OnGCodeCommandTranslation("LineWidth");
                            clf.AddPrint("#CuraEngine: " + LineWidthCaption +"="+lineWidth.ToString());
                            var LineThicknessCaption = OnGCodeCommandTranslation("LineThickness");
                            clf.AddPrint("#CuraEngine: " + LineThicknessCaption +"="+lineThickness.ToString());
                            var LineFeedrateCaption = OnGCodeCommandTranslation("LineFeedrate");             
                            clf.AddPrint("#CuraEngine: " + LineFeedrateCaption + "="+lineFeedrate.ToString());
                        }
                        
                        prevLineType = lineType;
                        prevLineWidth = lineWidth;
                        prevLineThickness = lineThickness; 
                        prevLineFeedrate = lineFeedrate;  
                        prevX = p.X;
                        prevY = p.Y;  
                        startPathSegment = false;
                    }
                    else
                    {                                 
                        var feed = FeedConverter.ConvertToCLDataFeed(lineType); 
                        var prevFeed = FeedConverter.ConvertToCLDataFeed(prevLineType);
                        lineFeedrate = lineFeedrate*60;
                        if (feed != prevFeed || lineFeedrate != prevLineFeedrate)    //check for correctly working of interpolation  
                            clf.OutFeed(feed, lineFeedrate, true);

                        if (LastLineSection!=lineType)
                        {
                            if (lineType!=0 && lineType!=8 && lineType!=9)
                            {
                                LastLineSection = lineType;
                                if (isFeedSectionOpened)
                                {
                                    clf.EndItem();
                                    isFeedSectionOpened = false;
                                }
                                var feedName = FeedConverter.GetFeedName((int)lineType);
                                var captionFeed = OnGCodeCommandTranslation(feedName); 
                                clf.BeginItem(TCLDItemType.aitGroup, feedName, captionFeed);
                                isFeedSectionOpened = true;
                            }
                        }
                        if (prevLineType!=lineType)
                        {
                            if (IsOutputAdditionalCLDataParameters) 
                            {
                                var LineTypeCaption = OnGCodeCommandTranslation("LineType"); 
                                clf.AddPrint("#CuraEngine: " + LineTypeCaption + "="+lineType.ToString());
                            }
                            prevLineType = lineType;
                        }
                        if (prevLineWidth!=lineWidth)
                        {
                            if (IsOutputAdditionalCLDataParameters) 
                            {
                                var LineWidthCaption = OnGCodeCommandTranslation("LineWidth"); 
                                clf.AddPrint("#CuraEngine: " + LineWidthCaption + "="+lineWidth.ToString());
                            }
                            prevLineWidth = lineWidth;
                        }
                        if (prevLineThickness!=lineThickness)
                        {
                            if (IsOutputAdditionalCLDataParameters) 
                            {
                                var LineThicknessCaption = OnGCodeCommandTranslation("LineThickness"); 
                                clf.AddPrint("#CuraEngine: " + LineThicknessCaption + "="+lineThickness.ToString());
                            }
                            prevLineThickness = lineThickness;
                        }            
                        if (prevLineFeedrate!=lineFeedrate)
                        {
                            if (IsOutputAdditionalCLDataParameters) 
                            {
                                var LineFeedrateCaption = OnGCodeCommandTranslation("LineFeedrate"); 
                                clf.AddPrint("#CuraEngine: " + LineFeedrateCaption + "="+lineFeedrate.ToString());
                            }
                            prevLineFeedrate = lineFeedrate;
                        }          
                        
                        var pZ = currentLayerHeight + boundingBox.Min.Z;
                        var pX = p.X;
                        var pY = p.Y;
                        clf.CutTo(new TST3DPoint { X = pX, Y = pY, Z = pZ });
                        prevX = p.X;
                        prevY = p.Y;
                    }
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
        LogItem logItem = default;
        logItem.EventType = (TLogEventType)messageType;
        logItem.message = messageString;
        Logger?.Log(logItem);
    }
    
    bool ICuraEngineControlProcess.IsProcessCancelled()
    {
        return UpdateHandler.IsCancelled;
    }
}