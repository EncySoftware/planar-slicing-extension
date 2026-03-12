using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Utils.AdminRunner;

namespace stbuild;

internal abstract class BuildUtils
{
    private const StringComparison IgnCase = StringComparison.InvariantCultureIgnoreCase;
    private static AdminRunner? _adminRunner;

    /// <summary>
    /// Converting variant to configuration
    /// </summary>
    /// <param name="variant"> Example Release_x64 </param>
    /// <returns> Release/Debug configuration </returns>
    public static string Configuration(string variant)
    {
        var config = variant[..variant.IndexOf('_')];
        return config.Equals("debug", IgnCase) ? "Debug" : "Release";
    }

    /// <summary>
    /// Parametrized process runner
    /// </summary>
    /// <returns> Exit code </returns>
    public static int RunProcAs(string fileName, string args, string workDir = "", int timeout = 60000, bool runas = false)
    {
        using var p = new Process();
        p.StartInfo.FileName = fileName;
        p.StartInfo.Arguments = args;
        try {
            if (!string.IsNullOrEmpty(workDir) && Directory.Exists(workDir))
                p.StartInfo.WorkingDirectory = workDir;
            else
                p.StartInfo.WorkingDirectory = Path.GetDirectoryName(fileName);
            if (runas) {
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.Verb = "runas";
            }
            if (p.Start() && p.WaitForExit(timeout))
                return p.ExitCode;
        } catch (Exception ex) {
            Console.WriteLine("BuildUtils.RunProcAs: " + ex.Message);
        }

        return 1;
    }

    public static string[] GetJsonArrayValue(string jsonPath, string arrayPropertyName)
    {
        var resList = new List<string>();
        if (!File.Exists(jsonPath))
            return resList.ToArray();
        
        var jsonObj = JObject.Parse(File.ReadAllText(jsonPath));
        if (!jsonObj.TryGetValue(arrayPropertyName, IgnCase, out var projects))
            return resList.ToArray();

        resList.AddRange(projects.Select(project => project.ToString()));
        return resList.ToArray();
    }
}