namespace CuraEngineOperation;

using System;
using STTypes;
using STOperationTypes;
using STMCDFormerTypes;
using STCuttingToolTypes;
using System.Runtime.InteropServices;
using CuraConnectionInterface;
using STMeshTypes;
using CuraEngineParametersLibrary;
using CuraEngineNetWrapper;
using STCustomPropTypes;
using CAMAPI.Extensions;
using CAMAPI.ResultStatus;
using CAMAPI.UIDialogs;
using Microsoft.Win32;
using System.Reflection;
using System.Text.Json;
using STModelFormerTypes;
using STXMLPropTypes;
using CAMAPI.Logger;
using CAMAPI.Application;
using System.Globalization;
public enum ToolpathParsingMode
{
    tpmSimplified,
    tpmGCodeBased
}
public delegate string GCodeCommandTranslation(string label = "");
public class CuraEngineControlProcess : ICuraEngineControlProcess
{
    public double FilamentExtrudingLength = 100; 
    public ToolpathParsingMode tpm = ToolpathParsingMode.tpmSimplified;
    public bool IsOutputAdditionalCLDataParameters = false;
    public bool IsOutputFilamentExtruding = false;
    public IExtensionLogger? Logger = null;
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
    public IST_CLDReceiver? clf;
    public IST_UpdateHandler? updateHandler;
    bool isFeedSectionOpened = false;
    int LastLineSection = 0;
    public GCodeCommandTranslation OnGCodeCommandTranslation;
    public void OnProgress(double progress)
    {
        try
        {
            var intProgress = Convert.ToInt32(Math.Floor(progress*100));
            if (intProgress>100)
                intProgress = 100;
            updateHandler.SetProgressStatus("", intProgress);
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
                clf.BeginItem(TST_CLDItemType.itGroup, group, group);
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
                            clf.BeginItem(TST_CLDItemType.itGroup, feedName, captionFeed);
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
                                clf.BeginItem(TST_CLDItemType.itGroup, feedName, captionFeed);
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
        Logger.Log(logItem);
    }
    bool ICuraEngineControlProcess.IsProcessCancelled()
    {
        return updateHandler.IsCancelled;
    }
}
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
public class CuraEngineToolpath : IST_Operation, IST_OperationSolver, IExtension, IExtensionGlobal, IExtensionOperationSolver
{
    public CuraEngineToolpath()
    {

    }
    ~CuraEngineToolpath()
    {
        helpers = null;
        if (UserLangCatalog!=null)
        {
            UserLangCatalog.Clear();
            UserLangCatalog = null;
        }
        if (LangCatalog!=null)
        {
            LangCatalog.Clear();
            LangCatalog = null;
        }
    }
    // ------------ IExtension --------------------------
    private IExtensionInfo? fInfo = null;
    public IExtensionInfo? Info
    {
        get => fInfo;
        set
        {
            if (fInfo != null)
            {
                Marshal.FinalReleaseComObject(fInfo);
                fInfo = null;
            }
            fInfo = value;
        }
    }

    IST_OpContainer? opContainer;
    IST_UpdateHandler? updateHandler;
    IST_CLDReceiver? clf;
    private CuraEngineControlProcess CEControlProcess = null;
    private ParamsReceiver CEParamsReceiver = null;
    IST_CustomPropHelpers? helpers;
    string SearchFilter = "";
    string CuraPath = "";
    string UserCuraPathFromJSon = "";
    string AutoCuraPath = "";
    string UserCuraPath = "";
    bool isOperationCreating = true;
    bool UserPathEnabled = false;
    string WarningMessage = "Cura not found installed. Set path to CuraEngine.exe manually.\nParameters tab -> Set Cura path";
    Dictionary<string, string> LangCatalog = null;
    Dictionary<string, string> UserLangCatalog = null;
    IST_SimplePropIterator? ParamSimplePropIterator = null;
    string CurrentLang = "en-US";
    // ------------ IST_Operation --------------------------

