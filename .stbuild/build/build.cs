using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using BuildSystem;
using BuildSystem.Info;
using BuildSystem.ProjectList;
using Loggers;
using Logging;
using LoggingLevel = Logging.LogLevel;
using Nuke.Common;
using Nuke.Common.Utilities.Collections;

namespace stbuild;

/// <inheritdoc />
[SuppressMessage("ReSharper", "AllUnderscoreLocalParameterName")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class Build : NukeBuild
{
    /// <summary>
    /// Calling target by default
    /// </summary>
    public static int Main()
    {
        var parentDirectory = new DirectoryInfo(EnvironmentInfo.WorkingDirectory)
            .DescendantsAndSelf(x => x.Parent ?? throw new Exception("Parent directory is null for " + x.FullName))
            .First(x => x.GetDirectories(".stbuild").Any())
            .FullName;
        Environment.SetEnvironmentVariable("root", Path.Combine(parentDirectory, ".stbuild"));
        return Execute<Build>(x => x.Compile);
    }

    /// <summary>
    /// Build variant. Valid values: Debug_x64, Release_x64.
    /// </summary>
    [Parameter("Build variant")]
    public readonly string Variant = "Debug_x64";

    /// <summary> Logging level. </summary>
    [Parameter("Logging level")]
    public readonly string LogLevel = "info";

    /// <summary>
    /// Force rebuild of projects even if they are up to date.
    /// </summary>
    [Parameter("Force build of projects")]
    public readonly string ForceBuild = "false";

    private LoggingLevel _logLevel => LogLevel.ToLower() switch {
        "debug"   => LoggingLevel.debug,
        "verbose" => LoggingLevel.verbose,
        "head"    => LoggingLevel.head,
        _         => LoggingLevel.info
    };

    private ILogger? _logger;
    public ILogger Logger => _logger ??= InitLogger();

    private ILogger InitLogger() {
        var bcaster = new LoggerBroadCaster();
        var console = new LoggerConsole();
        var file = new LoggerFile(RootDirectory + "//logs", "enc", 12);
        file.setMinLevel(LoggingLevel.debug);
        console.setMinLevel(_logLevel);
        bcaster.Loggers.Add(file);
        bcaster.Loggers.Add(console);
        return bcaster;
    }

    private IBuildSpace? _buildSpace;
    private IBuildSpace BSpace => _buildSpace ??= InitBuildSpace();

    private IBuildSpace InitBuildSpace() {
        var localJsonFile = Path.Combine(RootDirectory, $"buildspace.{BuildInfo.RunParams[RunInfo.Local]}.json");
        var bsJsonFile = Path.Combine(RootDirectory, "buildspace.json");
        var config = new BuildSpaceSettings(Logger, [bsJsonFile, localJsonFile], Variant);
        return new BuildSpaceCommon(Logger, RootDirectory + "//temp", SettingsReaderType.Object, config);
    }

    /// <summary>
    /// Initialise shared build constants.
    /// </summary>
    private Target SetBuildInfo => _ => _
        .Executes(() => {
            BuildInfo.RunParams[RunInfo.Variant] = Variant;
            BuildInfo.RunParams[RunInfo.Local] = "local";
            BuildInfo.RunParams[RunInfo.ForceBuild] = ForceBuild;
            foreach (var runParam in BuildInfo.RunParams)
                Logger.debug($"{runParam.Key}: {runParam.Value}");
        });

    /// <summary>
    /// Restore build space NuGet dependencies.
    /// </summary>
    private Target Restore => _ => _
        .DependsOn(SetBuildInfo)
        .Executes(() =>
        {
            BSpace.Projects.Restore(Variant);
        });

    /// <summary>
    /// Full build: IDL (Delphi), C++ (conan/cmake), and C# projects.
    /// Requires all toolchains to be installed. Use for local development.
    /// </summary>
    private Target Compile => _ => _
        .DependsOn(SetBuildInfo)
        .After(Restore)
        .Executes(() =>
        {
            var config = BuildUtils.Configuration(Variant);
            var conanCmd = Path.Combine(RootDirectory, $"../CuraEngineConnection/conan_build/Build_{config}.bat");

            BSpace.Projects.Compile(Variant, true);
            var res = BuildUtils.RunProcAs(conanCmd, "");
            if (res != 0) throw new Exception("Conan or CMake build error");
        });

    /// <summary>
    /// Build IDL and C# projects. Used by CI where Conan/CMake are not available.
    /// IDL is compiled via MIDL + tlbimp (Windows SDK tools on windows-latest).
    /// CuraEngineConnection.dll (C++ / Conan) is sourced from resources/ tracked in git LFS.
    /// </summary>
    private Target CompileDotnet => _ => _
        .DependsOn(SetBuildInfo)
        .After(Restore)
        .Executes(() =>
        {
            BSpace.Projects.Compile(Variant, true,
                project => string.Equals(project.Type, "CSharp", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(project.Type, "Idl",    StringComparison.OrdinalIgnoreCase));
        });

    /// <summary>
    /// Create NuGet package without publishing. Requires the full toolchain.
    /// Run this locally before reviewing the package contents.
    /// </summary>
    private Target Pack => _ => _
        .DependsOn(SetBuildInfo, Compile)
        .Executes(() =>
        {
            BSpace.Projects.Deploy(Variant, true, _ => true);
        });

    /// <summary>
    /// Build C# projects, create NuGet package, and publish to the feed.
    /// Intended for CI. Native DLLs are sourced from resources/.
    /// Requires NUGET_FEED_URL and NUGET_AUTH_TOKEN environment variables.
    /// </summary>
    private Target Push => _ => _
        .DependsOn(SetBuildInfo, CompileDotnet)
        .Executes(() =>
        {
            BSpace.Projects.Deploy(Variant, false, _ => true);
        });

    /// <summary>
    /// Clean all build artifacts.
    /// </summary>
    private Target Clean => _ => _
        .DependsOn(SetBuildInfo)
        .Executes(() =>
        {
            BSpace.Projects.Clean(Variant);
        });
}
