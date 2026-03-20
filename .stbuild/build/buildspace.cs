using System;
using System.Collections.Generic;
using System.IO;
using BuildSystem.Core.Builders.Dotnet;
using BuildSystem.Core.Builders.Midl;
using BuildSystem.Core.Builders.MsDelphi;
using BuildSystem.Core.Cleaner;
using BuildSystem.Core.HashGenerator;
using BuildSystem.Core.PackageManager;
using BuildSystem.Core.ProjectCache;
using BuildSystem.Core.TestRunner;
using BuildSystem.Core.TlbGenBpl;
using BuildSystem.Core.TlbGenDotnetDll;
using BuildSystem.Core.TlbGenPas;
using BuildSystem.Core.VersionManager;
using BuildSystem.ManagerObject.Interfaces;
using BuildSystem.ManagerObject.Interfaces.Package;
using BuildSystem.ManagerObject.Interfaces.Variants;
using BuildSystem.ProjectList;
using BuildSystem.ProjectList.Restorer;
using Logging;
using Nuke.Common;

namespace stbuild;

/// <inheritdoc />
internal class BuildSpaceSettings : SettingsObject
{
    private readonly string? _config;
    private static string GitBranch => Environment.GetEnvironmentVariable("GITHUB_REF_NAME") + "";

    private readonly ReaderJson _readerJson;

    /// <inheritdoc />
    public BuildSpaceSettings(ILogger logger, string[] configFiles, string variant)
    {
        _readerJson = new ReaderJson(logger);
        _readerJson.ReadRules(configFiles);
        ReaderLocalVars = _readerJson.LocalVars;
        ReaderDefines = _readerJson.Defines;
        Projects = GetProjectList(configFiles);
        ProjectListProps = new ProjectListCommonProps(logger)
        {
            SetStorageInfo = SetStorageInfoFunc
        };
        _config = BuildUtils.Configuration(variant);
        RegisterBuildSystemObjects();
    }

    /// <summary> Returns full path </summary>
    /// <param name="relPath"> Input relative path </param>
    private static string FPath(string relPath) => Path.GetFullPath(Path.Combine(NukeBuild.RootDirectory, relPath));

    /// <summary>
    /// Reads a list of projects from configFiles
    /// </summary>
    /// <param name="configFiles"> Json configuration file paths </param>
    private HashSet<string> GetProjectList(string[] configFiles)
    {
        var resultList = new HashSet<string>();
        foreach (var config in configFiles)
        {
            if (!File.Exists(config)) 
                continue;
            var configDir = Path.GetDirectoryName(config) + "";
            var projects = BuildUtils.GetJsonArrayValue(config, "projects");
            foreach (var project in projects)
            {
                var projPath = Path.GetFullPath(Path.Combine(configDir, project));
                if (File.Exists(projPath))
                    resultList.Add(projPath);
            }
            
        }
        return resultList;
    }

    /// <summary>
    /// Register Build System control objects
    /// </summary>
    private void RegisterBuildSystemObjects()
    {
        Variants =
        [
            new Variant
            {
                Name = "Debug_x64",
                Configurations = new Dictionary<string, string> { [Variant.NodeConfig] = "Debug" },
                Platforms = new Dictionary<string, string> { [Variant.NodePlatform] = "Win64", [Variant.NodePlatform + "_CSharp"] = "x64" }
            },

            new Variant
            {
                Name = "Release_x64",
                Configurations = new Dictionary<string, string> { [Variant.NodeConfig] = "Release" },
                Platforms = new Dictionary<string, string> { [Variant.NodePlatform] = "Win64", [Variant.NodePlatform + "_CSharp"] = "x64" },
            }
        ];

        // names in ManagerConstNames
        AddManagerProp("builder_csharp", null, BuilderDotNet);
        AddManagerProp("builder_idl", null, BuilderIdl);
        AddManagerProp("hash_generator", null, HashGeneratorCommon);
        AddManagerProp("restorer", null, RestorerNuget);
        AddManagerProp("cleaner", null, CleanerCommon);
        // AddManagerProp("test_runner", null, TestRunnerPropsCommon);
        AddManagerProp("project_cache", null, ProjectCacheCommon);
        AddManagerProp("package_manager", null, PackageManagerDotnet);
        AddManagerProp("version_manager", null, VersionManagerCommon);
    }
    
