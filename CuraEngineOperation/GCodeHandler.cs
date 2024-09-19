using STMCDFormerTypes;
using STTypes;
using CuraEngineParametersLibrary;
namespace CuraEngineOperation;

public static class FeedConverter
{
    public static int ConvertToCLDataFeed(int typeFeed)
    {
        TSTFeedTypeFlag feed = TSTFeedTypeFlag.ffLongNext;
        switch (typeFeed)
        {
            case 1:
                feed = TSTFeedTypeFlag.ffFinish;
                break;
            case 2:
                feed = TSTFeedTypeFlag.ffEngage;
                break;
            case 3:
                feed = TSTFeedTypeFlag.ffFinish;
                break;
            case 4:
                feed = TSTFeedTypeFlag.ffRetract;
                break;
            case 5:
                feed = TSTFeedTypeFlag.ffFirst;
                break;
            case 6:
                feed = TSTFeedTypeFlag.ffWorking;
                break;
            case 7:
                feed = TSTFeedTypeFlag.ffRetract;
                break;
            case 8:
                feed = TSTFeedTypeFlag.ffNext;
                break;
            case 9:
                feed = TSTFeedTypeFlag.ffLongNext;
                break;
            case 10:
                feed = TSTFeedTypeFlag.ffRetract;
                break;
            case 11:
                feed = TSTFeedTypeFlag.ffPlunge;
                break;
        }
        return (int)feed;
    }
    public static string GetFeedName(int typeFeed)
    {
        var feed = "WALL-OUTER";
        switch (typeFeed)
        {
            case 1:
                feed = "WALL-OUTER";
                break;
            case 2:
                feed = "WALL-INNER";
                break;
            case 3:
                feed = "SKIN";
                break;
            case 4:
                feed = "SUPPORT";
                break;
            case 5:
                feed = "SKIRT";
                break;
            case 6:
                feed = "FILL";
                break;
            case 7:
                feed = "SUPPORT-INFILL";
                break;
            case 8:
                feed = "MOVE-COMBING";
                break;
            case 9:
                feed = "MOVE-RETRACTION";
                break;
            case 10:
                feed = "SUPPORT-INTERFACE";
                break;
            case 11:
                feed = "PRIME-TOWER";
                break;
        }
        return feed;
    }
}
public enum GCodeLineType
{
    ltCommand,
    ltMesh,
    ltFeed,
    ltComment
}
public enum GCodeFeedType
{
    ftNone,
    ftOuterWall,
    ftInnerWall,
    ftSkin,
    ftSupport,
    ftSkirtBrim,
    ftInfill,
    ftSupportInfill,
    ftMoveCombing,
    ftMoveRetraction,
    ftSupportInterface,
    ftPrimeTower
}
public interface IGCodeLine
{
    GCodeLineType LineType();
}
public class GCodeCommand: IGCodeLine
{
    public bool IsXChanged = false;
    public bool IsYChanged = false;
    public bool IsZChanged = false;
    public bool IsFeedValueChanged = false;
    public bool IsMovingFeedValueChanged = false;
    public TST3DBox BoundingBox;
    public double pX;
    public double pY;
    public double pZ;
    public double LastFeedValue;
    public double LastMovingFeedValue;
    string Comment = "";
    public double FilamentExtrudingLength = 100;
    public bool IsOutputFilamentExtruding = false;
    public GCodeCommand(string LineText)
    {
        Parameters = new Dictionary<string, string>();
        ParseLine(LineText);   
    }
    public string CommandName = "";
    public Dictionary<string, string> Parameters; 

