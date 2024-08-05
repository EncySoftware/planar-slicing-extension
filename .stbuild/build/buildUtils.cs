using System;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using BuildSystem.Utils.AdminRunner;

class BuildUtils {
    
    public const StringComparison IGN_CASE = StringComparison.InvariantCultureIgnoreCase;

    private static AdminRunner? _adminRunner;
    public static AdminRunner CommandRunner => _adminRunner ??= new AdminRunner(true);

    /// <summary> Converting variant to platform </summary>
    /// <param name="variant"> Example Release_x64 </param>
    /// <returns> Win64/Win32 platform </returns>
    public static string WinPlatform(string variant) {
        string platf = variant.Substring(variant.IndexOf("_") + 1);
        return platf == "x32" ? "Win32" : "Win64";
    }

    /// <summary> Converting variant to configuration </summary>
    /// <param name="variant"> Example Release_x64 </param>
    /// <returns> Release/Debug configuration </returns>
    public static string Configuration(string variant) {
        string config = variant.Substring(0, variant.IndexOf("_"));
        return config.Equals("debug", IGN_CASE) ? "Debug" : "Release";
    }

    /// <summary> Parametrized process runner </summary>
    /// <returns> Exit code </returns>
    public static int RunProcAs(string fileName, string args, string workDir = "", int timeout = 60000, bool runas = false) {
        using (Process p = new Process()) {
            p.StartInfo.FileName = fileName;
            p.StartInfo.Arguments = args;
            try {
                if (!String.IsNullOrEmpty(workDir) && Directory.Exists(workDir))
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
        }
        return 1;
    }

    /// <summary> Running cmd.exe and exucutes all 'commandLines' </summary>
    /// <returns> Exit code </returns>
    public static int ExecuteCMD(string[] commandLines, string workdir = "", int timeout = 60000) {
        try {
            using (Process cmdProc = new Process()) {
                cmdProc.StartInfo.FileName = "cmd.exe";
                cmdProc.StartInfo.RedirectStandardInput = true;
                cmdProc.StartInfo.UseShellExecute = false;
                if (!String.IsNullOrEmpty(workdir) && Directory.Exists(workdir))
                    cmdProc.StartInfo.WorkingDirectory = workdir;
                // start cmd.exe
                if (!cmdProc.Start()) {
                    Console.WriteLine("BuildUtils.ExecuteCMD error: cmd.exe was not started.");
                    return 1;
                }
                // execute cmd lines
                using (var pWriter = cmdProc.StandardInput) {
                    foreach (var line in commandLines)
                        pWriter.WriteLine(line);
                }
                // waiting for execution
                if (cmdProc.WaitForExit(timeout))
                    return cmdProc.ExitCode;
            }
        } catch(Exception ex) {
            Console.WriteLine("BuildUtils.ExecuteCMD error: " + ex.Message);
        }
        return 1;
    }

    public static string[] GetJsonArrayValue(string jsonPath, string arrayPropertyName) {
        var resList = new List<string>();
        if (File.Exists(jsonPath)) {
            var jsonObj = JObject.Parse(File.ReadAllText(jsonPath));
            if (jsonObj.TryGetValue(arrayPropertyName, IGN_CASE, out var jprojs))
                foreach (var jproj in jprojs)
                    resList.Add(jproj.ToString());
        }
        return resList.ToArray();
    }

    /// <summary> Clear all files and directories in a specified folder </summary>
    public static void ClearFolder(string folderName) {
        if (string.IsNullOrEmpty(folderName) || !Directory.Exists(folderName)) return;
        var dir = new DirectoryInfo(folderName);
        foreach (var f in dir.GetFiles()) f.Delete();
        foreach (var d in dir.GetDirectories()) d.Delete(true);
    }

}