    private List<StorageInfo> SetStorageInfoFunc(PackageAction packageAction, string packageId, VersionProp? packageVersion)
    {
        // add the main feed anyway
        var result = new List<StorageInfo>
        {
            new()
            {
                Url = Environment.GetEnvironmentVariable("NUGET_FEED_URL")
                      ?? throw new Exception("Environment variable NUGET_FEED_URL is not set"),
                ApiKey = Environment.GetEnvironmentVariable("NUGET_AUTH_TOKEN") ?? ""
            }
        };
        
        // for search purposes add other feeds
        if (packageAction != PackageAction.Push)
        {
            result.Add(new StorageInfo
            {
                Url = "https://nexus.encycam.com/repository/master/index.json"
            });
        }
        
        // result
        return result;
    }

    private PackageManagerDotnetProps PackageManagerDotnet => new()
    { 
        Name = "package_manager_dotnet",
        SetStorageInfo = SetStorageInfoFunc
    };

    private VersionManagerCommonProps VersionManagerCommon => new()
    {
        Name = "version_manager_common",
        DepthSearch = 2,
        DevelopBranchName = GitBranch.EndsWith("develop", StringComparison.OrdinalIgnoreCase)
            ? GitBranch
            : "develop",
        MasterBranchName = GitBranch.EndsWith("main", StringComparison.OrdinalIgnoreCase)
            ? GitBranch
            : "main",
        ReleaseBranchName = GitBranch.EndsWith("release", StringComparison.OrdinalIgnoreCase)
            ? GitBranch
            : "release"
    };

    private static BuilderDotnetProps BuilderDotNet => new()
    { 
        Name = "builder_csharp_main" 
    };

    // private BuilderMsDelphiProps BuilderDelphiCommon => new()
    // {
    //     Name = "builder_delphi_common",
    //     BuilderVersion = "23.0",
    //     MsBuilderPath = _readerJson.LocalVars["msbuilder_path"],
    //     EnvBdsPath = _readerJson.LocalVars["env_bds"],
    //     RsVarsPath = _readerJson.LocalVars["rsvars_path"],
    //     AutoClean = true,
    //     BuildParams = new Dictionary<string, string?>
    //     {
    //         ["-verbosity"] = "normal",
    //         ["-consoleloggerparameters"] = "ErrorsOnly",
    //         ["-nologo"] = "true",
    //         ["/t:build"] = "true",
    //         ["/p:DCC_Hints"] = "false",
    //         ["/p:DCC_MapFile"] = "3",
    //         ["/p:DCC_AssertionsAtRuntime"] = "true",
    //         ["/p:DCC_IOChecking"] = "true",
    //         ["/p:DCC_WriteableConstants"] = "true"
    //     }
    // };
    //
    // private BuilderMsDelphiProps BuilderDelphiRelease
    // {
    //     get {
    //         var bdr = new BuilderMsDelphiProps(BuilderDelphiCommon)
    //         {
    //             Name = "builder_delphi_release"
    //         };
    //         bdr.BuildParams.Add("/p:DCC_Optimize", "true");
    //         bdr.BuildParams.Add("/p:DCC_GenerateStackFrames", "true");
    //         bdr.BuildParams.Add("/p:DCC_DebugInformation", "0");
    //         bdr.BuildParams.Add("/p:DCC_DebugDCUs", "false");
    //         bdr.BuildParams.Add("/p:DCC_LocalDebugSymbols", "false");
    //         bdr.BuildParams.Add("/p:DCC_SymbolReferenceInfo", "0");
    //         bdr.BuildParams.Add("/p:DCC_IntegerOverflowCheck", "false");
    //         bdr.BuildParams.Add("/p:DCC_RangeChecking", "false");
    //         return bdr;
    //     }
    // }
    //
    // private BuilderMsDelphiProps BuilderDelphiIdl
    // {
    //     get {
    //         var result = new BuilderMsDelphiProps(BuilderDelphiRelease)
    //         {
    //             Name = "builder_delphi_midl"
    //         };
    //         result.BuildParams.Add("/p:DCC_DefaultNamespace", "IDL;$(DCC_DefaultNamespace)");
    //         result.BuildParams.Add("/p:DCC_BplOutput", FPath("../CuraEngineConnection/build/debug"));
    //         result.BuildParams.Add("/p:DCC_DcpOutput", FPath("../CuraEngineConnection/build/debug/dcu"));
    //         result.BuildParams.Add("/p:DCC_DcuOutput", FPath("../CuraEngineConnection/build/debug/dcu/$native_project:name$"));
    //         result.BuildParams.Add("/p:DCC_UnitSearchPath", FPath("../CuraEngineConnection/build/debug/dcu"));
    //         return result;
    //     }
    // }

