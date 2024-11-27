using System.Globalization;
using CAMAPI.Application;
using CAMAPI.DotnetHelper;
using CAMAPI.Extensions;
using CAMAPI.ResultStatus;

namespace CuraEngineOperation;

public class Localize
{
    private Dictionary<string, string>? _langCatalog;
    private Dictionary<string, string>? _userLangCatalog;
    private readonly string _currentLang = "en-US";
    
    public Localize(IExtensionInfo? info)
    {
        using var extensionCom = SystemExtensionFactory.GetSingletonExtension<ICamApiApplicationSingleton>(
            "Extension.Global.Singletons.Application", info);
        TResultStatus ret = default;
        var langCode = extensionCom.Instance?.GetApplication(out ret).LanguageCode ?? 0;
        if (ret.Code == TResultStatusCode.rsError)
            throw new Exception(ret.Description);
        if (langCode == 0)
            return;
      
        var cultureInfo = new CultureInfo(langCode);
        if (cultureInfo.Name != "")
            _currentLang = cultureInfo.Name;
    }
    
    public void ReadMainTranslations(string filePath)
    {
        if (_langCatalog==null)
            _langCatalog = ReadTranslations(filePath);
    }
    
    public void ReadUserTranslations(string filePath)
    {
        _userLangCatalog = ReadTranslations(filePath, true);
    }
  
    private Dictionary<string, string> ReadTranslations(string filePath, bool isUserTranslations = false)
    {
        var isFileExists = false;
        var path = "";
        var filename = "";
        filename = isUserTranslations ? ".po" : "\\fdmprinter.def.json.po";  

        path = Path.Combine(filePath, _currentLang + filename);
        if (File.Exists(path))
        {
            isFileExists = true;
        }
        else
        {
            var lang = _currentLang.Replace("-", "_");
            path = Path.Combine(filePath, lang + filename);
            if (File.Exists(path))
            {
                isFileExists = true;
            }
        }

        Dictionary<string, string>? translations = null;
        if (!isFileExists)
            return translations;
        
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
        return translations;
    }

    public string GetTranslation(string id, string caption = "")
    {
        var result = "";
        id = id.ToLower();
        caption = caption.ToLower();
        if (_userLangCatalog!=null)
        {
            if (caption!="")
            {
                if (_userLangCatalog.TryGetValue(caption, out result))
                    return result;
            }
            if (_userLangCatalog.TryGetValue(id, out result))
                return result;
        }
        _langCatalog?.TryGetValue(id, out result);
        return result;
    }
    
    public string GetLabelTranslation(string id, string label = "")
    {
        var label_ = "";
        if (label!="")
            label_ = label + " label";
        var caption = GetTranslation(id + " label", label_);
        if (!string.IsNullOrEmpty(caption))
            return caption;
        return label != "" ? label : id;
    }
    
    public string GetDescriptionTranslation(string id, string caption, string description)
    {
        var desc = GetTranslation(id + " description", caption + " description");
        if (string.IsNullOrEmpty(desc))
        {
            desc = description;
        }
        return desc;
    }
    
    public string GetEnumsTranslation(string ParamId, string ParamLabel, string enumItemId, string enumItemName)
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
}