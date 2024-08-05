using System;
using System.IO;
using Nuke.Common;
using BuildSystem.BuildSpace;
using BuildSystem.BuildSpace.Common;
using BuildSystem.Info;
using BuildSystem.Loggers;
using BuildSystem.Logging;
using BuildSystem.SettingsReader.Object;
using BuildSystem.SettingsReader;

/// <inheritdoc />
public class Build : NukeBuild
{
    /// <summary>
    /// Calling target by default
    /// </summary>
    public static int Main() => Execute<Build>(x => x.Compile);
    
    /// <summary>
    /// Configuration to build - 'Debug' (default) or 'Release'
    /// </summary>
    [Parameter("Settings provided for running build space")]
    public readonly string Variant = "Debug_x64";

    /// <summary> Logging level </summary>
    [Parameter("Logging level")]
    public readonly string LogLevel = "info";

    /// <summary>
    /// Force build of projects, even if they are up to date
    /// </summary>
    [Parameter("Force build of projects")]
    public readonly string ForceBuild = "false";

    /// <summary> Build Space logger </summary>
    public static ILogger Logger = new LoggerConsole();

    private IBuildSpace? _buildSpace;
    private IBuildSpace BSpace => _buildSpace ??= InitBuildSpace();

    private IBuildSpace InitBuildSpace() {
        var localJsonFile = Path.Combine(RootDirectory, $"buildspace.{BuildInfo.RunParams[RunInfo.Local]}.json");
        var bsJsonFile = Path.Combine(RootDirectory, "buildspace.json");
        SettingsObject config = new BuildSpaceSettings(Logger, new[] { bsJsonFile, localJsonFile }, Variant);
        return new BuildSpaceCommon(Logger, RootDirectory + "//temp", SettingsReaderType.Object, config);
    }

    /// <summary> 
    /// Set build constants 
    /// </summary>
    private Target SetBuildInfo => _ => _
        .Executes(() => {
            switch (LogLevel) {
                case "debug": Logger.setMinLevel(BuildSystem.Logging.LogLevel.debug); break;
                case "verbose": Logger.setMinLevel(BuildSystem.Logging.LogLevel.verbose); break;
                case "head": Logger.setMinLevel(BuildSystem.Logging.LogLevel.head); break;
                default: Logger.setMinLevel(BuildSystem.Logging.LogLevel.info); break;
            }

            // init static params
            BuildInfo.RunParams[RunInfo.Variant] = Variant;
            BuildInfo.RunParams[RunInfo.Local] = "local";
            BuildInfo.RunParams[RunInfo.ForceBuild] = ForceBuild;
            foreach (var runParam in BuildInfo.RunParams)
                Logger.debug($"{runParam.Key}: {runParam.Value}");
        });

    /// <summary>
    /// Restore build space dependencies
    /// </summary>
    private Target Restore => _ => _
        .DependsOn(SetBuildInfo)
        .Executes(() =>
        {
            BSpace.Projects.Restore(Variant);
        });

    /// <summary>
    /// Parameterized compile
    /// </summary>
    private Target Compile => _ => _
        .DependsOn(SetBuildInfo)
        .After(Restore)
        .Executes(() =>
        {
            var config =  BuildUtils.Configuration(Variant);
            var outdir = Path.Combine(RootDirectory, $"../CuraEngineConnection/build/{config}");
            var conan_cmd_release = Path.Combine(RootDirectory, $"../CuraEngineConnection/conan_build/Build_{config}.bat");
           // BuildUtils.ClearFolder(outdir);

            BSpace.Projects.Compile(Variant, true);
            var res = BuildUtils.RunProcAs(conan_cmd_release, "");
            if (res != 0) throw new Exception("Conan or CMake build error");
        });

    /// <summary>
    /// Publishing packages
    /// </summary>
    private Target Deploy => _ => _
        .DependsOn(SetBuildInfo, Compile)
        .Executes(() =>
        {
            BSpace.Projects.Deploy(Variant, true);
        });

    /// <summary>
    /// Parameterized clean
    /// </summary>
    private Target Clean => _ => _
        .DependsOn(SetBuildInfo)
        .Executes(() =>
        {
            BSpace.Projects.Clean(Variant);
        });

}