    // private TlbGenPasLibImpProps TlbGenPas => new()
    // {
    //     Name = "tlb_genpas_main",
    //     RunAddNameSpaces = true,
    //     EnvBdsPath = _readerJson.LocalVars["env_bds"],
    //     GenDoc = false
    // };
    //
    // private TlbGenBplThroughPasProps TlbGenBpl => new()
    // {
    //     Name = "tlb_genbpl_main",
    //     BuilderProps = BuilderDelphiIdl,
    //     GenPasProps = TlbGenPas,
    //     GenDoc = false
    // };

    private static TlbGenDotnetDllTlbImpProps TlbGenDotnetDll => new()
    {
        Name = "tlb_gen_dotnet_dll_main",
        GenDoc = false
    };

    private BuilderMidlProps BuilderIdl => new()
    {
        BuilderVersion = "MIDL_6.00.0366;TLIBIMP_12.16581;TLDotNet_4.8.4084.0",
        Name = "builder_midl_main",
        // TlbGenPasProps = TlbGenPas,
        // TlbGenBplProps = TlbGenBpl,
        PropsTlbGenDotnetDll = TlbGenDotnetDll,
        SearchDirIdl =    FPath($"../CuraEngineConnection/build/{_config}/idl"),
        SearchDirTlb =    FPath($"../CuraEngineConnection/build/{_config}/tlb"),
        IdlOutput =       FPath($"../CuraEngineConnection/build/{_config}/idl"),
        TlbOutput =       FPath($"../CuraEngineConnection/build/{_config}/tlb"),
        DotnetDllOutput = FPath($"../CuraEngineConnection/build/{_config}"),
        // PasOutput =       FPath($"../CuraEngineConnection/build/{_config}/dcu/$native_project:name$"),
        HOutput =         FPath($"../CuraEngineConnection/include/utils"),  
        PasFileName = "IDL.$native_project:name$"
    };

    // private TestRunnerCommonProps TestRunnerPropsCommon => new()
    // {
    //     Name = "test_runner_common",
    //     Compile = false,
    //     BuilderDprojProps = BuilderDelphiRelease
    // };

    private static ProjectCacheCommonProps ProjectCacheCommon => new()
    {
        Name = "project_cache_main",
        TempDir = "./hash"
    };

    private static HashGeneratorCommonProps HashGeneratorCommon => new()
    {
        Name = "hash_generator_main",
        HashAlgorithmType = HashAlgorithmType.Sha256
    };

    private static CleanerCommonProps CleanerCommon => new()
    {
        Name = "cleaner_default_main",
        AllBuildResults = true
    };

    private static RestorerNugetProps RestorerNuget => new()
    { 
        Name = "restorer_main",
        DepsProp = []
    };
}