    public void Create(IST_OpContainer Container) {
        opContainer = Container;
        CEControlProcess = new CuraEngineControlProcess(); 
        CEParamsReceiver = new ParamsReceiver();
        GetLangApplication();

        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        string UserLangPath = Path.GetDirectoryName(assemblyLocation)+"\\UserLocalization\\";
        UserLangCatalog = ReadTranslations(UserLangPath, true);
        WarningMessage = GetLabelTranslation("Path_warning_message", WarningMessage);
        GetPathToCura();
        if (!CheckCuraPath())
        {
            Console.WriteLine(WarningMessage);
            Info.InstanceInfo.ExtensionManager.Logger.Warning(WarningMessage);  
            ShowMessageBox(WarningMessage, TMessageDialogType.mdtWarning, (ushort)1, TUIButtonType.btOk, "");
        }
        else
        {
            CEParamsReceiver.CEParameters.CuraPath = CuraPath;
            CEParamsReceiver.CEParameters.ReadAllParametersAndConfigs(); 
        }
        var extension = Info.InstanceInfo.ExtensionManager.GetSingletonExtension("Extension.CustomPropHelpers", out TResultStatus ret);
        helpers = (IST_CustomPropHelpers)extension;    
        CEControlProcess.Logger = Info.InstanceInfo.ExtensionManager.Logger;
        Info.InstanceInfo.ExtensionManager.Logger.Info("Cura operation is created.");  
    }
    private void GetLangApplication()
    {
        var appSingleton = fInfo.InstanceInfo.ExtensionManager.GetSingletonExtension("Extension.Global.Singletons.Application", out _);
        var appApi = appSingleton as ICamApiApplicationSingleton;
        var app = appApi.GetApplication(out _); 
        var langCode = app.LanguageCode;
        if (langCode!=null && langCode!=0)
        {
            CultureInfo cultureInfo = new CultureInfo(langCode);
            if (cultureInfo!=null && cultureInfo.Name!="")
                CurrentLang = cultureInfo.Name;
        }
    }
    private Dictionary<string, string> ReadTranslations(string FilePath, bool isUserTranslations = false)
    {
        var isFileExists = false;
        var path = "";
        var filename = "";
        if (isUserTranslations)
            filename = ".po";
        else
            filename = "\\fdmprinter.def.json.po";  

        path = FilePath + CurrentLang + filename;
        if (File.Exists(path))
        {
            isFileExists = true;
        }
        else
        {
            var lang = CurrentLang.Replace("-", "_");
            path = FilePath + lang + filename;
            if (File.Exists(path))
            {
                isFileExists = true;
            }
        }

        Dictionary<string, string> translations = null;
        if (isFileExists)
        {
            translations = new Dictionary<string, string>();
            var reader = new StreamReader(path);
            var line = reader.ReadLine();
            var key = "";
            var value = "";
            var hasKey = false;
            var hasValue = false;
            while (line!=null)
            {
                if (line.StartsWith("msgctxt \""))
                {
                    hasKey = true;
                    key = line.Substring(9, line.Length-10).Trim().ToLower();
                }
                if (line.StartsWith("msgstr \""))
                {
                    hasValue = true;
                    value = line.Substring(8, line.Length-9).Trim();
                }
                if (line.Trim()=="")
                {
                    hasKey = false;
                    hasValue = false;
                }
                if (hasKey && hasValue)
                    translations[key] = value;
                line = reader.ReadLine();
            }
        }  
        return translations;
    }
    private string GetTranslation(string id, string label = "")
    {
        var result = "";
        id = id.ToLower();
        label = label.ToLower();
        if (UserLangCatalog!=null)
        {
            if (label!="")
            {
                if (UserLangCatalog.TryGetValue(label, out result))
                    return result;
            }
            if (UserLangCatalog.TryGetValue(id, out result))
                return result;
        }
        if (LangCatalog!=null)
            LangCatalog.TryGetValue(id, out result);
        return result;
    }
    private string GetLabelTranslation(string id, string label = "")
    {
        var label_ = "";
        if (label!="")
            label_ = label + " label";
        var caption = GetTranslation(id + " label", label_);
        if (caption==null || caption=="")
        {
            if (label!="")
                caption = label;
            else
                caption = id;
        }
        return caption;
    }
    private string GetGCodeCommandTranslation(string label = "")
    {
        var label_ = "";
        if (label!="")
            label_ = label + " CLData_Command";
        var caption = GetTranslation(label_);
        if (caption==null || caption=="")
        {
            if (label!="")
                caption = label;
        }
        return caption;
    }
    private string GetLabelTranslation(string id, string label, string description)
    {
        var desc = GetTranslation(id + " description", label + " description");
        if (desc==null || desc=="")
        {
            desc = description;
        }
        return desc;
    }
    private string GetEnumsTranslation(string ParamId, string ParamLabel, string enumItemId, string enumItemName)
    {
        var desc = GetTranslation(ParamId + " option " + enumItemName, ParamLabel + " option " + enumItemName);
        if (desc==null || desc=="")
        {
            desc = GetTranslation(ParamId + " option " + enumItemId, ParamLabel + " option " + enumItemId);
            if (desc==null || desc=="")
                desc = enumItemName;
        }
        return desc;
    }
    private int ShowMessageBox(string Msg, TMessageDialogType DlgType, ushort Buttons, TUIButtonType DefaultButton, string ATitle)
    {
        var extension = Info.InstanceInfo.ExtensionManager.GetSingletonExtension("Extension.UIDialogs.Core", out TResultStatus ret);
        var uiDialog = (ICAMAPI_UIDialogsHelper)extension;
        ushort buttons = (ushort)TUIButtonTypeFlags.btfOk;
        return uiDialog.MessageBox(Msg, DlgType, buttons, DefaultButton, ATitle);
    }
    private void SetToolParameterization(string paramName)
    {
        if (IsAutoToolParameterization)
        {
            var paramNameInCAMSystem = "";
            if (paramName=="layer_height")
            {
                paramNameInCAMSystem = "WorkingLength";
            }
            else
            {
                paramNameInCAMSystem = "Diameter";
            }
            var xmlProp = opContainer.XMLProp;
            var xmlParam = xmlProp.Ptr["ToolSection.Tools(0).Properties." + paramNameInCAMSystem];
            var value = CEParamsReceiver.CEParameters.GetValue(paramName, true);
            double doubleValue;
            if (DoubleParser.DoubleTryParse(value, out doubleValue))
            {
                xmlParam.ValueAsDouble = doubleValue; 
            }
        }
    }
    private bool CuraEnginePathCorrect(string path)
    {
        var CEPath = @Path.GetDirectoryName(path) + @"\CuraEngine.exe";
        if (File.Exists(CEPath))
            return true;
        return false;
    }
    private bool CheckCuraPath()
    {
        UserPathEnabled = opContainer.XMLProp.Ptr["CuraPath.SetPath"].ValueAsBoolean;
        if (AutoCuraPath=="" || !Directory.Exists(AutoCuraPath) || !CuraEnginePathCorrect(AutoCuraPath) || UserPathEnabled)
        {
            var UserCuraPathFromXML = opContainer.XMLProp.Ptr["CuraPath.Path"].ValueAsString;
            if (UserCuraPathFromXML!="" && File.Exists(UserCuraPathFromXML) && CuraEnginePathCorrect(UserCuraPathFromXML))
            {
                CuraPath = @Path.GetDirectoryName(UserCuraPathFromXML) + @"\";
                UserPathEnabled = true;
                opContainer.XMLProp.Ptr["CuraPath.SetPath"].ValueAsBoolean = UserPathEnabled;
                opContainer.XMLProp.Ptr["CuraPath.Path"].ValueAsString = @UserCuraPathFromXML;
                if (@UserCuraPathFromXML!=@UserCuraPath)
                {
                    UserCuraPath = UserCuraPathFromXML;
                    UpdateCuraPathJson(UserCuraPathFromXML);
                }          
            }
            else
            {
                UserPathEnabled = opContainer.XMLProp.Ptr["CuraPath.SetPath"].ValueAsBoolean; 
                if (!UserPathEnabled || UserCuraPathFromXML=="" || !File.Exists(UserCuraPathFromXML) || !CuraEnginePathCorrect(UserCuraPathFromXML))
                {
                    if (UserCuraPathFromXML!=null)
                        UserCuraPath = UserCuraPathFromXML;
                    return false;
                }
                else
                {
                    CuraPath = @Path.GetDirectoryName(UserCuraPathFromXML) + @"\";
                    if (@UserCuraPathFromXML!=@UserCuraPath)
                    {
                        UserCuraPath = UserCuraPathFromXML;
                        UpdateCuraPathJson(UserCuraPathFromXML);
                    }        
                }
            }
        }
        else
        {
            UserCuraPath = UserCuraPathFromJSon;
            CuraPath = AutoCuraPath;
        }
        if (LangCatalog==null)
        {
            var LangPath = CuraPath + "share\\cura\\resources\\i18n\\";
            LangCatalog = ReadTranslations(LangPath);
        } 
        return true;
    }
    private void UpdateCuraPathJson(string UserPath)
    {
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        string assemblyDirectory = Path.GetDirectoryName(assemblyLocation)+"\\";
        string CuraSettingsJsonFile = assemblyDirectory+"CuraSettings.json";
        if (File.Exists(CuraSettingsJsonFile))
        {
            File.Delete(CuraSettingsJsonFile);
            CuraPath = ReadPathFromRegistry();
            var curaSettings = new CuraSettingsJson();
            curaSettings.Name = "CuraSettings";
            curaSettings.Settings = new SettingsJson
            {
                PathToCura = AutoCuraPath,
                UserPathToCura = UserPath
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonString = JsonSerializer.Serialize(curaSettings, options);
            File.WriteAllText(CuraSettingsJsonFile, jsonString);
        }
    }
    private void GetPathToCura()
    {
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        string assemblyDirectory = Path.GetDirectoryName(assemblyLocation)+"\\";
        string CuraSettingsJsonFile = assemblyDirectory+"CuraSettings.json";
        if (!File.Exists(CuraSettingsJsonFile))
        {
            AutoCuraPath = ReadPathFromRegistry();
            var curaSettings = new CuraSettingsJson();
            curaSettings.Name = "CuraSettings";
            curaSettings.Settings = new SettingsJson
            {
                PathToCura = AutoCuraPath,
                UserPathToCura = ""
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonString = JsonSerializer.Serialize(curaSettings, options);
            File.WriteAllText(CuraSettingsJsonFile, jsonString);
        }
        else
        {
            string jsonContent = File.ReadAllText(CuraSettingsJsonFile);
            JsonDocument document = JsonDocument.Parse(jsonContent);
            JsonElement root = document.RootElement;
            if (root.TryGetProperty("settings", out JsonElement settingsElement))  
            {
                foreach (JsonProperty childProperty in settingsElement.EnumerateObject())
                {   
                    if (childProperty.Name=="PathToCura")
                    {
                        var value = childProperty.Value.GetString();
                        AutoCuraPath = value;
                    }
                    if (childProperty.Name=="UserPathToCura")
                    {
                        var value = childProperty.Value.GetString();
                        UserCuraPath = value;
                        UserCuraPathFromJSon = value;
                        opContainer.XMLProp.Ptr["CuraPath.Path"].ValueAsString = @UserCuraPath;
                    }
                }
            }
            if (CuraPath.Trim()=="")
            {
                AutoCuraPath = ReadPathFromRegistry();
                string json = File.ReadAllText(CuraSettingsJsonFile);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                CuraSettingsJson curaSettings = JsonSerializer.Deserialize<CuraSettingsJson>(json, options);
                curaSettings.Settings.PathToCura = AutoCuraPath;
                string updatedJson = JsonSerializer.Serialize(curaSettings, options);
                File.WriteAllText(CuraSettingsJsonFile, updatedJson);
            }
        }
    }
    private string ReadPathFromRegistry()
    {
        var path = "";
        RegistryKey classesRootKey = Registry.ClassesRoot;
        RegistryKey curaKey = classesRootKey.OpenSubKey("cura\\DefaultIcon");

        curaKey = null;
        if (curaKey!=null)
        {
            path = curaKey.GetValue("").ToString();
            if (path!="")
                path = path.Remove(path.LastIndexOf("\\") + 1);
        } 
        if (path=="")
        {
            RegistryKey curaPackageKey = classesRootKey.OpenSubKey("curapackage\\shell\\open_curapackage\\command");
            if (curaPackageKey!=null)
            {
                path = curaPackageKey.GetValue("").ToString();
                if (path!="") 
                {
                    path = path.Replace("\"", "");
                    path = path.Remove(path.LastIndexOf("\\") + 1);
                }       
            }
        }
        if (path=="")
        {
            RegistryKey curaModelKey = classesRootKey.OpenSubKey("Cura.model\\DefaultIcon");
            if (curaModelKey!=null)
            {
                path = curaModelKey.GetValue("").ToString();
                if (path!="") 
                    path = path.Remove(path.LastIndexOf("\\") + 1);
            }      
        }
        return path;
    }

    public void ClearReferences() {
        if (opContainer != null)
        {
            Marshal.FinalReleaseComObject(opContainer);
            opContainer = null;
        }
        if (helpers!=null)
        {            
            Marshal.FinalReleaseComObject(helpers);
            helpers = null;
        }
        if (ParamSimplePropIterator != null)
        {
            Marshal.FinalReleaseComObject(ParamSimplePropIterator);
            ParamSimplePropIterator = null;
        }
        if (CEControlProcess.Logger != null)
        {
            Marshal.FinalReleaseComObject(CEControlProcess.Logger);
            CEControlProcess.Logger = null;
        }
    }
    private void UpdateModelFormerItems()
	{
        IST_ModelFormerSupportedItems ja = new TModelFormerItems();
        Type interfaceType = typeof(IST_MeshesArrayModelItem);
        GuidAttribute guidAttr = (GuidAttribute)Attribute.GetCustomAttribute(interfaceType, typeof(GuidAttribute));
        ja.AddItem("Job Surfaces", new Guid(guidAttr.Value), "", "Job Surfaces", "", "", false, null);
        var mfJa = opContainer.MFJobAssignment;
        mfJa.SupportedItems = ja;
		mfJa.FillItemsBySupportedItems();
        Marshal.FinalReleaseComObject(mfJa);
    }
    public void InitModelFormers() {
        UpdateModelFormerItems();
    }

    public void SaveToStream(STXMLPropTypes.IStream Stream) {

    }

    public void LoadFromStream(STXMLPropTypes.IStream Stream) {

    }
    private void AddUserParameter(string ParamId, string Value, bool isGlobalParameter)
    {
        CEParamsReceiver.CEParameters.AddUserParameter(ParamId, Value, isGlobalParameter);
        SaveUserParameterToXML();
    }
    private void RemoveUserParameter(string ParamId)
    {
        CEParamsReceiver.CEParameters.UserParameters.Remove(ParamId);
        SaveUserParameterToXML();
    }
    private void SaveUserParameterToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var ar = XMLProp.Arr["CuraUserParameterArray"];
            if (ar!=null)
            {
                ar.Clear();
                var UserParameters = CEParamsReceiver.CEParameters.UserParameters;
                foreach (KeyValuePair<string, string> keyValueElement in UserParameters)
                {
                    var p = ar.CreateNewItem("Parameter");
                    p.Str["Name"] = keyValueElement.Key;
                    p.Str["Value"] = keyValueElement.Value;
                    ar.AddItem(p);
                } 
            }
        }
    }
    private void SaveManufacturerToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var manufacturer = XMLProp.Ptr["GeneralParameters.Manufacturer"]; 
            if (manufacturer!=null && CEParamsReceiver.CEParameters.SelectedMachineBrand!=null)
                manufacturer.ValueAsString = CEParamsReceiver.CEParameters.SelectedMachineBrand.name;  
            else
                manufacturer.ValueAsString = "";
        }
    }
    private void SaveMachineToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var machine = XMLProp.Ptr["GeneralParameters.Machine"]; 
            if (machine!=null && CEParamsReceiver.CEParameters.SelectedMachine!=null)
                machine.ValueAsString = CEParamsReceiver.CEParameters.SelectedMachine.id;  
            else
                machine.ValueAsString = "";
        }
    }
    private void SaveExtruderToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var extruder = XMLProp.Ptr["GeneralParameters.Extruder"]; 
            if (extruder!=null && CEParamsReceiver.CEParameters.SelectedExtruder!=null)
                extruder.ValueAsString = CEParamsReceiver.CEParameters.SelectedExtruder.id; 
            else
                extruder.ValueAsString = "";
        }
    }
    private void SaveMaterialBrandToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var materialBrand = XMLProp.Ptr["GeneralParameters.MaterialBrand"]; 
            if (materialBrand!=null && CEParamsReceiver.CEParameters.SelectedMaterialBrand!=null)
                materialBrand.ValueAsString = CEParamsReceiver.CEParameters.SelectedMaterialBrand.name;    
            else
                materialBrand.ValueAsString = "";
        }
    }
    private void SaveMaterialToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var material = XMLProp.Ptr["GeneralParameters.Material"]; 
            if (material!=null && CEParamsReceiver.CEParameters.SelectedMaterial!=null)
                material.ValueAsString = CEParamsReceiver.CEParameters.SelectedMaterial.id;    
            else
                material.ValueAsString = "";
        }
    }
    private void SaveVariantToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var variant = XMLProp.Ptr["GeneralParameters.Variant"]; 
            if (variant!=null && CEParamsReceiver.CEParameters.SelectedVariant!=null)
                variant.ValueAsString = CEParamsReceiver.CEParameters.SelectedVariant.Name; 
            else
                variant.ValueAsString = "";
        }
    }
    private void SaveProfileToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var intentCategory = XMLProp.Ptr["GeneralParameters.Profile"]; 
            if (intentCategory!=null && CEParamsReceiver.CEParameters.SelectedIntentCategory!=null)
                intentCategory.ValueAsString = CEParamsReceiver.CEParameters.SelectedIntentCategory.IntentCategoryName;
            else    
                intentCategory.ValueAsString = "";
        }
    }
    private void SaveQualityToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var resolution = XMLProp.Ptr["GeneralParameters.Quality"]; 
            if (resolution!=null && CEParamsReceiver.CEParameters.SelectedQuality!=null)
                resolution.ValueAsString = CEParamsReceiver.CEParameters.SelectedQuality.FileName;
            else    
                resolution.ValueAsString = "";
        }
    }
    private void SaveShowCustomParametersToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var showCustomParameters = XMLProp.Ptr["GeneralParameters.ShowCustomParameters"]; 
            if (showCustomParameters!=null)
                showCustomParameters.ValueAsBoolean = CEParamsReceiver.CEParameters.IsShowCustomParameters;
        }
    }
    private void SaveSettingVisibilityToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var settingVisibility = XMLProp.Ptr["GeneralParameters.SettingVisibility"]; 
            if (settingVisibility!=null && CEParamsReceiver.CEParameters.SelectedSettingVisibilities!="")
                settingVisibility.ValueAsString = CEParamsReceiver.CEParameters.SelectedSettingVisibilities;
            else    
                settingVisibility.ValueAsString = "";
        }
    }
    private void SaveAutoToolParameterizationToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var AutoToolParameterization = XMLProp.Ptr["GeneralParameters.AutoToolParameterization"]; 
            if (AutoToolParameterization!=null)
                AutoToolParameterization.ValueAsBoolean = IsAutoToolParameterization;
            if (isOperationCreating && IsAutoToolParameterization)
            {
                isOperationCreating = false;
                SetToolParameterization("layer_height");
                SetToolParameterization("line_width"); 
            }
        }
    }
    private void SaveFilamentExtrudingLengthToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var fel = XMLProp.Ptr["GeneralParameters.FilamentExtrudingLength"]; 
            if (fel!=null)
                fel.ValueAsDouble = FilamentExtrudingLength;
        }
    }
    private void SaveOutputAdditionalParametersToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var OutputAdditionalParameters = XMLProp.Ptr["GeneralParameters.OutputAdditionalParameters"]; 
            if (OutputAdditionalParameters!=null)
                OutputAdditionalParameters.ValueAsBoolean = IsOutputAdditionalCLDataParameters;
        }
    }
    private void SaveOutputFilamentExtrudingToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
    if (XMLProp==null)
        XMLProp = opContainer.XMLProp;
    if (CheckCuraPath())
    {
        var OutputFilamentExtruding = XMLProp.Ptr["GeneralParameters.OutputFilamentExtruding"]; 
        if (OutputFilamentExtruding!=null)
            OutputFilamentExtruding.ValueAsBoolean = IsOutputFilamentExtruding;
    }
    }
    private void SaveToolpathParsingModeToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp = null) {
        if (XMLProp==null)
            XMLProp = opContainer.XMLProp;
        if (CheckCuraPath())
        {
            var tpMode = XMLProp.Ptr["GeneralParameters.ToolpathParsingMode"]; 
            if (tpMode!=null)
            {
                if (tpm==ToolpathParsingMode.tpmSimplified)
                    tpMode.ValueAsString = "Simplified";
                else    
                    tpMode.ValueAsString = "GCodeBased";
            }
                
        }
    }
    public void SaveToXML(STXMLPropTypes.IST_XMLPropPointer XMLProp) {

        SaveUserParameterToXML(XMLProp);
        SaveManufacturerToXML(XMLProp);
        SaveMachineToXML(XMLProp);
        SaveExtruderToXML(XMLProp);
        SaveMaterialBrandToXML(XMLProp);
        SaveMaterialToXML(XMLProp);
        SaveVariantToXML(XMLProp);
        SaveProfileToXML(XMLProp);
        SaveQualityToXML(XMLProp);
        SaveShowCustomParametersToXML(XMLProp);
        SaveSettingVisibilityToXML(XMLProp);
        SaveAutoToolParameterizationToXML(XMLProp);
        SaveToolpathParsingModeToXML(XMLProp);
        SaveOutputAdditionalParametersToXML(XMLProp);
        SaveOutputFilamentExtrudingToXML(XMLProp);
        SaveFilamentExtrudingLengthToXML(XMLProp);
        XMLProp.Ptr["CuraPath.SetPath"].ValueAsBoolean = UserPathEnabled;
        XMLProp.Ptr["CuraPath.Path"].ValueAsString = UserCuraPath;
    }
    private void LoadAdhesionValueFromXML(STXMLPropTypes.IST_XMLPropPointer XMLProp)
    {
        var Adhesion = XMLProp.Ptr["GeneralParameters.Adhesion"]; 
        if (Adhesion!=null)
        {
            Parameter param;
            var value = Adhesion.ValueAsBoolean;
            if (CEParamsReceiver.CEParameters.GlobalParams.TryGetValue("adhesion_type", out param))
            {
                if (!value)
                    CEParamsReceiver.CEParameters.UserParameters[param.id] = "none";
            }
        }
    }
    public void LoadFromXML(STXMLPropTypes.IST_XMLPropPointer XMLProp) {
        if (CheckCuraPath())
        {
            UserPathEnabled = XMLProp.Ptr["CuraPath.SetPath"].ValueAsBoolean;
            UserCuraPath = XMLProp.Ptr["CuraPath.Path"].ValueAsString;
            var ar = XMLProp.Arr["CuraUserParameterArray"];
            if (ar!=null)
            {
                CEParamsReceiver.CEParameters.UserParameters.Clear();
                LoadAdhesionValueFromXML(XMLProp);
                for (int i=0; i<ar.TopItem+1; i++)
                {
                        var param = ar[i];
                        var name = param.Str["Name"];
                        var value = param.Str["Value"];
                        CEParamsReceiver.CEParameters.UserParameters[name] = value;
                }        
            }
            else    
                LoadAdhesionValueFromXML(XMLProp);

            var manufacturer = XMLProp.Ptr["GeneralParameters.Manufacturer"]; 
            if (manufacturer!=null && manufacturer.ValueAsString!=null && manufacturer.ValueAsString!="")
                CEParamsReceiver.CEParameters.SetSelectedMachineBrand(manufacturer.ValueAsString);

            var machine = XMLProp.Ptr["GeneralParameters.Machine"]; 
            if (machine!=null && machine.ValueAsString!=null && machine.ValueAsString!="")
                CEParamsReceiver.CEParameters.SetSelectedMachine(machine.ValueAsString);

            var extruder = XMLProp.Ptr["GeneralParameters.Extruder"]; 
            if (extruder!=null && extruder.ValueAsString!=null && extruder.ValueAsString!="")
                CEParamsReceiver.CEParameters.SetSelectedExtruder(extruder.ValueAsString);

            var materialBrand = XMLProp.Ptr["GeneralParameters.MaterialBrand"]; 
            if (materialBrand!=null && materialBrand.ValueAsString!=null && materialBrand.ValueAsString!="")
                CEParamsReceiver.CEParameters.SetSelectedMaterialBrand(materialBrand.ValueAsString);

            var material = XMLProp.Ptr["GeneralParameters.Material"]; 
            if (material!=null && material.ValueAsString!=null && material.ValueAsString!="")
                CEParamsReceiver.CEParameters.SetSelectedMaterial(material.ValueAsString);

            var variant = XMLProp.Ptr["GeneralParameters.Variant"]; 
            if (variant!=null && variant.ValueAsString!=null && variant.ValueAsString!="")
                CEParamsReceiver.CEParameters.SetSelectedVariant(variant.ValueAsString);

            var intentCategory = XMLProp.Ptr["GeneralParameters.Profile"]; 
            if (intentCategory!=null && intentCategory.ValueAsString!=null && intentCategory.ValueAsString!="")
                CEParamsReceiver.CEParameters.SetSelectedIntentCategory(intentCategory.ValueAsString);

            var resolution = XMLProp.Ptr["GeneralParameters.Quality"]; 
            if (resolution!=null && resolution.ValueAsString!=null && resolution.ValueAsString!="")
                CEParamsReceiver.CEParameters.SetSelectedQuality(resolution.ValueAsString);

            var showCustomParameters = XMLProp.Ptr["GeneralParameters.ShowCustomParameters"]; 
            if (showCustomParameters!=null)
                CEParamsReceiver.CEParameters.IsShowCustomParameters = showCustomParameters.ValueAsBoolean;

            var settingVisibility = XMLProp.Ptr["GeneralParameters.SettingVisibility"]; 
            if (settingVisibility!=null && settingVisibility.ValueAsString!=null && settingVisibility.ValueAsString!="")
                CEParamsReceiver.CEParameters.SelectedSettingVisibilities = settingVisibility.ValueAsString;

            var AutoToolParameterization = XMLProp.Ptr["GeneralParameters.AutoToolParameterization"]; 
            if (AutoToolParameterization!=null)
                IsAutoToolParameterization = AutoToolParameterization.ValueAsBoolean;

            var TPMode = XMLProp.Ptr["GeneralParameters.ToolpathParsingMode"]; 
            if (TPMode!=null)
            {
                if (TPMode.ValueAsString=="Simplified")
                    tpm = ToolpathParsingMode.tpmSimplified;
                else
                    tpm = ToolpathParsingMode.tpmGCodeBased;
            }

            var OutputAdditionalParameters = XMLProp.Ptr["GeneralParameters.OutputAdditionalParameters"]; 
            if (OutputAdditionalParameters!=null)
                IsOutputAdditionalCLDataParameters = OutputAdditionalParameters.ValueAsBoolean;

            var OutputFilamentExtruding = XMLProp.Ptr["GeneralParameters.OutputFilamentExtruding"]; 
            if (OutputFilamentExtruding!=null)
                IsOutputFilamentExtruding = OutputFilamentExtruding.ValueAsBoolean;

            var fel = XMLProp.Ptr["GeneralParameters.FilamentExtrudingLength"]; 
            if (fel!=null)
                FilamentExtrudingLength = fel.ValueAsDouble;

            CEParamsReceiver.CEParameters.UpdateAllParameters();
        }
    }

    public void SetDefaultParameters(IST_OpContainer CopyFrom) {

    }

    public bool IsToolTypeSupported(TSTMillToolType tt) {
        return true;
    }

    public bool IsCorrectParameters() {
        return true;
    }

    public void ResetAll() {

    }

    public void ResetFillOnly() {

    }

    public void ResetTransitionOnly() {

    }

    public void ResetTechInfOnly() {

    }

    public void SaveDebugFiles(string FileNameWithoutExt) {

    }

     public Guid ID => Guid.Empty;

    public IST_OpContainer Container => opContainer;

    public IST_OperationSolver Solver => this;

    public IST_OpParametersUI ParametersUI => null;

    // ------------ IExtensionGlobal --------------------------
    public TResultStatus OnSCInitializing()
    {
        return default(TResultStatus);
    }

    public TResultStatus OnSCFinalizing()
    {
        return default(TResultStatus);
    }

    // ------------ IST_OperationSolver --------------------------

    public bool IsCorrectParameters(IST_Operation Operation) {
        return true;
    }

    public void InitializeRun(IST_CLDReceiver CLDFormer, IST_UpdateHandler UpdateHandler) {
        clf = CLDFormer;
        updateHandler = UpdateHandler;
    }

    public void Prepare() {

    }
    private delegate void TMakeOneLayer(double currentZ);
    private double GetRealTolerance(){
        if (opContainer.Units==TSTSystemUnits.suImperial) 
			return 0.0006;
		else
			return 0.006;
    }
    private static TCE3SPoint P3S(TST3DPoint p)
    {
        return new TCE3SPoint{
            X = (float)p.X,
            Y = (float)p.Y,
            Z = (float)p.Z
        }; 
    }
 
    private TST3DPoint AcceptBoxToLCS(TST3DMatrix lcs, TST3DBox b)
    {
        var pZ = lcs.vZ;
        pZ.X = pZ.X * b.Min.Z;
        pZ.Y = pZ.Y * b.Min.Z;
        pZ.Z = pZ.Z * b.Min.Z;
        var pT = lcs.vT;
        pT.X = pT.X + pZ.X;
        pT.Y = pT.Y + pZ.Y;
        pT.Z = pT.Z + pZ.Z;
        return pT;
    }
    private void FillPoints(ITrianglesReciever r)
    {  
        r.BeginTransfer();
        try
        {
            r.BeginModel();
            try
            {
                var mfJa = opContainer.MFJobAssignment;
                var faceList = mfJa.GetFaceList(opContainer.LCS);
                var newLCS = opContainer.LCS; 
                if (faceList.Count>0)
                { 
                    var mf = mfJa;
                    IST_GetBox ib = (IST_GetBox)mf;
                    var bb = ib.GetBoundingBox(opContainer.LCS); 
                    CEControlProcess.boundingBox = bb; 
                    newLCS.vT = AcceptBoxToLCS(newLCS, bb); //       newLCS.vT + newLCS.vZ * bb.Min.Z
                    faceList = mfJa.GetFaceList(newLCS);   
                }
                else if (faceList.Count<1)
                {
                    var mf = opContainer.MFPart;
                    IST_GetBox ib = (IST_GetBox)mf;
                    var bb = ib.GetBoundingBox(opContainer.LCS); 
                    CEControlProcess.boundingBox = bb; 
                    newLCS.vT = AcceptBoxToLCS(newLCS, bb);
                    faceList = opContainer.MFPart.GetFaceList(newLCS);    
                    //faceList = opContainer.MFPart.GetFaceList(opContainer.LCS);   
                }
                Marshal.FinalReleaseComObject(mfJa);
                                  
                int TotalTriangleCount = 0;  
                for (int i = 0; i < faceList.Count; i++)
                {
                    IST_Mesh mesh = faceList.GetMesh(i, GetRealTolerance(), 0, true);
                    TotalTriangleCount += mesh.GetTriangleCount();
                }    
                r.BeginMesh("Mesh", TotalTriangleCount);
                try
                {
                    for (int i = 0; i < faceList.Count; i++)
                    {
                        IST_Mesh mesh = faceList.GetMesh(i, GetRealTolerance(), 0, true);
                        int TriangleCount = mesh.GetTriangleCount();
                        for (var j = 0; j < TriangleCount; j++)
                        {
                            var xInd = mesh.GetTriangle(j).X;
                            var yInd = mesh.GetTriangle(j).Y;
                            var zInd = mesh.GetTriangle(j).Z;                            
                            r.AddTriangle(P3S(mesh.GetVertex(xInd)), P3S(mesh.GetVertex(yInd)), P3S(mesh.GetVertex(zInd)));
                        }
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
        var lib = CuraEngineConnectionHelper.LoadNativeLib(CuraPath);
        if (lib == null)
            return;
        var tr = lib.TrianglesReciever;
        FillPoints(tr);
        CEParamsReceiver.CEParameters.ParseAllParameters();
        CEControlProcess.clf = clf;
        CEControlProcess.OnGCodeCommandTranslation = GetGCodeCommandTranslation;
        CEControlProcess.IsOutputAdditionalCLDataParameters = IsOutputAdditionalCLDataParameters;
        CEControlProcess.IsOutputFilamentExtruding = IsOutputFilamentExtruding;
        CEControlProcess.tpm = tpm;
        CEControlProcess.FilamentExtrudingLength = FilamentExtrudingLength;
        CEControlProcess.updateHandler = updateHandler;
        try{
            lib.Slice(CEControlProcess, CEParamsReceiver, CuraPath);
        }
        catch(Exception e){
            Info.InstanceInfo.ExtensionManager.Logger.Error(e.Message);
        }
        
    }
    private void StartCalculateInCuraEngine() 
    {    
        if (!CheckCuraPath())
        {
            Console.WriteLine(WarningMessage);
            Info.InstanceInfo.ExtensionManager.Logger.Warning(WarningMessage); 
            ShowMessageBox(WarningMessage, TMessageDialogType.mdtWarning, (ushort)1, TUIButtonType.btOk, "");
        }
        else
        {
            FillParametersAndCalculate();
            CuraEngineConnectionHelper.FinalizeLib();  
        }
        
    }
    public void MakeWorkPath() {
        if (opContainer == null || clf == null)
             return;
        
        StartCalculateInCuraEngine();
    }

    public void MakeFill() {

    }

    public void MakeTransition() {

    }

    public void MakeTechInf() {

    }

    public void FinalizeRun() {

    }

    public void InitLngRes(int LngID) {

    }

    private double Step{ get; set; }
    private string Name{ get; set; }
    private int Count{ get; set; }
    private bool Visible{ get; set; }
    private IST_CustomProp GetStringProp(Parameter param, bool isGlobalParameter, bool isAlwaysVisible = false, bool IsStructural = false)
    {
        var label = GetLabelTranslation(param.id, param.label);
        var sp = helpers.CreateStringProp(label);
        if (param.icon != null && param.icon != "")
            sp.IconFile = CuraPath + "share\\cura\\resources\\themes\\cura-light\\icons\\default\\" + param.icon + ".svg";
        sp.PropID = param.id;
        sp.UnitsStr = param.unit;
        sp.PropIsExpandedGetter = new BooleanValueGetter(() => param.IsExpanded);
        sp.PropIsExpandedSetter = new BooleanValueSetter((v) => param.IsExpanded = v);

        sp.RestoreValueProc = new DefaultPropValue(delegate()
        {
            String userParam;
            if (CEParamsReceiver.CEParameters.UserParameters.TryGetValue(param.id, out userParam))
            {
                RemoveUserParameter(param.id);
                CEParamsReceiver.CEParameters.UpdateAllParameters();
            }
        });
        sp.IsStructural = new BooleanValueGetter(() => IsStructural);
        sp.Visible = new BooleanValueGetter(delegate ()
        {         
            bool isVisible = true;
            if (!isAlwaysVisible)
            {
                try
                {
                    isVisible = CEParamsReceiver.CEParameters.IsParameterEnabled(param.id, isGlobalParameter);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Get visible failed: {e.Message}");
                }
                if (isVisible && SearchFilter!="" && !param.IsVisibleInInspector)
                    isVisible = false;
            }

            return isVisible;
        });
        sp.ValueGetter = new StringValueGetter(() => CEParamsReceiver.CEParameters.GetValue(param.id, isGlobalParameter));
        sp.ValueSetter = new StringValueSetter(delegate (string v)
        {
            if (v!=null)
            {
                CEParamsReceiver.CEParameters.SetValue(param.id, v, isGlobalParameter);
                AddUserParameter(param.id, v, isGlobalParameter);
            }
            else
            {
                CEParamsReceiver.CEParameters.SetValue(param.id, "", isGlobalParameter);
                AddUserParameter(param.id, "", isGlobalParameter);
            }
            CEParamsReceiver.CEParameters.UpdateAllParameters();
                 
        });
        return sp;
    }
    private IST_CustomProp GetIntegerProp(Parameter param, bool isGlobalParameter, bool isAlwaysVisible = false, bool IsStructural = false)
    {
        var label = GetLabelTranslation(param.id, param.label);
        var ip = helpers.CreateIntegerProp(label);
        if (param.icon != null && param.icon != "")
            ip.IconFile = CuraPath + "share\\cura\\resources\\themes\\cura-light\\icons\\default\\" + param.icon + ".svg";
        ip.PropID = param.id;
        ip.UnitsStr = param.unit;
        ip.PropIsExpandedGetter = new BooleanValueGetter(() => param.IsExpanded);
        ip.PropIsExpandedSetter = new BooleanValueSetter((v) => param.IsExpanded = v);
        ip.IsStructural = new BooleanValueGetter(() => IsStructural);
        ip.RestoreValueProc = new DefaultPropValue(delegate()
        {
            String userParam;
            if (CEParamsReceiver.CEParameters.UserParameters.TryGetValue(param.id, out userParam))
            {
                RemoveUserParameter(param.id);
                CEParamsReceiver.CEParameters.UpdateAllParameters();
            }
        });
        ip.Visible = new BooleanValueGetter(delegate ()
        {
            bool isVisible = true;
            if (!isAlwaysVisible)
            {
                try
                {
                    isVisible = CEParamsReceiver.CEParameters.IsParameterEnabled(param.id, isGlobalParameter);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Get visible failed: {e.Message}");
                }
                if (isVisible && SearchFilter!="" && !param.IsVisibleInInspector)
                    isVisible = false;
            }
            
            return isVisible;
        });
        ip.ValueGetter = new IntegerValueGetter(delegate ()
        {
            var value = CEParamsReceiver.CEParameters.GetValue(param.id, true);
            int intValue;
            if (int.TryParse(value, out intValue))
            {
                return intValue; 
            }
            return 0;
        });
        ip.ValueSetter = new IntegerValueSetter(delegate (int v)
        {
            CEParamsReceiver.CEParameters.SetValue(param.id, v.ToString(), isGlobalParameter);
            AddUserParameter(param.id, v.ToString(), isGlobalParameter); 
            CEParamsReceiver.CEParameters.UpdateAllParameters();
        });
        return ip;
    }
    private IST_CustomProp GetFloatProp(Parameter param, bool isGlobalParameter, bool isAlwaysVisible = false, bool IsStructural = false)
    {
        var label = GetLabelTranslation(param.id, param.label);
        var fp = helpers.CreateDoubleProp(label);
        if (param.icon != null && param.icon != "")
            fp.IconFile = CuraPath + "share\\cura\\resources\\themes\\cura-light\\icons\\default\\" + param.icon + ".svg";
        fp.PropID = param.id;
        fp.UnitsStr = param.unit;
        fp.PropIsExpandedGetter = new BooleanValueGetter(() => param.IsExpanded);
        fp.PropIsExpandedSetter = new BooleanValueSetter((v) => param.IsExpanded = v);
        fp.IsStructural = new BooleanValueGetter(() => IsStructural);
        fp.RestoreValueProc = new DefaultPropValue(delegate()
        {
            String userParam;
            if (CEParamsReceiver.CEParameters.UserParameters.TryGetValue(param.id, out userParam))
            {
                RemoveUserParameter(param.id);
                CEParamsReceiver.CEParameters.UpdateAllParameters();
            }
        });
        // bool isVisible = CEParamsReceiver.CEParameters.IsParameterEnabled(param.id, isGlobalParameter);
        // fp.Visible = new BooleanValueGetter(() => isVisible);
        fp.Visible = new BooleanValueGetter(delegate ()
        {
            bool isVisible = true;
            if (!isAlwaysVisible)
            {
                try
                {
                    isVisible = CEParamsReceiver.CEParameters.IsParameterEnabled(param.id, isGlobalParameter);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Get visible failed: {e.Message}");
                }
                if (isVisible && SearchFilter!="" && !param.IsVisibleInInspector)
                    isVisible = false;
            }
            
            return isVisible;
        });
        fp.ValueGetter = new DoubleValueGetter(delegate ()
        {
            var value = CEParamsReceiver.CEParameters.GetValue(param.id, true);
            double doubleValue;
            if (DoubleParser.DoubleTryParse(value, out doubleValue))
            {
                return doubleValue; 
            }
            return 0;
        });
        fp.ValueSetter = new DoubleValueSetter(delegate (double v)
        {
            CEParamsReceiver.CEParameters.SetValue(param.id, v.ToString(), isGlobalParameter);
            AddUserParameter(param.id, v.ToString(), isGlobalParameter); 
            CEParamsReceiver.CEParameters.UpdateAllParameters();
            if (param.id=="layer_height")
                SetToolParameterization("layer_height");
            if (param.id=="line_width")
                SetToolParameterization("line_width");
        });
        return fp;
    }
    private IST_CustomProp GetBoolProp(Parameter param, bool isGlobalParameter, bool isAlwaysVisible = false, bool IsStructural = false)
    {
        var label = GetLabelTranslation(param.id, param.label);
        var bp = helpers.CreateBooleanProp(label);
        if (param.icon != null && param.icon != "")
            bp.IconFile = CuraPath + "share\\cura\\resources\\themes\\cura-light\\icons\\default\\" + param.icon + ".svg";
        bp.PropID = param.id;
        bp.PropIsExpandedGetter = new BooleanValueGetter(() => param.IsExpanded);
        bp.PropIsExpandedSetter = new BooleanValueSetter((v) => param.IsExpanded = v);
        bp.IsStructural = new BooleanValueGetter(() => IsStructural);
        bp.RestoreValueProc = new DefaultPropValue(delegate()
        {
            String userParam;
            if (CEParamsReceiver.CEParameters.UserParameters.TryGetValue(param.id, out userParam))
            {
                RemoveUserParameter(param.id);
                CEParamsReceiver.CEParameters.UpdateAllParameters();
            }
        });
        bp.Visible = new BooleanValueGetter(delegate ()
        {
            bool isVisible = true;
            if (!isAlwaysVisible)
            {
                try
                {
                    isVisible = CEParamsReceiver.CEParameters.IsParameterEnabled(param.id, isGlobalParameter);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Get visible failed: {e.Message}");
                }
                if (isVisible && SearchFilter!="" && !param.IsVisibleInInspector)
                    isVisible = false;
            }
            
            return isVisible;
        });

        bp.ValueGetter = new BooleanValueGetter(delegate ()
        {
            var value = CEParamsReceiver.CEParameters.GetValue(param.id, true);
            bool boolValue;
            if (bool.TryParse(value, out boolValue))
            {
                return boolValue; 
            }
            return true;
        });
        bp.ValueSetter = new BooleanValueSetter(delegate (bool v)
        {
            CEParamsReceiver.CEParameters.SetValue(param.id, v.ToString(), isGlobalParameter);
            AddUserParameter(param.id, v.ToString(), isGlobalParameter);
            CEParamsReceiver.CEParameters.UpdateAllParameters(); 
        });
        return bp;
    }
    private IST_CustomProp GetEnumProp(Parameter param, bool isGlobalParameter, bool isAlwaysVisible = false, bool IsStructural = false)
    {
        var label = GetLabelTranslation(param.id, param.label);
        var ep = helpers.CreateEnumWithIDProp(label);
        if (param.icon != null && param.icon != "")
            ep.IconFile = CuraPath + "share\\cura\\resources\\themes\\cura-light\\icons\\default\\" + param.icon + ".svg";
        ep.PropID = param.id;
        ep.IsStructural = new BooleanValueGetter(() => IsStructural);
        ep.PropIsExpandedGetter = new BooleanValueGetter(() => param.IsExpanded);
        ep.PropIsExpandedSetter = new BooleanValueSetter((v) => param.IsExpanded = v);
        ep.RestoreValueProc = new DefaultPropValue(delegate()
        {
            String userParam;
            if (CEParamsReceiver.CEParameters.UserParameters.TryGetValue(param.id, out userParam))
            {
                RemoveUserParameter(param.id);
                CEParamsReceiver.CEParameters.UpdateAllParameters();
            }
        });
        ep.Visible = new BooleanValueGetter(delegate ()
        {
            bool isVisible = true;
            if (!isAlwaysVisible)
            {
                try
                {
                    isVisible = CEParamsReceiver.CEParameters.IsParameterEnabled(param.id, isGlobalParameter);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Get visible failed: {e.Message}");
                }
                if (isVisible && SearchFilter!="" && !param.IsVisibleInInspector)
                    isVisible = false;
                // if (isVisible)
                // {
                //     if (SearchFilter!="")
                //     {
                //         if (param.label.ToLower().Contains(SearchFilter.ToLower()))
                //         {
                //             isVisible = true;
                //         }
                //         else
                //         {
                //             for (int i=0; i<param.children.Count; i++)
                //             {
                //                 if (param.children[i].label.ToLower().Contains(SearchFilter))
                //                 {
                //                     isVisible = true;
                //                 }
                //             }
                //         }
                //     }
                //     else
                //     {
                //         isVisible = true;
                //     }
                // }  
            }
            
            return isVisible;
        });
        foreach (EnumItem ei in param.options) 
        {
            var Caption = GetEnumsTranslation(param.id, param.label, ei.id, ei.name);
            ep.Add(ei.id, Caption, ""); 
        }
        ep.ValueGetter = new StringValueGetter(() => CEParamsReceiver.CEParameters.GetValue(param.id, isGlobalParameter));
        ep.ValueSetter = new StringValueSetter(delegate (string v)
        {
            CEParamsReceiver.CEParameters.SetValue(param.id, v, isGlobalParameter);
            AddUserParameter(param.id, v, isGlobalParameter);  
            CEParamsReceiver.CEParameters.UpdateAllParameters();           
        });
        return ep;
    }
    private IST_CustomProp GetComplexProp(Parameter param, bool isAlwaysVisible = false)
    {
        var label = GetLabelTranslation(param.id, param.label);
        var prop = helpers.CreateComplexProp(label);
        if (param.icon != null && param.icon != "")
            prop.IconFile = "$(SUPPLEMENT_FOLDER)\\operations\\TypeImages\\MeasuringItem.bmp";
        prop.PropID = param.id;
        prop.PropIsExpandedGetter = new BooleanValueGetter(() => param.IsExpanded);
        prop.PropIsExpandedSetter = new BooleanValueSetter((v) => param.IsExpanded = v);
        prop.Visible = new BooleanValueGetter(delegate ()
        {
            bool isVisible = true;
            if (SearchFilter!="" && !param.IsVisibleInInspector)
                isVisible = false;
            return isVisible;
        });
        return prop;
    }
    private void AddComplexProps(IST_SimplePropIterator SimpleIterator, string filename)
    {
        string line;
        Parameter param;
        try
        {
            StreamReader sr = new StreamReader(filename);
            line = sr.ReadLine();
            while (line != null)
            {
                line = line.Trim();
                var line2 = sr.ReadLine();
                if (line.StartsWith("[") && line.EndsWith("]") && !line.Contains("dual"))
                {
                    if (line2 != null && line2.Trim() != "" && !line2.Contains("="))
                    {
                        line = line.Replace("[", "");
                        line = line.Replace("]", "");
                        if (CEParamsReceiver.CEParameters.GlobalParams.TryGetValue(line, out param))
                        {
                            var prop = GetComplexProp(param);
                            if (prop != null)
                            {
                                param.indProp = SimpleIterator.AddNewProp(prop, -1);
                            }  
                        }
                    }
                }
                line = line2;
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }
    private void AddProps(IST_SimplePropIterator SimpleIterator, string filename)
    {
        string line;
        Parameter param;
        try
        {     
            StreamReader sr = new StreamReader(filename);
            line = sr.ReadLine();
            while (line != null)
            {
                IST_CustomProp prop = null;
                line = line.Trim();
                var line2 = sr.ReadLine();
                if (line != "" && !line.Contains("="))
                {
                    if (CEParamsReceiver.CEParameters.GlobalParams.TryGetValue(line, out param))
                    {
                        var NeedAddParam = true;
                        Parameter parent = param.parent;
                        while (parent!=null)
                        {
                            if (parent.id == "dual")
                            {
                                NeedAddParam = false;
                                break;
                            }
                            parent = parent.parent;
                        }
                        if (NeedAddParam)
                        {
                            switch (param.paramType)
                            {
                                case ParameterType.ptStr:
                                    prop = GetStringProp(param, true);
                                    break;
                                case ParameterType.ptArrayofInt:
                                    prop = GetStringProp(param, true);
                                    break;
                                case ParameterType.ptInt:
                                    prop = GetIntegerProp(param, true);
                                    break;
                                case ParameterType.ptFloat:
                                    prop = GetFloatProp(param, true);
                                    break;
                                case ParameterType.ptBool:
                                    prop = GetBoolProp(param, true);
                                    break;
                                case ParameterType.ptEnum:
                                    prop = GetEnumProp(param, true);
                                    break;
                            }
                            if (prop != null)
                            {
                                param.indProp = SimpleIterator.AddNewProp(prop, param.parent.indProp);
                            }                           
                        }
                    }
                }
                line = line2;
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }
    private IST_SimplePropIterator FillPropIteratorFromFile(IST_SimplePropIterator SimpleIterator, string filename)
    {
        AddComplexProps(SimpleIterator, filename);
        AddProps(SimpleIterator, filename);
        return SimpleIterator;
    }
    private void AddChildrenProps(IST_SimplePropIterator SimpleIterator, List<Parameter> children)
    {
        for (int i = 0; i < children.Count; i++)
        {
            var childParam = children[i];
            IST_CustomProp prop = null;
            switch (childParam.paramType)
            {
                case ParameterType.ptStr:
                    prop = GetStringProp(childParam, true);
                    break;
                case ParameterType.ptArrayofInt:
                    prop = GetStringProp(childParam, true);
                    break;
                case ParameterType.ptInt:
                    prop = GetIntegerProp(childParam, true);
                    break;
                case ParameterType.ptFloat:
                    prop = GetFloatProp(childParam, true);
                    break;
                case ParameterType.ptBool:
                    prop = GetBoolProp(childParam, true);
                    break;
                case ParameterType.ptEnum:
                    prop = GetEnumProp(childParam, true);
                    break;
            }
            if (prop != null)
            {
                childParam.indProp = SimpleIterator.AddNewProp(prop, childParam.parent.indProp);
                if (childParam.HasChildren())
                    AddChildrenProps(SimpleIterator, childParam.children);
            }
        }
    }
    private IST_SimplePropIterator FillPropIteratorByAllParams(IST_SimplePropIterator SimpleIterator)
    {
        string line;
        Parameter param;
        try
        {
            StreamReader sr = new StreamReader(CuraPath + "share\\cura\\resources\\setting_visibility\\basic.cfg");
            line = sr.ReadLine();
            while (line != null)
            {
                line = line.Trim();
                if (line.StartsWith("[") && line.EndsWith("]") && !line.Contains("general") && 
                    !line.Contains("machine_settings") && !line.Contains("dual"))
                {
                    line = line.Replace("[", "");
                    line = line.Replace("]", "");
                    if (CEParamsReceiver.CEParameters.GlobalParams.TryGetValue(line, out param))
                    {
                        var prop = GetComplexProp(param);
                        if (prop != null)
                        {
                            param.indProp = SimpleIterator.AddNewProp(prop, -1);
                            if (param.HasChildren())
                                AddChildrenProps(SimpleIterator, param.children);
                        }
                            
                    }
                }
                line = sr.ReadLine();
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
        return SimpleIterator;
    }
    private bool IsGeneralPropsExpanded = false;
    private bool IsStrengthPropsExpanded = true;
    private bool IsSupportPropsExpanded = true;
    private bool IsAdhesionPropsExpanded = true;
    private bool IsAutoToolParameterization = true;
    public ToolpathParsingMode tpm = ToolpathParsingMode.tpmSimplified;
    public bool IsOutputAdditionalCLDataParameters = false;
    public bool IsOutputFilamentExtruding = false;
    public double FilamentExtrudingLength = 100; 
    private IST_SimplePropIterator FillSimplifiedPropIterator(IST_SimplePropIterator SimpleIterator)
    {
        Parameter param;
        var parentInd = -1;

        //Strength
        IST_CustomDoublePropHelper infillProp = null;
        if (CEParamsReceiver.CEParameters.GlobalParams.TryGetValue("infill_sparse_density", out param))
            infillProp = (IST_CustomDoublePropHelper)GetFloatProp(param, true, false, true);

        IST_CustomEnumWithIDPropHelper infillPatternProp = null;
        if (CEParamsReceiver.CEParameters.GlobalParams.TryGetValue("infill_pattern", out param))
            infillPatternProp = (IST_CustomEnumWithIDPropHelper)GetEnumProp(param, true, false, true);

        IST_CustomDoublePropHelper wallProp = null;
        if (CEParamsReceiver.CEParameters.GlobalParams.TryGetValue("wall_thickness", out param))
            wallProp = (IST_CustomDoublePropHelper)GetFloatProp(param, true, false, true);

        IST_CustomDoublePropHelper topBottomProp = null; 
        if (CEParamsReceiver.CEParameters.GlobalParams.TryGetValue("top_bottom_thickness", out param))
            topBottomProp = (IST_CustomDoublePropHelper)GetFloatProp(param, true, false, true);

        var strengthCaption = GetLabelTranslation("Strength");
        var strengthProp = helpers.CreateComplexProp(strengthCaption);
        if (strengthProp != null)
        {
            strengthProp.IconFile = "$(SUPPLEMENT_FOLDER)\\operations\\TypeImages\\MeasuringItem.bmp";
            strengthProp.PropIsExpandedGetter = new BooleanValueGetter(() => IsStrengthPropsExpanded);
            strengthProp.PropIsExpandedSetter = new BooleanValueSetter((v) => IsStrengthPropsExpanded = v);
            strengthProp.Visible = new BooleanValueGetter(delegate ()
            {
                bool isVisible = false;
                if (infillProp!=null && infillProp.Visible.GetValue() || infillPatternProp!=null && infillPatternProp.Visible.GetValue()  ||
                    wallProp!=null && wallProp.Visible.GetValue() || topBottomProp!=null && topBottomProp.Visible.GetValue())
                    isVisible = true;
                return isVisible;
            });
            parentInd = SimpleIterator.AddNewProp(strengthProp, -1);
        }
        if (infillProp != null)
            SimpleIterator.AddNewProp(infillProp, parentInd);
        if (infillPatternProp != null)
            SimpleIterator.AddNewProp(infillPatternProp, parentInd);
        if (wallProp != null)
            SimpleIterator.AddNewProp(wallProp, parentInd);
        if (topBottomProp != null)
            SimpleIterator.AddNewProp(topBottomProp, parentInd);

        //support
        IST_CustomBooleanPropHelper supportEnabledProp = null;
        Parameter seParam = null;
        if (CEParamsReceiver.CEParameters.GlobalParams.TryGetValue("support_enable", out seParam))
            supportEnabledProp = (IST_CustomBooleanPropHelper)GetBoolProp(seParam, true, false, true);

        Parameter ssParam = null;
        IST_CustomEnumWithIDPropHelper supportStructureProp = null;
        if (CEParamsReceiver.CEParameters.GlobalParams.TryGetValue("support_structure", out ssParam))
            supportStructureProp = (IST_CustomEnumWithIDPropHelper)GetEnumProp(ssParam, true, false, true);

        Parameter stParam = null;
        IST_CustomEnumWithIDPropHelper supportTypeProp = null;
        if (CEParamsReceiver.CEParameters.GlobalParams.TryGetValue("support_type", out stParam))
            supportTypeProp = (IST_CustomEnumWithIDPropHelper)GetEnumProp(stParam, true, false, true);

        var SupportCaption = GetLabelTranslation("Support");
        var supportProp = helpers.CreateComplexProp(SupportCaption);
        if (supportProp != null)
        {
            supportProp.IconFile = "$(SUPPLEMENT_FOLDER)\\operations\\TypeImages\\MeasuringItem.bmp";
            supportProp.PropIsExpandedGetter = new BooleanValueGetter(() => IsSupportPropsExpanded);
            supportProp.PropIsExpandedSetter = new BooleanValueSetter((v) => IsSupportPropsExpanded = v);
            supportProp.Visible = new BooleanValueGetter(delegate ()
            {
                bool isVisible = false;
                
                if (SearchFilter=="" || (supportEnabledProp!=null && supportEnabledProp.Caption.ToLower().Contains(SearchFilter.ToLower())) ||
                    (supportStructureProp!=null && supportStructureProp.Caption.ToLower().Contains(SearchFilter.ToLower())) ||
                    (supportTypeProp!=null && supportTypeProp.Caption.ToLower().Contains(SearchFilter.ToLower())))
                    isVisible = true;
                return isVisible;
            });
            parentInd = SimpleIterator.AddNewProp(supportProp, -1);
        }
        if (supportEnabledProp != null)
            SimpleIterator.AddNewProp(supportEnabledProp, parentInd);
        if (supportStructureProp != null)
            SimpleIterator.AddNewProp(supportStructureProp, parentInd);
        if (supportTypeProp != null)
            SimpleIterator.AddNewProp(supportTypeProp, parentInd);
        //adhesion
        if (CEParamsReceiver.CEParameters.GlobalParams.TryGetValue("adhesion_type", out param))
        {
            var PropName = GetLabelTranslation("Adhesion");
            var bp = helpers.CreateBooleanProp(PropName);
            bp.PropID = "_Adhesion";
            bp.IsStructural = new BooleanValueGetter(() => true);
            bp.Visible = new BooleanValueGetter(delegate ()
            {
                bool isVisible = true;

                try
                {
                    isVisible = CEParamsReceiver.CEParameters.IsParameterEnabled(param.id, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Get visible failed: {e.Message}");
                }
                if (isVisible && SearchFilter!="" && !PropName.ToLower().Contains(SearchFilter.ToLower()))
                    isVisible = false;
                return isVisible;
            });
            bp.PropIsExpandedGetter = new BooleanValueGetter(() => IsAdhesionPropsExpanded);
            bp.PropIsExpandedSetter = new BooleanValueSetter((v) => IsAdhesionPropsExpanded = v);
            bp.ValueGetter = new BooleanValueGetter(delegate ()
            {
                var value = CEParamsReceiver.CEParameters.GetValue(param.id, true);
                if (value=="none")// || value=="skirt")
                    return false;
                else
                    return true;
            });
            bp.ValueSetter = new BooleanValueSetter(delegate (bool v)
            {
                if (v)
                {
                    CEParamsReceiver.CEParameters.SetValue(param.id, "brim", true);
                    AddUserParameter(param.id, "brim", true); 
                }
                else
                {
                    CEParamsReceiver.CEParameters.SetValue(param.id, "none", true);
                    AddUserParameter(param.id, "none", true); 
                }
            });

            if (bp != null)
                SimpleIterator.AddNewProp(bp, -1);
        }
        return SimpleIterator;
    }
    private IST_SimplePropIterator AddAutoToolParameterization(IST_SimplePropIterator SimpleIterator)
    {
        var atpCaption = GetLabelTranslation("Auto tool parameterization");
        var atpProp = helpers.CreateBooleanProp(atpCaption);
        if (atpProp!=null)
        {
            atpProp.PropID = "_auto_tool_parameterization";
            atpProp.Visible = new BooleanValueGetter(delegate ()
            {
                bool isVisible = true;
                if (SearchFilter!="" && !atpProp.Caption.ToLower().Contains(SearchFilter.ToLower()))
                    isVisible = false;
                return isVisible;
            });
            atpProp.ValueGetter = new BooleanValueGetter(() => IsAutoToolParameterization);
            // atpProp.ValueSetter = new BooleanValueSetter((v) => IsAutoToolParameterization = v);  
            atpProp.ValueSetter = new BooleanValueSetter(delegate (bool v)
            {
                IsAutoToolParameterization = v;
                if (v)
                {
                    SetToolParameterization("layer_height");
                    SetToolParameterization("line_width"); 
                }
                SaveAutoToolParameterizationToXML();
            });
            SimpleIterator.AddNewProp(atpProp, -1);
        }
        return SimpleIterator;
    }
    private IST_SimplePropIterator AddToolpathParsingMode(IST_SimplePropIterator SimpleIterator)
    {
        int parentInd = -1;

        var atpCaption = GetLabelTranslation("Output additional parameters");
        var atpProp = helpers.CreateBooleanProp(atpCaption);
        if (atpProp!=null)
        {
            atpProp.PropID = "_output_additional_parameters";
            atpProp.IsStructural = new BooleanValueGetter(() => true);
            atpProp.Visible = new BooleanValueGetter(delegate ()
            {
                bool isVisible = false;
                if (tpm==ToolpathParsingMode.tpmSimplified)
                    isVisible = true;
                if (SearchFilter!="" && !atpProp.Caption.ToLower().Contains(SearchFilter.ToLower()))
                    isVisible = false;
                return isVisible;
            });
            atpProp.ValueGetter = new BooleanValueGetter(() => IsOutputAdditionalCLDataParameters);
            atpProp.ValueSetter = new BooleanValueSetter(delegate (bool v)
            {
                IsOutputAdditionalCLDataParameters = v;
                SaveOutputAdditionalParametersToXML();
            });
        }
        var ofeCaption = GetLabelTranslation("Output filament extruding");
        var ofeProp = helpers.CreateBooleanProp(ofeCaption);
        if (ofeProp!=null)
        {
            ofeProp.PropID = "_output_filament_extruding";
            ofeProp.IsStructural = new BooleanValueGetter(() => true);
            ofeProp.Visible = new BooleanValueGetter(delegate ()
            {
                bool isVisible = false;
                if (tpm==ToolpathParsingMode.tpmGCodeBased)
                    isVisible = true;
                if (SearchFilter!="" && !ofeProp.Caption.ToLower().Contains(SearchFilter.ToLower()))
                    isVisible = false;
                return isVisible;
            });
            ofeProp.ValueGetter = new BooleanValueGetter(() => IsOutputFilamentExtruding);
            ofeProp.ValueSetter = new BooleanValueSetter(delegate (bool v)
            {
                IsOutputFilamentExtruding = v;
                SaveOutputFilamentExtrudingToXML();
            });
        }

        var felCaption = GetLabelTranslation("Filament extruding length per frame");
        var felProp = helpers.CreateDoubleProp(felCaption);
        if (felProp!=null)
        {
            felProp.PropID = "_filament_extruding_length";
            felProp.IsStructural = new BooleanValueGetter(() => true);
            felProp.UnitsStr = "mm";
            felProp.Visible = new BooleanValueGetter(delegate ()
            {
                bool isVisible = false;
                if (tpm==ToolpathParsingMode.tpmGCodeBased && IsOutputFilamentExtruding)
                    isVisible = true;
                if (SearchFilter!="" && !felProp.Caption.ToLower().Contains(SearchFilter.ToLower()))
                    isVisible = false;
                return isVisible;
            });
            felProp.ValueGetter = new DoubleValueGetter(() => FilamentExtrudingLength);
            felProp.ValueSetter = new DoubleValueSetter(delegate (double v)
            {
                FilamentExtrudingLength = v;
                SaveFilamentExtrudingLengthToXML();
            });
        }

        var cldataModeCaption = GetLabelTranslation("Toolpath parsing mode");
        var cldataModeProp = helpers.CreateEnumWithIDProp(cldataModeCaption);
        if (cldataModeProp!=null)
        {
            cldataModeProp.PropID = "_toolpath_parsing_mode";
            cldataModeProp.IsStructural = new BooleanValueGetter(() => true);
            cldataModeProp.Visible = new BooleanValueGetter(delegate ()
            {
                bool isVisible = true;
                if (SearchFilter!="" && !cldataModeProp.Caption.ToLower().Contains(SearchFilter.ToLower()) &&
                     !atpProp.Visible.GetValue() && !ofeProp.Visible.GetValue() && !felProp.Visible.GetValue())
                    isVisible = false;
                return isVisible;
            });
            var smplCaption = GetEnumsTranslation("_toolpath_parsing_mode", "Toolpath parsing mode", "Simplified", "Simplified");
            cldataModeProp.Add("Simplified", smplCaption, ""); 
            var gcodeCaption = GetEnumsTranslation("_toolpath_parsing_mode", "Toolpath parsing mode", "GCodeBased", "GCode based");
            cldataModeProp.Add("GCodeBased", gcodeCaption, ""); 
            cldataModeProp.ValueGetter = new StringValueGetter(delegate ()
            {
                if (tpm==ToolpathParsingMode.tpmSimplified)
                    return "Simplified";
                else
                    return "GCodeBased";
            });   
            cldataModeProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                if (v=="Simplified")
                    tpm = ToolpathParsingMode.tpmSimplified;
                else
                    tpm = ToolpathParsingMode.tpmGCodeBased;
                SaveToolpathParsingModeToXML();
            });
            parentInd = SimpleIterator.AddNewProp(cldataModeProp, -1);
        }

        if (atpProp!=null) 
            SimpleIterator.AddNewProp(atpProp, parentInd);
        if (ofeProp!=null)    
            SimpleIterator.AddNewProp(ofeProp, parentInd);
        if (felProp!=null)
            SimpleIterator.AddNewProp(felProp, parentInd);
        return SimpleIterator;
    }
    private IST_SimplePropIterator AddGeneralParameters(IST_SimplePropIterator SimpleIterator)
    {
        var GenParamsCaption = GetLabelTranslation("General parameters");
        var genProp = helpers.CreateComplexProp(GenParamsCaption);
        if (genProp!=null)
        {
            genProp.PropID = "_general_parameters";
            genProp.Visible = new BooleanValueGetter(delegate ()
            {
                bool isVisible = true;
                if (SearchFilter!="" && !genProp.Caption.ToLower().Contains(SearchFilter.ToLower()))
                    isVisible = false;
                return isVisible;
            });
            genProp.IconFile = "";
            var textCaption = GetLabelTranslation("Click_to_open_general_parameters", "Click \"...\" to change");
            genProp.TextGetter = new StringValueGetter(() => textCaption);
            genProp.ButtonQuantity = 1;
            genProp.ButtonDisplayName[0] = "...";
            genProp.ButtonHelpText[0] = "";
            genProp.ButtonIconPath[0] = "";
            genProp.ButtonMode[0] = TCustomPropButtonState.cpbsClickable;
            genProp.ClickAction[0]  = new ButtonClickAction(delegate ()
            {
                var extension = Info.InstanceInfo.ExtensionManager.GetSingletonExtension("Extension.UIDialogs.Core", out TResultStatus ret);
                var uiDialog = (ICAMAPI_UIDialogsHelper)extension;

                var genParamsWindow = uiDialog.CreateWindow(GenParamsCaption);
                var Iterator = helpers.CreateSimplePropIterator();
                Iterator = FillGeneralparameters(Iterator);
                Iterator.MoveToRoot();
                genParamsWindow.PropIteratorGetter = new PropIteratorGetter(delegate()
                {
                    var Iterator = helpers.CreateSimplePropIterator();
                    Iterator = FillGeneralparameters(Iterator);
                    Iterator.MoveToRoot();
                    return (IST_CustomPropIterator)Iterator;
                });
                TUIButtonTypeFlags buttons = TUIButtonTypeFlags.btfOk;
                genParamsWindow.Buttons = (ushort)buttons;
                genParamsWindow.ShowModal();

                Marshal.FinalReleaseComObject(genParamsWindow);
                Marshal.FinalReleaseComObject(uiDialog);
                Info.InstanceInfo.ExtensionManager.Logger.Info("General parameters window is opened."); 
            });
            SimpleIterator.AddNewProp(genProp, -1);
        }
        return SimpleIterator;
    }
    private IST_SimplePropIterator AddCustomParamtersField(IST_SimplePropIterator SimpleIterator)
    {

        var parentInd = -1;
        //setting visibility
        var svCaption = GetLabelTranslation("Setting visibility");
        var svProp = helpers.CreateEnumWithIDProp(svCaption);
        if (svProp!=null)
        {
            svProp.PropID = "_setting_visibility";
            svProp.IsStructural = new BooleanValueGetter(() => true);
            svProp.Visible = new BooleanValueGetter(delegate ()
            {
                bool isVisible = CEParamsReceiver.CEParameters.IsShowCustomParameters;
                if (SearchFilter!="" && !svProp.Caption.ToLower().Contains(SearchFilter.ToLower()))
                    isVisible = false;
                return isVisible;
            });
            for (int i=0; i<CEParamsReceiver.CEParameters.SettingVisibilities.Count; i++)
            {
                var sv = CEParamsReceiver.CEParameters.SettingVisibilities[i];
                var svName = GetEnumsTranslation("_setting_visibility", "Setting visibility", sv, sv);
                svProp.Add(sv, svName, ""); 
            }
            svProp.ValueGetter = new StringValueGetter(() => CEParamsReceiver.CEParameters.SelectedSettingVisibilities);
            svProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                CEParamsReceiver.CEParameters.SelectedSettingVisibilities = v;
                SaveSettingVisibilityToXML();
            });
        }
        // show custom parameters
        var scpCaption = GetLabelTranslation("Show custom parameters");
        var scpProp = helpers.CreateBooleanProp(scpCaption);
        if (scpProp!=null)
        {
            scpProp.PropID = "_show_custom_parameters";
            scpProp.IsStructural = new BooleanValueGetter(() => true);
            scpProp.Visible = new BooleanValueGetter(delegate ()
            {
                bool isVisible = true;
                if (SearchFilter!="" && !scpProp.Caption.ToLower().Contains(SearchFilter.ToLower()) &&
                    !svProp.Visible.GetValue())
                    isVisible = false;
                return isVisible;
            });
            scpProp.ValueGetter = new BooleanValueGetter(() => CEParamsReceiver.CEParameters.IsShowCustomParameters);
            scpProp.ValueSetter = new BooleanValueSetter(delegate (bool v)
            {
                CEParamsReceiver.CEParameters.IsShowCustomParameters = v;
                SaveShowCustomParametersToXML();
            }); 
            parentInd = SimpleIterator.AddNewProp(scpProp, parentInd);
        }
        if (svProp!=null)
            SimpleIterator.AddNewProp(svProp, parentInd);
        return SimpleIterator;
    }
    private IST_SimplePropIterator FillGeneralparameters(IST_SimplePropIterator SimpleIterator)
    {

        var parentInd = -1;
        // machines brands
        var manufCaption = GetLabelTranslation("Manufacturer");
        var manufProp = helpers.CreateEnumWithIDProp(manufCaption);
        if (manufProp!=null)
        {
            manufProp.PropID = "_manufacturer";
            manufProp.IsStructural = new BooleanValueGetter(() => true);
            manufProp.Visible = new BooleanValueGetter(() => true);
            for (int i=0; i<CEParamsReceiver.CEParameters.MachinesBrands.Count; i++)
            {
                var manuf = CEParamsReceiver.CEParameters.MachinesBrands[i];
                manufProp.Add(manuf.name, manuf.name, ""); 
            }
            manufProp.ValueGetter = new StringValueGetter(() => CEParamsReceiver.CEParameters.SelectedMachineBrand.name);
            manufProp.ValueSetter = new StringValueSetter(delegate (string v) 
            {
                CEParamsReceiver.CEParameters.SetSelectedMachineBrand(v, true);
                SaveManufacturerToXML();
            });
            SimpleIterator.AddNewProp(manufProp, parentInd);
        }

        // machines/printers
        var printCaption = GetLabelTranslation("Printers");
        var machProp = helpers.CreateEnumWithIDProp(printCaption);
        if (machProp!=null)
        {
            machProp.PropID = "_printers";
            machProp.IsStructural = new BooleanValueGetter(() => true);
            machProp.Visible = new BooleanValueGetter(() => true);
            for (int i=0; i<CEParamsReceiver.CEParameters.SelectedMachineBrand.Machines.Count; i++)
            {         
                var mach = CEParamsReceiver.CEParameters.SelectedMachineBrand.Machines[i];
                if (mach.isVisible)
                    machProp.Add(mach.id, mach.name, ""); 
            }
            machProp.ValueGetter = new StringValueGetter(() => CEParamsReceiver.CEParameters.SelectedMachine.id);
            machProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                CEParamsReceiver.CEParameters.SetSelectedMachine(v);
                SaveMachineToXML();
            });
            SimpleIterator.AddNewProp(machProp, parentInd);
        }

        // extruders
        var extrCaption = GetLabelTranslation("Extruders");
        var extrProp = helpers.CreateEnumWithIDProp(extrCaption);
        if (extrProp!=null)
        {
            extrProp.PropID = "_extruders";
            extrProp.IsStructural = new BooleanValueGetter(() => true);
            extrProp.Visible = new BooleanValueGetter(delegate()
            {
                if (CEParamsReceiver.CEParameters.SelectedMachine.Extruders.Count>0)
                    return true;
                else
                    return false;
            });
            for (int i=0; i<CEParamsReceiver.CEParameters.SelectedMachine.Extruders.Count; i++)
            {
                var extr = CEParamsReceiver.CEParameters.SelectedMachine.Extruders[i];
                var caption = "";
                if (extr.name.Contains("Extruder"))
                {
                    caption = extr.name.Substring(0, 8);
                    caption = GetEnumsTranslation("_extruders", "Extruders", caption, caption);
                    var number = "";
                    if (8<extr.name.Length)
                        number = extr.name.Substring(8);
                    caption = caption+number;
                }
                else
                    caption = extr.name;   
                     
                extrProp.Add(extr.id, caption, ""); 
            }
            extrProp.ValueGetter = new StringValueGetter(() => CEParamsReceiver.CEParameters.SelectedExtruder.id);
            extrProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                CEParamsReceiver.CEParameters.SetSelectedExtruder(v);
                SaveExtruderToXML();
            });
            SimpleIterator.AddNewProp(extrProp, parentInd);
        }

        // materials brands
        var mbCaption = GetLabelTranslation("Material brand");
        var materialBrandProp = helpers.CreateEnumWithIDProp(mbCaption);
        if (materialBrandProp!=null)
        {
            materialBrandProp.PropID = "_material_brand";
            materialBrandProp.IsStructural = new BooleanValueGetter(() => true);
            materialBrandProp.Visible = new BooleanValueGetter(() => true);
            MaterialBrand mat = null;
            for (int i=0; i<CEParamsReceiver.CEParameters.MaterialsBrands.Count; i++)
            {
                mat = CEParamsReceiver.CEParameters.MaterialsBrands[i];
                var machine = CEParamsReceiver.CEParameters.SelectedMachine;
                if (machine!=null)
                {
                    if (machine.id.Contains("ultimaker"))
                    {
                        
                        if (mat.name.ToLower().Contains("generic") || mat.name.ToLower().Contains("ultimaker"))
                        {
                            if (mat.name.ToLower().Contains("generic"))
                            {
                                var Caption = GetEnumsTranslation("_material_brand", "Material brand", mat.name, mat.name);
                                materialBrandProp.Add(mat.name, Caption, ""); 
                            }
                            else
                                materialBrandProp.Add(mat.name, mat.name, ""); 
                        }       
                    }
                    else
                    {
                        if (!mat.name.ToLower().Contains("ultimaker"))
                            materialBrandProp.Add(mat.name, mat.name, ""); 
                    }
                }
                else
                {
                    materialBrandProp.Add(mat.name, mat.name, ""); 
                }        
            }
            materialBrandProp.ValueGetter = new StringValueGetter(() => CEParamsReceiver.CEParameters.SelectedMaterialBrand.name);
            materialBrandProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                CEParamsReceiver.CEParameters.SetSelectedMaterialBrand(v, true);
                SaveMaterialBrandToXML();
            });
            SimpleIterator.AddNewProp(materialBrandProp, parentInd);
        }

        // materials
        var matCaption = GetLabelTranslation("Materials");
        var matProp = helpers.CreateEnumWithIDProp(matCaption);
        if (matProp!=null)
        {
            matProp.PropID = "_materials";
            matProp.IsStructural = new BooleanValueGetter(() => true);
            matProp.Visible = new BooleanValueGetter(() => true);
            for (int i=0; i<CEParamsReceiver.CEParameters.SelectedMaterialBrand.Materials.Count; i++)
            {
                var mat = CEParamsReceiver.CEParameters.SelectedMaterialBrand.Materials[i];
                var isExcluded = false;
                for (var j=0; j<CEParamsReceiver.CEParameters.ExcludedMaterials.Count; j++)
                {
                    if (mat.id==CEParamsReceiver.CEParameters.ExcludedMaterials[j])
                    {
                        isExcluded = true;
                        break;
                    }
                }
                if (!isExcluded)
                {
                    if (mat.Color!="")
                    matProp.Add(mat.id, mat.GetCaption() + " (" + mat.Color + ", \u00F8"+ mat.Diameter.ToString() +")", ""); 
                    else
                        matProp.Add(mat.id, mat.GetCaption() + " (\u00F8"+ mat.Diameter.ToString() +")", ""); 
                }
                
            }
            matProp.ValueGetter = new StringValueGetter(() => CEParamsReceiver.CEParameters.SelectedMaterial.id);
            matProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                CEParamsReceiver.CEParameters.SetSelectedMaterial(v);
                SaveMaterialToXML();
            });
            SimpleIterator.AddNewProp(matProp, parentInd);
        }   

        // variants
        var varCaption = GetLabelTranslation("Variants");
        var varProp = helpers.CreateEnumWithIDProp(varCaption);
        if (varProp!=null)
        {
            varProp.PropID = "_variants";
            varProp.IsStructural = new BooleanValueGetter(() => true);
            varProp.Visible = new BooleanValueGetter(delegate()
            {
                if (CEParamsReceiver.CEParameters.Variants.Count>0)
                    return true;
                else
                    return false;
            });
            for (int i=0; i<CEParamsReceiver.CEParameters.Variants.Count; i++)
            {
                var variant = CEParamsReceiver.CEParameters.Variants[i];
                varProp.Add(variant.Name, variant.Name, ""); 
            }
            varProp.ValueGetter = new StringValueGetter(delegate ()
            {
                if (CEParamsReceiver.CEParameters.SelectedVariant != null)
                    return CEParamsReceiver.CEParameters.SelectedVariant.Name;
                else    
                    return "";
            });
            varProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                CEParamsReceiver.CEParameters.SetSelectedVariant(v);
                SaveVariantToXML();
            });
            SimpleIterator.AddNewProp(varProp, parentInd);
        }

        // profiles
        var profCaption = GetLabelTranslation("Profiles");
        var profProp = helpers.CreateEnumWithIDProp(profCaption);
        if (profProp!=null)
        {
            profProp.PropID = "_profiles";
            profProp.IsStructural = new BooleanValueGetter(() => true);
            profProp.Visible = new BooleanValueGetter(delegate()
            {
                if (CEParamsReceiver.CEParameters.IntentCategories.Count>0)
                    return true;
                else
                    return false;
            });
            for (int i=0; i<CEParamsReceiver.CEParameters.IntentCategories.Count; i++)
            {
                var pr = CEParamsReceiver.CEParameters.IntentCategories[i];
                var name = pr.IntentName;
                var Caption = GetEnumsTranslation("_profiles", "Profiles", name, name);
                profProp.Add(pr.IntentCategoryName, Caption, ""); 
            }
            profProp.ValueGetter = new StringValueGetter(delegate ()
            {
                if (CEParamsReceiver.CEParameters.SelectedIntentCategory != null)
                    return CEParamsReceiver.CEParameters.SelectedIntentCategory.IntentCategoryName;
                else    
                    return "";
            });
            profProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                CEParamsReceiver.CEParameters.SetSelectedIntentCategory(v);
                SaveProfileToXML();
            });
            SimpleIterator.AddNewProp(profProp, parentInd);
        }
        
        // resolution
        var resCaption = GetLabelTranslation("Resolutions");
        var resProp = helpers.CreateEnumWithIDProp(resCaption);
        if (resProp!=null)
        {
            resProp.PropID = "_resolutions";
            resProp.IsStructural = new BooleanValueGetter(() => true);
            resProp.Visible = new BooleanValueGetter(delegate()
            {
                if (CEParamsReceiver.CEParameters.SelectedIntentCategory!=null && CEParamsReceiver.CEParameters.SelectedIntentCategory.QualityInstances.Count>0)
                    return true;
                else
                    return false;
            });
            var SelectedIntentCategory = CEParamsReceiver.CEParameters.SelectedIntentCategory;
            if (SelectedIntentCategory!=null)
            {
                for (int i=0; i<SelectedIntentCategory.QualityInstances.Count; i++)
                {
                    var res = SelectedIntentCategory.QualityInstances[i];
                    resProp.Add(res.FileName, res.Caption, ""); 
                }
            }
            resProp.ValueGetter = new StringValueGetter(delegate ()
            {
                if (CEParamsReceiver.CEParameters.SelectedQuality != null)
                    return CEParamsReceiver.CEParameters.SelectedQuality.FileName;
                else    
                    return "";
            });
            resProp.ValueSetter = new StringValueSetter(delegate (string v)
            {
                CEParamsReceiver.CEParameters.SetSelectedQuality(v);
                SaveQualityToXML();
            }); 
            SimpleIterator.AddNewProp(resProp, parentInd);
        }

        return SimpleIterator;
    }
    private void AcceptFilterToParams()
    {
        var ParamPropIterator = (IST_CustomPropIterator)ParamSimplePropIterator;
        var prop = (IST_AbstractPropHelper)ParamPropIterator.CurProp;
        if (prop!=null)
        {
            Parameter param = null;
            if (prop.PropID!=null && prop.PropID!="")
            {
               if (CEParamsReceiver.CEParameters.GlobalParams.TryGetValue(prop.PropID, out param))
                {
                    if (SearchFilter!="")
                    {
                        var label = GetLabelTranslation(param.id, param.label);
                        if (param.paramType!=ParameterType.ptCategory && label.ToLower().Contains(SearchFilter.ToLower()))
                        {
                            param.IsVisibleInInspector = true;
                            var parent = param.parent;
                            while (parent!=null)
                            {
                                parent.IsVisibleInInspector = true;
                                parent = parent.parent;
                            }
                        }
                        else
                        {
                            param.IsVisibleInInspector = false;
                        }
                    }
                    else
                    {
                        param.IsVisibleInInspector = true;
                    }
                } 
            }
            
            if (ParamPropIterator.MoveToChild())
                AcceptFilterToParams();    
            if (ParamPropIterator.MoveToSibling())
                AcceptFilterToParams();    
            ParamPropIterator.MoveToParent();
        }
    }
    private void AcceptFilter()
    {
        ParamSimplePropIterator.MoveToRoot();
        AcceptFilterToParams();    
    }
    
    bool IST_Operation.GetPropIterator(string PageID, out IST_CustomPropIterator PropIterator)
    {
        if (!CheckCuraPath())
        {
            Console.WriteLine(WarningMessage);
            Info.InstanceInfo.ExtensionManager.Logger.Warning(WarningMessage); 
            ShowMessageBox(WarningMessage, TMessageDialogType.mdtWarning, (ushort)1, TUIButtonType.btOk, "");
            PropIterator = null;
            return false;
        }
        CEParamsReceiver.CEParameters.UpdateAllParameters();

        var SimpleIterator = helpers.CreateSimplePropIterator();
        SimpleIterator = AddGeneralParameters(SimpleIterator);
        SimpleIterator = AddAutoToolParameterization(SimpleIterator); 
        SimpleIterator = AddToolpathParsingMode(SimpleIterator);       
        SimpleIterator = AddCustomParamtersField(SimpleIterator);
        
        var ssv = CEParamsReceiver.CEParameters.SelectedSettingVisibilities;
        if (CEParamsReceiver.CEParameters.IsShowCustomParameters && ssv!=null && ssv!="")
        {
            if (ssv=="all")
            {
                SimpleIterator = FillPropIteratorByAllParams(SimpleIterator);
            }
            else
            {
                SimpleIterator = FillPropIteratorFromFile(SimpleIterator, CuraPath + "share\\cura\\resources\\setting_visibility\\" + ssv + ".cfg");
            }
        }
        else
        {
            SimpleIterator = FillSimplifiedPropIterator(SimpleIterator);
        }
        SimpleIterator.MoveToRoot();
        if (ParamSimplePropIterator!=null)
            Marshal.FinalReleaseComObject(ParamSimplePropIterator);
        ParamSimplePropIterator = SimpleIterator;
        PropIterator = (IST_CustomPropIterator)SimpleIterator;
        return true;
    }

    void IST_Operation.DoChangeParameter(string ParameterName, string Value)
    {
        if (ParameterName != null)
        {
            if (ParameterName == "_searching")
            {
                if (Value == null)
                {
                    SearchFilter = "";
                } 
                else if (SearchFilter.ToLower() != Value.ToLower())
                {
                    SearchFilter = Value;
                }
                AcceptFilter();
            }
        }  
    }
}

