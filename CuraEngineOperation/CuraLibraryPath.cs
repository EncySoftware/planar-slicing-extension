using System.Reflection;
using System.Text.Json;
using CAMAPI.DotnetHelper;
using CuraEngineParametersLibrary;
using Microsoft.Win32;
using STXMLPropTypes;

namespace CuraEngineOperation;

public class CuraLibraryPath
{
    public bool UserPathEnabled;
    public string CuraPath = "";
    private readonly string _userCuraPathFromJSon = "";
    private readonly string _autoCuraPath = "";
    public string UserCuraPath = "";

    public CuraLibraryPath(IST_XMLPropPointer xmlProp)
    {
        using var xmlPropCom = new ComWrapper<IST_XMLPropPointer>(xmlProp);
        var xmlPropObj = xmlPropCom.Instance
                      ?? throw new Exception("XMLProp is null");
        
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation)+"\\";
        var curaSettingsJsonFile = Path.Combine(assemblyDirectory, "CuraSettings.json");
        if (!File.Exists(curaSettingsJsonFile))
        {
            _autoCuraPath = ReadPathFromRegistry();
            var curaSettings = new CuraSettingsJson
            {
                Name = "CuraSettings",
                Settings = new SettingsJson
                {
                    PathToCura = _autoCuraPath,
                    UserPathToCura = ""
                }
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
  
            var jsonString = JsonSerializer.Serialize(curaSettings, options);
            File.WriteAllText(curaSettingsJsonFile, jsonString);
        }
        else
        {
            var jsonContent = File.ReadAllText(curaSettingsJsonFile);
            var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;
            if (root.TryGetProperty("settings", out var settingsElement))  
            {
                foreach (var childProperty in settingsElement.EnumerateObject())
                {   
                    if (childProperty.Name=="PathToCura")
                    {
                        var value = childProperty.Value.GetString() ?? "";
                        _autoCuraPath = value;
                    }
                    if (childProperty.Name=="UserPathToCura")
                    {
                        var value = childProperty.Value.GetString() ?? "";
                        UserCuraPath = value;
                        _userCuraPathFromJSon = value;
                        xmlPropObj.Ptr["CuraPath.Path"].ValueAsString = UserCuraPath;
                    }
                }
            }
            if (CuraPath.Trim() != "")
                return;
            
            _autoCuraPath = ReadPathFromRegistry();
            var json = File.ReadAllText(curaSettingsJsonFile);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            var curaSettings = JsonSerializer.Deserialize<CuraSettingsJson>(json, options)
                               ?? throw new InvalidOperationException("Failed to deserialize CuraSettingsJson");
            curaSettings.Settings.PathToCura = _autoCuraPath;
            var updatedJson = JsonSerializer.Serialize(curaSettings, options);
            File.WriteAllText(curaSettingsJsonFile, updatedJson);
        }
    }
    
    public bool CheckLibraryExists(IST_XMLPropPointer xmlProp)
    {
        UserPathEnabled = xmlProp.Ptr["CuraPath.SetPath"].ValueAsBoolean;
        if (_autoCuraPath=="" || !Directory.Exists(_autoCuraPath) || !CuraEnginePathCorrect(_autoCuraPath) || UserPathEnabled)
        {
            var userCuraPathFromXml = xmlProp.Ptr["CuraPath.Path"].ValueAsString;
            if (userCuraPathFromXml!="" && File.Exists(userCuraPathFromXml) && CuraEnginePathCorrect(userCuraPathFromXml))
            {
                CuraPath = @Path.GetDirectoryName(userCuraPathFromXml) + @"\";
                UserPathEnabled = true;
                xmlProp.Ptr["CuraPath.SetPath"].ValueAsBoolean = UserPathEnabled;
                xmlProp.Ptr["CuraPath.Path"].ValueAsString = userCuraPathFromXml;
                if (userCuraPathFromXml!=UserCuraPath)
                {
                    UserCuraPath = userCuraPathFromXml;
                    UpdateCuraPathJson(userCuraPathFromXml);
                }          
            }
            else
            {
                UserPathEnabled = xmlProp.Ptr["CuraPath.SetPath"].ValueAsBoolean; 
                if (!UserPathEnabled || userCuraPathFromXml=="" || !File.Exists(userCuraPathFromXml) || !CuraEnginePathCorrect(userCuraPathFromXml))
                {
                    if (userCuraPathFromXml!=null)
                        UserCuraPath = userCuraPathFromXml;
                    return false;
                }
  
                CuraPath = @Path.GetDirectoryName(userCuraPathFromXml) + @"\";
                if (userCuraPathFromXml!=UserCuraPath)
                {
                    UserCuraPath = userCuraPathFromXml;
                    UpdateCuraPathJson(userCuraPathFromXml);
                }
            }
        }
        else
        {
            UserCuraPath = _userCuraPathFromJSon;
            CuraPath = _autoCuraPath;
        }
        return true;
    }
    
    private static bool CuraEnginePathCorrect(string path)
    {
        var exePath = Path.Combine(Path.GetDirectoryName(path) ?? "", "CuraEngine.exe");
        return File.Exists(exePath);
    }
    
    private void UpdateCuraPathJson(string userPath)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation)+"\\";
        var curaSettingsJsonFile = assemblyDirectory+"CuraSettings.json";
        if (!File.Exists(curaSettingsJsonFile))
            return;
        File.Delete(curaSettingsJsonFile);
        CuraPath = ReadPathFromRegistry();
        var curaSettings = new CuraSettingsJson
        {
            Name = "CuraSettings",
            Settings = new SettingsJson
            {
                PathToCura = _autoCuraPath,
                UserPathToCura = userPath
            }
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    
        var jsonString = JsonSerializer.Serialize(curaSettings, options);
        File.WriteAllText(curaSettingsJsonFile, jsonString);
    }
    
    private static string ReadPathFromRegistry()
    {
        var path = "";
        var classesRootKey = Registry.ClassesRoot;
        var curaKey = classesRootKey.OpenSubKey("cura\\DefaultIcon");
  
        curaKey = null;
        if (curaKey!=null)
        {
            path = curaKey.GetValue("").ToString();
            if (path!="")
                path = path.Remove(path.LastIndexOf("\\") + 1);
        } 
        if (path=="")
        {
            var curaPackageKey = classesRootKey.OpenSubKey("curapackage\\shell\\open_curapackage\\command");
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
            var curaModelKey = classesRootKey.OpenSubKey("Cura.model\\DefaultIcon");
            if (curaModelKey!=null)
            {
                path = curaModelKey.GetValue("").ToString();
                if (path!="") 
                    path = path.Remove(path.LastIndexOf("\\") + 1);
            }      
        }
        return path;
    }
}