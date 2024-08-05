namespace CAMAPI;

using CAMAPI.Extensions;
using CAMAPI.ResultStatus;

/// <summary>
/// Singleton factory for creating extensions
/// </summary>
public class ExtensionFactory : IExtensionFactory
{
    /// <summary>
    /// Create extension by identifier from json-file
    /// </summary>
    /// <param name="extensionIdent">Identifier from json-file</param>
    /// <param name="ret">Structure to provide error text</param>
    /// <returns>New instance of extension</returns>
    public IExtension? Create(string extensionIdent, out TResultStatus ret)
    {
        if (String.Equals(extensionIdent, "CuraEngineToolpath", StringComparison.InvariantCultureIgnoreCase))
        {
            ret.Code = TResultStatusCode.rsSuccess;
            ret.Description = string.Empty;
            return new CuraEngineOperation.CuraEngineToolpath();
        } else {
            ret.Code = TResultStatusCode.rsError;
            ret.Description = "Unknown extension identifier: " + extensionIdent;
            return null;
        }
    }

    public void OnLibraryRegistered(IExtensionFactoryContext Context, out TResultStatus ret)
    {
        ret = default;
    }

    public void OnLibraryUnRegistered(IExtensionFactoryContext Context, out TResultStatus ret)
    {
        ret = default;
    }
}