public struct TModelItemRec
{
    public TModelItemRec(Guid _ItemType, string _ItemID, string _ItemTypeName, string _ItemCaption)
    {
        ItemType= _ItemType;
        ItemID = _ItemID;
        ItemTypeName = _ItemTypeName;
        ItemCaption = _ItemCaption;
    }
    public Guid ItemType;
	public string ItemID;
	public string ItemTypeName;
	public string ItemCaption;
}
public class TModelFormerItems : IST_ModelFormerSupportedItems
{
    List<TModelItemRec> fRecs;
    public TModelFormerItems()
    {
        fRecs = new List<TModelItemRec>();
    }
    public void Clear()
    {
        fRecs.Clear();
    }

    public int AddItem(string ItemID, Guid ItemType, string ItemTypeName, string ItemCaption, string ItemHint, string ItemIconFile, bool AllowDoubleItems, IST_XMLPropPointer ItemXMLProp)
    {
        var rec = new TModelItemRec(ItemType, ItemID, ItemTypeName, ItemCaption);
        fRecs.Add(rec);
        return fRecs.Count-1;
    }

    public int IndexOfItem(string AnItemID)
    {
        for (var i=0; i<fRecs.Count; i++)
        {
            if (fRecs[i].ItemID==AnItemID)
                return i;
        }
        return -1;
    }

    public int Count => fRecs.Count;

    public Guid get_ItemType(int i)
    {
        return fRecs[i].ItemType;
    }

    public string get_ItemTypeName(int i)
    {
        return fRecs[i].ItemTypeName;
    }

    public string get_ItemID(int i)
    {
        return fRecs[i].ItemID;
    }

    public string get_ItemCaption(int i)
    {
        return fRecs[i].ItemCaption;
    }

    public string get_ItemHint(int i)
    {
        return "";
    }

    public string get_ItemIconFile(int i)
    {
        return "";
    }

    public bool get_AllowDoubleItems(int i)
    {
        return false;
    }

    public bool get_ItemVisible(int i)
    {
        return true;
    }

    public IST_XMLPropPointer get_ItemXMLProp(int i)
    {
        return null;
    }
}
