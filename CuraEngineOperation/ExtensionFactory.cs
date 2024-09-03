namespace CAMAPI;

using CAMAPI.Extensions;
using CAMAPI.ResultStatus;
using CAMAPI.Singletons;
using System.Reflection;
using System;
using System.Xml;
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
    private XmlNode GetOperationsNode(XmlNode rootNode)
    {
        if (rootNode!=null)
        {
            foreach (XmlNode childNode in rootNode.ChildNodes)
            {
                if (childNode.Attributes != null && childNode.Attributes["ID"] != null && childNode.Attributes["ID"].Value == "Operations")
                {
                    return childNode;
                }
            }
        }  
        return null;
    } 
    private void DeleteOperationNode(XmlNode operationsNode, string OperationXMlName)
    {
        if (operationsNode!=null)
        {
            foreach (XmlNode childNode in operationsNode.ChildNodes)
            {
                if (childNode.InnerText.ToLower().EndsWith(OperationXMlName)) 
                {
                    operationsNode.RemoveChild(childNode);
                }
            }
        }
    } 
    public void OnLibraryRegistered(IExtensionFactoryContext Context, out TResultStatus ret)
    {
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        string OperationXMlName = "CuraEngineToolpath_ExtOp.xml";
        string pathToOperationXML = Path.GetDirectoryName(assemblyLocation) + "\\" + OperationXMlName;
        string pathToUserOperationsList = Context.Paths.TryUnfoldPath(@"$(OPERATIONS_FOLDER)\UserOperationsList.xml");
        if (File.Exists(pathToUserOperationsList))
        {
            try
            {
                XmlDocument UserOperationsListDoc = new XmlDocument();
                UserOperationsListDoc.Load(pathToUserOperationsList);
                XmlNode rootNode = UserOperationsListDoc.DocumentElement;     
                if (rootNode!=null)
                {
                    XmlNode operationsNode = GetOperationsNode(rootNode);
                    if (operationsNode!=null)
                    {
                        DeleteOperationNode(operationsNode, OperationXMlName);
                        XmlElement newOperationElement = UserOperationsListDoc.CreateElement("SCInclude");
                        newOperationElement.InnerText = pathToOperationXML;
                        XmlAttribute optionalAttr = UserOperationsListDoc.CreateAttribute("Optional");
                        optionalAttr.Value = "true";
                        newOperationElement.Attributes.Append(optionalAttr);
                        operationsNode.AppendChild(newOperationElement);
                        UserOperationsListDoc.Save(pathToUserOperationsList);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        else
        {
             try
            {
                XmlDocument UserOperationsListDoc = new XmlDocument();
                XmlDeclaration xmlDeclaration = UserOperationsListDoc.CreateXmlDeclaration("1.0", null, null);
                UserOperationsListDoc.InsertBefore(xmlDeclaration, UserOperationsListDoc.DocumentElement);

                XmlElement rootElement = UserOperationsListDoc.CreateElement("SCCollection");
                XmlAttribute rootIDAttr = UserOperationsListDoc.CreateAttribute("ID");
                rootIDAttr.Value = @"$(OPERATIONS_FOLDER)\UserOperationsList.xml";
                rootElement.Attributes.Append(rootIDAttr);
                UserOperationsListDoc.AppendChild(rootElement);

                XmlElement nameSpaceElement = UserOperationsListDoc.CreateElement("SCNameSpace");
                XmlAttribute nameSpaceIDAttr = UserOperationsListDoc.CreateAttribute("ID");
                nameSpaceIDAttr.Value = "Operations";
                nameSpaceElement.Attributes.Append(nameSpaceIDAttr);
                rootElement.AppendChild(nameSpaceElement);

                XmlElement newOperationElement = UserOperationsListDoc.CreateElement("SCInclude");
                newOperationElement.InnerText = pathToOperationXML;
                XmlAttribute optionalAttr = UserOperationsListDoc.CreateAttribute("Optional");
                optionalAttr.Value = "true";
                newOperationElement.Attributes.Append(optionalAttr);
                nameSpaceElement.AppendChild(newOperationElement);
                UserOperationsListDoc.Save(pathToUserOperationsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        ret = default;
    }

    public void OnLibraryUnRegistered(IExtensionFactoryContext Context, out TResultStatus ret)
    {
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        string OperationXMlName = "CuraEngineToolpath_ExtOp.xml";
        string pathToOperationXML = Path.GetDirectoryName(assemblyLocation) + "\\" + OperationXMlName;
        string pathToUserOperationsList = Context.Paths.TryUnfoldPath(@"$(OPERATIONS_FOLDER)\UserOperationsList.xml");
        if (File.Exists(pathToUserOperationsList))
        {
            try
            {
                XmlDocument UserOperationsListDoc = new XmlDocument();
                UserOperationsListDoc.Load(pathToUserOperationsList);
                XmlNode rootNode = UserOperationsListDoc.DocumentElement;     
                if (rootNode!=null)
                {
                    XmlNode operationsNode = GetOperationsNode(rootNode);
                    if (operationsNode!=null)
                    {
                        DeleteOperationNode(operationsNode, OperationXMlName);
                        UserOperationsListDoc.Save(pathToUserOperationsList);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        ret = default;
    }
}