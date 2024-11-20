using CAMAPI.DotnetHelper;
using CAMAPI.EventHandler;
using CAMAPI.MCDFormerTypes;
using CAMAPI.Extensions;
using CAMAPI.ResultStatus;
using System.Reflection;
using CAMAPI.ModelFormerTypes;
using CAMAPI.SurfaceTypes;
using CAMAPI.TechOperation;
using CuraConnectionInterface;
using CuraEngineNetWrapper;
using STCustomPropTypes;
using STTypes;
using STXMLPropTypes;
using Geometry.VecMatrLib;
using CAMAPI.UIDialogs;
using System.Runtime.InteropServices;
namespace CuraEngineOperation;

public class CuraEngineOperationSolver :
    IExtension,
    ICamApiTechOperationSolver
{
    /// <summary>
    /// Additional information about extension, provided in json file. It initializes in main CAM application
    /// </summary>
    public IExtensionInfo? Info { get; set; }

    /// <summary>
    /// Object to register model formers
    /// </summary>
    private ICamApiEventHandler? _operationEventHandlerInitModelFormers;

    /// <summary>
    /// Object to load and save XML properties
    /// </summary>
    private ICamApiEventHandler? _operationEventHandlerLoadSaveXml;
    
    /// <summary>
    /// Object to get localized labels
    /// </summary>
    private Localize? _localization;

    /// <summary>
    /// Builder of prop iterators
    /// </summary>
    private OperationProps? _operationProps;
    
    /// <summary>
    /// Object to get path to cura library
    /// </summary>
    private CuraLibraryPath? _curaLibraryPath;
    
    private ParamsReceiver? _curaParamsReceiver;
    
    private CuraEngineControlProcess? _curaControlProcess;
    
    /// <summary>
    /// Wrapper over COM object - operation
    /// </summary>
    private ComWrapper<ICamApiTechOperation>? _operationComWrapper;
    
    /// <summary>
    /// COM object - operation
    /// </summary>
    private ICamApiTechOperation? Operation => _operationComWrapper?.Instance;
    
    private const string HandlerIdentInitModelFormers = "InitModelFormers";
    private const string HandlerIdentLoadSaveXml = "LoadSaveXml";
    private bool IsCorrectInitSolver = false;
    private string _warningMessage = "Cura not found installed. Set path to CuraEngine.exe manually.\nParameters tab -> Set Cura path";
    
    /// <summary>
    /// Nothing to do
    /// </summary>
    public void InitSolver(ICamApiTechOperationSolverInitializeContext context, out TResultStatus resultStatus)
    {
        resultStatus = default;
        try
        {
            if (Info == null)
                throw new Exception("Info is null");
            
            // save COM objects
            _operationComWrapper = new ComWrapper<ICamApiTechOperation>(context.TechOperation);
            
            // get wrapper over COM objects to operate later in this method
            var operation = _operationComWrapper.Instance;
            if (operation == null)
                throw new Exception("Operation is null");
            
            using var xmlPropCom = new ComWrapper<IST_XMLPropPointer>(operation.XMLProp);
            var xmlProp = xmlPropCom.Instance
                ?? throw new Exception("XMLProp is null");
            
            // cura engine parameters
            _curaParamsReceiver = new ParamsReceiver();
            
            // object to manage cura calculating tool path
            _curaControlProcess = new CuraEngineControlProcess(context.UpdateHandler, Info);

            // path to cura library
            _localization = new Localize(Info);
            _warningMessage = _localization.GetLabelTranslation("Path_warning_message", _warningMessage);

            // user localization of parameters
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var userLangPath = Path.Combine(Path.GetDirectoryName(assemblyLocation) ?? "", "UserLocalization");
            _localization.ReadUserTranslations(userLangPath);  

            _curaLibraryPath = new CuraLibraryPath(operation.XMLProp);
            if (!_curaLibraryPath.CheckLibraryExists(xmlProp))
            {
                _curaControlProcess.Logger.Warning(_warningMessage);
                ShowMessageBox(_warningMessage, TMessageDialogType.mdtWarning, (ushort)1, TUIButtonType.btOk, "");
            }
            else
            {
                InitConfigurations(ref resultStatus);
            }
        } catch (Exception e)
        {
            resultStatus.Code = TResultStatusCode.rsError;
            resultStatus.Description = e.Message;
        }
    }
    private void InitConfigurations(ref TResultStatus resultStatus)
    {
        if (!IsCorrectInitSolver)
        {
            try
            {
                var operation = _operationComWrapper.Instance;
                if (operation == null)
                    throw new Exception("Operation is null");

                _localization.ReadMainTranslations(_curaLibraryPath.CuraPath + @"share\cura\resources\i18n");
                _curaParamsReceiver.CEParameters.CuraPath = _curaLibraryPath.CuraPath;
                _curaParamsReceiver.CEParameters.ReadAllParametersAndConfigs();
                            
                // builder of prop iterators
                _operationProps = new OperationProps(Info, operation, _curaParamsReceiver.CEParameters, _localization, _curaLibraryPath);
                
                // event handler, so we can execute some non-mandatory methods of ICamApiTechOperationSolver
                _operationEventHandlerInitModelFormers ??= new OperationInitModelFormers();
                operation.RegisterHandler(HandlerIdentInitModelFormers, _operationEventHandlerInitModelFormers, new ListString(), out resultStatus);
                _operationEventHandlerLoadSaveXml ??= new OperationLoadSaveXmlProp(_curaParamsReceiver, _operationProps, _curaLibraryPath);
                operation.RegisterHandler(HandlerIdentLoadSaveXml, _operationEventHandlerLoadSaveXml, new ListString(), out resultStatus);

                IsCorrectInitSolver = true;
            } catch (Exception e)
            {
                resultStatus.Code = TResultStatusCode.rsError;
                resultStatus.Description = e.Message;
            } 
        }
    }
    private int ShowMessageBox(string Msg, TMessageDialogType DlgType, ushort Buttons, TUIButtonType DefaultButton, string ATitle)
    {
        using var box = SystemExtensionFactory.GetSingletonExtension<ICAMAPI_UIDialogsHelper>("Extension.UIDialogs.Core", Info);


        // var extension = Info.InstanceInfo.ExtensionManager.GetSingletonExtension("Extension.UIDialogs.Core", out TResultStatus ret);
        // var uiDialog = (ICAMAPI_UIDialogsHelper)extension;
        ushort buttons = (ushort)TUIButtonTypeFlags.btfOk;
        return box.Instance.MessageBox(Msg, DlgType, buttons, DefaultButton, ATitle);
    }
    /// <summary>
    /// Nothing to do
    /// </summary>
    public void FinalizeSolver()
    {
        Operation?.UnregisterHandler(HandlerIdentInitModelFormers, out _);
        Operation?.UnregisterHandler(HandlerIdentLoadSaveXml, out _);
        Marshal.FinalReleaseComObject(_operationComWrapper?.Instance);
        _operationComWrapper?.Dispose();
        _curaControlProcess?.Dispose();
        _operationProps?.Dispose();
    }
    
    public bool GetPropIterator(string pageId,
        out IST_CustomPropIterator? iterator,
        out TResultStatus resultStatus)
    {
        resultStatus = default;
        iterator = null;
        
        try
        {
            var operation = _operationComWrapper.Instance;
            if (operation != null)
            {
                if (!_curaLibraryPath.CheckLibraryExists(operation.XMLProp))
                {
                    _curaControlProcess.Logger.Warning(_warningMessage);
                    ShowMessageBox(_warningMessage, TMessageDialogType.mdtWarning, (ushort)1, TUIButtonType.btOk, "");
                    return false;
                }
                else 
                {
                    InitConfigurations(ref resultStatus);
                }

                if (_curaParamsReceiver == null)
                    throw new Exception("CuraParamsReceiver is null");
                iterator = _operationProps?.CreateIterator(_curaParamsReceiver.CEParameters);
                return true;          
            }  
            return false;  
        }
        catch (Exception e)
        {
            resultStatus.Code = TResultStatusCode.rsError;
            resultStatus.Description = e.Message;
            return false;
        }
    }

    public void OnPropFilterChanged(string? parameterName, string? value)
    {
         if (parameterName == null)
             return;
         if (parameterName != "_searching")
             return;
         
         _operationProps?.FillAcceptedByFilter(value);
    }
    
    public void MakeWorkPath(ICamApiCLDReceiver cldFormer,
        ICamApiTechOperation techOperation,
        out TResultStatus resultStatus)
    {
        resultStatus = default;
        try
        {
            using var xmlPropCom = new ComWrapper<IST_XMLPropPointer>(techOperation.XMLProp);
            if (xmlPropCom.Instance == null)
                throw new Exception("XMLProp is null");
            if (!_curaLibraryPath?.CheckLibraryExists(xmlPropCom.Instance) ?? true)
            {
                _curaControlProcess.Logger.Warning(_warningMessage);
                ShowMessageBox(_warningMessage, TMessageDialogType.mdtWarning, (ushort)1, TUIButtonType.btOk, "");
                return;
            }
            InitConfigurations(ref resultStatus);
            FillParametersAndCalculate(techOperation, cldFormer);
            CuraEngineConnectionHelper.FinalizeLib();
        }
        catch (Exception e)
        {
            resultStatus.Code = TResultStatusCode.rsError;
            resultStatus.Description = e.Message;
        }
    }
    
    private void FillParametersAndCalculate(ICamApiTechOperation techOperation, ICamApiCLDReceiver cldReceiver)
    {
        if (_operationProps == null)
            throw new Exception("OperationProps is null");
        if (_curaControlProcess == null)
            throw new Exception("_curaControlProcess is null");
        if (_curaParamsReceiver == null)
            throw new Exception("_curaParamsReceiver is null");
        
        var curaPath = _curaParamsReceiver.CEParameters.CuraPath;
        var lib = CuraEngineConnectionHelper.LoadNativeLib(curaPath);
        if (lib == null)
            return;
        
        FillPoints(techOperation, lib.TrianglesReciever);
        _curaParamsReceiver.CEParameters.ParseAllParameters();
        _curaControlProcess.clf = cldReceiver;
        _curaControlProcess.OnGCodeCommandTranslation = _operationProps.GetGCodeCommandTranslation;
        _curaControlProcess.IsOutputAdditionalCLDataParameters = _operationProps.IsOutputAdditionalClDataParameters;
        _curaControlProcess.IsOutputFilamentExtruding = _operationProps.IsOutputFilamentExtruding;
        _curaControlProcess.tpm = _operationProps.Tpm;
        _curaControlProcess.FilamentExtrudingLength = _operationProps.FilamentExtrudingLength;
        
        lib.Slice(_curaControlProcess, _curaParamsReceiver, curaPath);
    }
 
    private void FillPoints(ICamApiTechOperation techOperation, ITrianglesReciever r)
    {
        if (_curaControlProcess == null)
            throw new Exception("_curaControlProcess is null");
        
        r.BeginTransfer();
        try
        {
            r.BeginModel();
            try
            {
                var tolerance = GetRealTolerance(techOperation);
                
                // get face list
                //TST3DMatrix newLCS = ((T3DMatrix)techOperation.LCS).InverseMatrix(); 
                TST3DMatrix newLCS = techOperation.LCS;    
                using var modelFormerJobAssignmentCom = new ComWrapper<ICamApiModelFormer>(techOperation.ModelFormerJobAssignment);
                var modelFormerJobAssignment = modelFormerJobAssignmentCom.Instance
                    ?? throw new Exception("ModelFormerJobAssignment is null");
                using var faceListCom = new ComWrapper<ICamApiFaceList>(modelFormerJobAssignment.GetFaceList(newLCS));
                var faceList = faceListCom.Instance
                    ?? throw new Exception("FaceList is null");
              
                if (faceList.Count > 0)
                { 
                    _curaControlProcess.boundingBox = modelFormerJobAssignment.GetBoundingBox(newLCS);
                    newLCS.vT = AcceptBoxToLCS(newLCS, _curaControlProcess.boundingBox); //       newLCS.vT + newLCS.vZ * bb.Min.Z
                    using var faceListNewCom = new ComWrapper<ICamApiFaceList>(modelFormerJobAssignment.GetFaceList(newLCS));
                    var faceListNew = faceListNewCom.Instance
                        ?? throw new Exception("FaceListNew is null");
                    FillTriangles(r, faceListNew, tolerance);
                }

                else
                {
                    using var modelFormerPartCom = new ComWrapper<ICamApiModelFormer>(techOperation.ModelFormerPart);
                    var modelFormerPart = modelFormerPartCom.Instance
                        ?? throw new Exception("ModelFormerPart is null");
                    _curaControlProcess.boundingBox = modelFormerPart.GetBoundingBox(newLCS);
                    newLCS.vT = AcceptBoxToLCS(newLCS, _curaControlProcess.boundingBox);
                    using var faceListNewCom = new ComWrapper<ICamApiFaceList>(modelFormerPart.GetFaceList(newLCS));
                    var faceListNew = faceListNewCom.Instance
                        ?? throw new Exception("FaceListNew is null");
                    FillTriangles(r, faceListNew, tolerance);  
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

    private void FillTriangles(ITrianglesReciever r, ICamApiFaceList faceList, double tolerance)
    {
        // calc total count of triangles
        var totalTriangleCount = 0;  
        for (var i = 0; i < faceList.Count; i++)
        {
            var mesh = faceList.GetMesh(i, tolerance, 0, true);
            totalTriangleCount += mesh.GetTriangleCount();
        }    
              
        // add triangles
        r.BeginMesh("Mesh", totalTriangleCount);
        try
        {
            for (var i = 0; i < faceList.Count; i++)
            {
                var mesh = faceList.GetMesh(i, tolerance, 0, true);
                var triangleCount = mesh.GetTriangleCount();
                for (var j = 0; j < triangleCount; j++)
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
  
    private TST3DPoint AcceptBoxToLCS(TST3DMatrix lcs, TST3DBox b) //newLCS.vT + newLCS.vZ * bb.Min.Z
    {
        var pT = lcs.vT + (T3DPoint)lcs.vZ * b.Min.Z;

        return pT;
    }
    
    private double GetRealTolerance(ICamApiTechOperation techOperation)
    {
        return techOperation.Units == TSTSystemUnits.suImperial ? 0.0006 : 0.006;
    }
    
    private static TCE3SPoint P3S(TST3DPoint p)
    {
        return new TCE3SPoint{
            X = (float)p.X,
            Y = (float)p.Y,
            Z = (float)p.Z
        }; 
    }
}