    public GCodeLineType LineType()
    {
        return GCodeLineType.ltCommand;
    }
    private bool TryGetDoubleValue(string parameterName, out double parameterValue)
    {
        string value;
        if (Parameters.TryGetValue(parameterName, out value))
        {
            double doubleValue;
            if (DoubleParser.DoubleTryParse(value, out doubleValue))
            {
                parameterValue = doubleValue;
                return true;
            }
        }
        parameterValue = 0;
        return false;
    }
    private void AddFilamentExtrudingToCLData(IST_CLDReceiver? clf, bool isG0)
    {
        double value;
        if (IsOutputFilamentExtruding)
        {
            if (TryGetDoubleValue("E", out value))
            {
                if (FilamentExtrudingLength!=0)
                {
                    value = 100*value/FilamentExtrudingLength;
                    if (IsXChanged || IsYChanged || IsZChanged)
                        clf.OutPower(0, value); 
                    else
                    {
                        if (isG0)
                            clf.AddPrint("EPerFrame: " + value + "%; F="+LastMovingFeedValue+"mm/min");
                        else
                            clf.AddPrint("EPerFrame: " + value + "%; F="+LastFeedValue+"mm/min");
                    }
                }      
            }
        }
    }
    private void AddXCoordinateToCLData(IST_CLDReceiver? clf, double prevX)
    {
        double value;
        if (TryGetDoubleValue("X", out value))
        {
            pX = value;
            IsXChanged = true;
        }          
        else
            pX = prevX;
    }
    private void AddYCoordinateToCLData(IST_CLDReceiver? clf, double prevY)
    {
        double value;
        if (TryGetDoubleValue("Y", out value))
        {
            pY = value;
            IsYChanged = true;
        }           
        else
            pY = prevY;
    }
    public void AddToCLData(IST_CLDReceiver? clf, double prevX, double prevY, double prevZ, GCodeFeedType prevFeedType, double prevFeedValue, bool isStartGCode = false)
    {
        double value;
        bool isAdded = false;
        if (CommandName=="G0")
        {
            isAdded = true;
            AddXCoordinateToCLData(clf, prevX);
            AddYCoordinateToCLData(clf, prevY);

            var feed = 0;
            double feedValue = 0;
            if (TryGetDoubleValue("Z", out value))
            {
                pZ = value + BoundingBox.Min.Z;
                IsZChanged = true;
                feed = FeedConverter.ConvertToCLDataFeed((int)GCodeFeedType.ftMoveRetraction);
            }
            else
            {
                pZ = prevZ;
                feed = FeedConverter.ConvertToCLDataFeed((int)GCodeFeedType.ftMoveCombing);
            }  

            if (TryGetDoubleValue("F", out value)) 
            {
                if (value!=LastMovingFeedValue)
                {
                    IsMovingFeedValueChanged = true;
                    LastMovingFeedValue = value;
                    feedValue = value;
                }        
            }               
            else if (!isStartGCode)
                feedValue = prevFeedValue;

            AddFilamentExtrudingToCLData(clf, true);

            if ((IsXChanged || IsYChanged || IsZChanged) && !isStartGCode)
            {
                clf.OutFeed(feed, feedValue, true);
                clf.CutTo(new TST3DPoint { X = pX, Y = pY, Z = pZ });  
            }         
        }
        else if (CommandName=="G1")
        {
            isAdded = true;
            AddXCoordinateToCLData(clf, prevX);
            AddYCoordinateToCLData(clf, prevY);

            if (TryGetDoubleValue("Z", out value))
            {
                pZ = value + BoundingBox.Min.Z;;
                IsZChanged = true;
            }
            else
                pZ = prevZ;
            
            var feed = 0;
            double feedValue = 0;
            if (TryGetDoubleValue("F", out feedValue))
            {
                if (prevFeedValue!=feedValue)
                {
                    LastFeedValue = feedValue;
                    feed = FeedConverter.ConvertToCLDataFeed((int)prevFeedType);
                    if (prevFeedType!=GCodeFeedType.ftNone)
                        clf.OutFeed(feed, feedValue, true);                
                }
            }

            AddFilamentExtrudingToCLData(clf, false);
                
            if ((IsXChanged || IsYChanged || IsZChanged) && !isStartGCode) 
            {
                if (feedValue!=0)
                    clf.OutFeed(feed, feedValue, true);
                clf.CutTo(new TST3DPoint { X = pX, Y = pY, Z = pZ });                 
            }  
        }
        else if (CommandName=="G4")
        {
            if (TryGetDoubleValue("S", out value))
                clf.AddDelay(value);
            else if (TryGetDoubleValue("P", out value))
            {
                var del = value/1000;
                clf.AddDelay(del);
            }
        }
        else if (CommandName=="G10" || CommandName=="G11" ||
                 CommandName=="M17" || CommandName=="M18" ||
                 CommandName=="M80" || CommandName=="M81" ||
                 CommandName=="M105" || CommandName=="M108" ||
                 CommandName=="M112" || CommandName=="M600")
        {
            clf.AddPrint(CommandName);
        }
        else if (CommandName=="M84")
        {
            var CommandPrint = CommandName+": ";
            if (TryGetDoubleValue("S", out value))
            {
                CommandPrint = CommandPrint + "S="+value+"; ";
            }
            if (TryGetDoubleValue("X", out value))
            {
                CommandPrint = CommandPrint + "X; ";
            }
            if (TryGetDoubleValue("Y", out value))
            {
                CommandPrint = CommandPrint + "Y; ";
            }
            if (TryGetDoubleValue("Z", out value))
            {
                CommandPrint = CommandPrint + "Z; ";
            }
            if (TryGetDoubleValue("E", out value))
            {
                CommandPrint = CommandPrint + "E; ";
            }
            clf.AddPrint(CommandPrint);
        }
        else if (CommandName=="M104" || CommandName=="M109" || CommandName=="M140" || CommandName=="M190" ||
                 CommandName=="M209" || CommandName=="M302")
        {
            if (TryGetDoubleValue("S", out value))
                clf.AddPrint(CommandName+": " + "S="+value);
        }
        else if (CommandName=="M106")
        {
            if (TryGetDoubleValue("S", out value))
            {
                clf.AddCoolant(true, 0);
                clf.AddPrint(CommandName+": " + "S="+value);
            }
        }
        else if (CommandName=="M107")
        {
            clf.AddCoolant(false, 0);
        }
        else if (CommandName=="M200")
        {
            if (TryGetDoubleValue("D", out value))
                clf.AddPrint(CommandName+": " + "D="+value);
        }
        else if (CommandName=="M201" || CommandName=="M202" || CommandName=="M203")
        {
            var CommandPrint = CommandName+": ";
            if (TryGetDoubleValue("X", out value))
            {
                CommandPrint = CommandPrint + "X="+value+"; ";
            }
            if (TryGetDoubleValue("Y", out value))
            {
                CommandPrint = CommandPrint + "Y="+value+"; ";
            }
            if (TryGetDoubleValue("Z", out value))
            {
                CommandPrint = CommandPrint + "Z="+value+"; ";
            }
            if (TryGetDoubleValue("E", out value))
            {
                CommandPrint = CommandPrint + "E="+value+"; ";
            }
            clf.AddPrint(CommandPrint);
        }
        else if (CommandName=="M204")
        {
            var CommandPrint = CommandName+": ";
            if (TryGetDoubleValue("P", out value))
            {
                CommandPrint = CommandPrint + "P="+value+"; ";
            }
            if (TryGetDoubleValue("R", out value))
            {
                CommandPrint = CommandPrint + "R="+value+"; ";
            }
            if (TryGetDoubleValue("T", out value))
            {
                CommandPrint = CommandPrint + "T="+value+"; ";
            }
            clf.AddPrint(CommandPrint);
        }
        else if (CommandName=="M205")
        {
            var CommandPrint = CommandName+": ";
            if (TryGetDoubleValue("X", out value))
            {
                CommandPrint = CommandPrint + "X="+value+"; ";
            }
            if (TryGetDoubleValue("Z", out value))
            {
                CommandPrint = CommandPrint + "Z="+value+"; ";
            }
            if (TryGetDoubleValue("E", out value))
            {
                CommandPrint = CommandPrint + "E="+value+"; ";
            }
            clf.AddPrint(CommandPrint);
        }
        else if (CommandName=="M207")
        {   
            var CommandPrint = CommandName+": ";
            if (TryGetDoubleValue("S", out value))
            {
                CommandPrint = CommandPrint + "S="+value+"; ";
            }
            if (TryGetDoubleValue("F", out value))
            {
                CommandPrint = CommandPrint + "F="+value+"; ";
            }
            if (TryGetDoubleValue("Z", out value))
            {
                CommandPrint = CommandPrint + "Z="+value+"; ";
            }
            clf.AddPrint(CommandPrint);
        }
        else if (CommandName=="M208")
        {
            var CommandPrint = CommandName+": ";
            if (TryGetDoubleValue("S", out value))
            {
                CommandPrint = CommandPrint + "S="+value+"; ";
            }
            if (TryGetDoubleValue("F", out value))
            {
                CommandPrint = CommandPrint + "F="+value+"; ";
            }
            clf.AddPrint(CommandPrint);
        }
        else if (CommandName=="M404")
        {
            if (TryGetDoubleValue("W", out value))
                clf.AddPrint(CommandName+": " + "W="+value);
        }
        if (Comment!="" && isAdded)
            clf.AddComment(Comment);
    }
    void ParseLine(string LineText)
    {
        if (LineText.Contains(";"))
        {
            var ind = LineText.IndexOf(";");
            Comment = LineText.Substring(ind+1, LineText.Length-ind-1);
            LineText = LineText.Remove(ind);
        }
        string[] lines = LineText.Split(' ');
        for(var i=0;i<lines.Length;i++)
        {
            var parameter = lines[i];
            if (i==0)
            {
                CommandName = parameter;
            }
            else
            {
                int index = 0;
                while (index < parameter.Length && !char.IsDigit(parameter[index]) && parameter[index] != '-' && parameter[index] != '+')
                {
                    index++;
                }
                string key = parameter.Substring(0, index);
                string value = "0";
                if (index<parameter.Length)
                    value = parameter.Substring(index);
                Parameters.Add(key, value);
            }
        }
    }

}
public class GCodeComment: IGCodeLine
{
    public string Comment;
    public GCodeComment(string comment_)
    {
        Comment = comment_.Substring(1);
    }
    public GCodeLineType LineType()
    {
        return GCodeLineType.ltComment;
    }
    public void AddToCLData(IST_CLDReceiver? clf)
    {
        if (Comment.Contains("LAYER_COUNT") || Comment.Contains("PRINT.") || Comment.Contains("TIME_ELAPSED"))
            clf.AddComment(Comment);
    }
}
public class GCodeMesh: IGCodeLine
{
    public bool IsStartMesh = true;
    public GCodeMesh(string MeshComment)
    {
        if (MeshComment!=null && MeshComment.Contains("NONMESH"))
            IsStartMesh = false;
    }
    public GCodeLineType LineType()
    {
        return GCodeLineType.ltMesh;
    }
    public void AddToCLData(IST_CLDReceiver? clf)
    {
        
    }
}
public class GCodeFeed: IGCodeLine
{
    public GCodeFeedType FeedType;
    public GCodeFeed(string FeedComment)
    {
        if (FeedComment!=null)
        {
            if (FeedComment.Contains("WALL-OUTER"))
                FeedType = GCodeFeedType.ftOuterWall;
            else if (FeedComment.Contains("WALL-INNER"))
                FeedType = GCodeFeedType.ftInnerWall;
            else if (FeedComment.Contains("SKIN"))
                FeedType = GCodeFeedType.ftSkin;
            else if (FeedComment.Contains("SUPPORT"))
                FeedType = GCodeFeedType.ftSupport;
            else if (FeedComment.Contains("SKIRT"))
                FeedType = GCodeFeedType.ftSkirtBrim;
            else if (FeedComment.Contains("FILL"))
                FeedType = GCodeFeedType.ftInfill;
            else if (FeedComment.Contains("SUPPORT"))
                FeedType = GCodeFeedType.ftSupportInfill;
            else if (FeedComment.Contains("SUPPORT-INTERFACE"))
                FeedType = GCodeFeedType.ftSupportInterface;
            else if (FeedComment.Contains("PRIME-TOWER"))
                FeedType = GCodeFeedType.ftPrimeTower;
        }
    }
    public GCodeLineType LineType()
    {
        return GCodeLineType.ltFeed;
    }
    public void AddToCLData(IST_CLDReceiver? clf)
    {
         clf.OutStandardFeed(FeedConverter.ConvertToCLDataFeed((int)FeedType));
    }
}
public class GCodeBlock
{
    public int LayerInd = -1;
    public bool IsLayerBlock()
    {
        var isLayerBlock = false;
        if (LayerInd>-1)
            isLayerBlock = true;
        return isLayerBlock;
    }
    public List<IGCodeLine> GCodeLines;
    public GCodeBlock(int layerInd)
    {
        LayerInd = layerInd;
        GCodeLines = new List<IGCodeLine>();
    }
    public void AddGCodeLine(string LineText)
    {
        IGCodeLine GCodeLine = null;
        if (LineText.Trim() != "")
        {
            if (LineText.StartsWith(";"))
            {
                if (LineText.StartsWith(";MESH:"))
                    GCodeLine = new GCodeMesh(LineText);
                else if (LineText.StartsWith(";TYPE:"))
                    GCodeLine = new GCodeFeed(LineText);
                else
                    GCodeLine = new GCodeComment(LineText);
            }
            else
                GCodeLine = new GCodeCommand(LineText);
            
            GCodeLines.Add(GCodeLine);
        }
    }
}
public class GCodeLayout
{
    bool isFeedSectionOpened = false;
    double LastX = 0;
    double LastY = 0;
    double LastZ = 0;
    int MeshInd = 1;
    GCodeFeedType LastFeedType = 0;
    double LastFeedValue = 0;
    double LastMovingFeedValue = 0;
    List<GCodeBlock> GCodeBlocks; 
    public GCodeLayout()
    {
        GCodeBlocks = new List<GCodeBlock>();
    }
    public void Clear()
    {
        isFeedSectionOpened = false;
        GCodeBlocks.Clear();
    }
    public void AddGCodeBlock (string GCode)
    {
        try
        {           
            var GCodeLines = GCode.Split("\n");
            GCodeBlock Block = null;
            if (GCodeLines.Length>0)
            {
                if (GCodeLines[0].StartsWith(";LAYER:"))
                {
                    var ind = GCodeLines[0].IndexOf(":");
                    var number = GCodeLines[0].Substring(ind+1, GCodeLines[0].Length-ind-1);
                    var layerNumber = 0;
                    if (int.TryParse(number, out layerNumber))
                        Block = new GCodeBlock(layerNumber);
                }
                else
                    Block = new GCodeBlock(-1);
                for (var i=1; i<GCodeLines.Length; i++)
                {
                    if (i==1 && Block.IsLayerBlock())
                        continue;
                    Block.AddGCodeLine(GCodeLines[i]);
                }
                GCodeBlocks.Add(Block);
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }
    public void AddToCLData(IST_CLDReceiver? clf, TST3DBox BoundingBox, bool IsOutputFilamentExtruding, double FilamentExtrudingLength = 100)
    {
        if (clf!=null)
        {
            if (IsOutputFilamentExtruding)
                clf.AddPrint("EPerFrame: " + FilamentExtrudingLength +"mm");
            for (var i=0; i<GCodeBlocks.Count; i++)
            {
                var isStartGCode = false;
                if (i==0)
                    isStartGCode = true;
                var Block = GCodeBlocks[i];
                if (Block.IsLayerBlock())
                {
                    var ind = Block.LayerInd+1;
                    var layer = "Layer: " + ind; 
                    clf.BeginItem(TST_CLDItemType.itGroup, layer, layer);
                }
                       
                for (var j=0; j<Block.GCodeLines.Count; j++)
                {
                    var Line = Block.GCodeLines[j];
                    switch (Line.LineType())
                    {
                        case GCodeLineType.ltCommand:
                            var CommandLine = (GCodeCommand)Line;
                            CommandLine.BoundingBox = BoundingBox;
                            CommandLine.FilamentExtrudingLength = FilamentExtrudingLength;
                            CommandLine.IsOutputFilamentExtruding = IsOutputFilamentExtruding;
                            if (CommandLine.CommandName=="G0")
                                CommandLine.AddToCLData(clf, LastX, LastY, LastZ, LastFeedType, LastMovingFeedValue, isStartGCode);
                            else    
                                CommandLine.AddToCLData(clf, LastX, LastY, LastZ, LastFeedType, LastFeedValue, isStartGCode);
                            if (CommandLine.IsXChanged)
                                LastX = CommandLine.pX;
                            if (CommandLine.IsYChanged)
                                LastY = CommandLine.pY;
                            if (CommandLine.IsZChanged)
                                LastZ = CommandLine.pZ;
                            if (CommandLine.IsFeedValueChanged)
                                LastFeedValue = CommandLine.LastFeedValue;
                            if (CommandLine.IsMovingFeedValueChanged)
                                LastMovingFeedValue = CommandLine.LastMovingFeedValue;
                            break; 
                        case GCodeLineType.ltComment:
                            var CommentLine = (GCodeComment)Line;
                            CommentLine.AddToCLData(clf);
                            break; 
                        case GCodeLineType.ltFeed:
                            var FeedLine = (GCodeFeed)Line;
                            if (!isFeedSectionOpened)
                            {
                                LastFeedType = FeedLine.FeedType;
                                var feedName = FeedConverter.GetFeedName((int)LastFeedType); 
                                clf.BeginItem(TST_CLDItemType.itGroup, feedName, feedName);
                                isFeedSectionOpened = true;
                            }    
                            if (LastFeedType != FeedLine.FeedType)
                            {
                                LastFeedType = FeedLine.FeedType;
                                if (isFeedSectionOpened)
                                {
                                    clf.EndItem();
                                    isFeedSectionOpened = false;
                                }       
                                var feedName = FeedConverter.GetFeedName((int)LastFeedType); 
                                clf.BeginItem(TST_CLDItemType.itGroup, feedName, feedName);
                                isFeedSectionOpened = true;
                            }  
                            //FeedLine.AddToCLData(clf);          
                            break; 
                        case GCodeLineType.ltMesh:
                            var MeshLine = (GCodeMesh)Line;
                            if (!MeshLine.IsStartMesh)
                            {
                                if (isFeedSectionOpened)
                                {
                                    isFeedSectionOpened = false;
                                    clf.EndItem();
                                }  
                            }
                            MeshLine.AddToCLData(clf);
                            break; 
                    }
                }  

                if (isFeedSectionOpened)
                {
                    clf.EndItem();
                    isFeedSectionOpened = false;
                }
                if (Block.IsLayerBlock())
                {
                    clf.EndItem();
                }               
            }
        }
    }
}