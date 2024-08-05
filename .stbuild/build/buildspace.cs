using System;
using System.IO;
using System.Collections.Generic;
using BuildSystem.Builder.MsDelphi;
using BuildSystem.SettingsReader;
using BuildSystem.SettingsReader.Object;
using BuildSystem.Cleaner.Common;
using BuildSystem.HashGenerator.Common;
using BuildSystem.ProjectCache.Common;
using BuildSystem.Restorer.Nuget;
using BuildSystem.Variants;
using BuildSystem.TlbGenPas.LibImp;
using BuildSystem.TlbGenBpl.ThroughPas;
using BuildSystem.Builder.Midl;
using BuildSystem.TlbGenDotnetDll.TlbImp;
using BuildSystem.Builder.Dotnet;
using BuildSystem.ProjectList.Common;
using BuildSystem.TestRunner.Common;
using BuildSystem.Logging;
using BuildSystem.ManagerObject.Interfaces;

/// <inheritdoc />
class BuildSpaceSettings : SettingsObject
{
    public string? config = "debug";

    ReaderJson readerJson = new ReaderJson(Build.Logger);

    /// <inheritdoc />
    public BuildSpaceSettings(ILogger logger, string[] configFiles, string variant) : base() {
        readerJson.ReadRules(configFiles);
        ReaderLocalVars = readerJson.LocalVars;
        ReaderDefines = readerJson.Defines;
        Projects = GetProjectList(configFiles);
        ProjectListProps = new ProjectListCommonProps(logger);
        config = BuildUtils.Configuration(variant);
        RegisterBSObjects();
    }

    /// <summary> Returns full path </summary>
    /// <param name="relPath"> Input relative path </param>
    public static string FPath(string relPath) => Path.GetFullPath(Path.Combine(Build.RootDirectory, relPath));

    /// <summary>
    /// Reads a list of projects from configFiles
    /// </summary>
    /// <param name="configFiles"> Json configuration file paths </param>
    private HashSet<string> GetProjectList(string[] configFiles) {
        var resultList = new HashSet<string>();
        foreach (var config in configFiles) {
            if (!File.Exists(config)) 
                continue;
            var configDir = Path.GetDirectoryName(config) + "";
            var projs = BuildUtils.GetJsonArrayValue(config, "projects");
            foreach (var jproj in projs) {
                var projPath = Path.GetFullPath(Path.Combine(configDir, jproj.ToString()));
                if (File.Exists(projPath) && !resultList.Contains(projPath))
                    resultList.Add(projPath);
            }
            
        }
        return resultList;
    }

    /// <summary>
    /// Register Build System control objects
    /// </summary>
    private void RegisterBSObjects() {
        Variants = new() {
            new() {
                Name = "Debug_x64",
                Configurations = new() { [Variant.NodeConfig] = "Debug" },
                Platforms =      new() { [Variant.NodePlatform] = "Win64", [Variant.NodePlatform + "_CSharp"] = "x64" }
            },
            new() {
                Name = "Release_x64",
                Configurations = new() { [Variant.NodeConfig] = "Release" },
                Platforms =      new() { [Variant.NodePlatform] = "Win64", [Variant.NodePlatform + "_CSharp"] = "x64" },
            }
        };

        // names in ManagerConstNames
        AddManagerProp("builder_csharp", null, builderDotNet);
        AddManagerProp("builder_idl", null, builderIdl);
        AddManagerProp("hash_generator", null, hashGeneratorCommon);
        AddManagerProp("restorer", null, restorerNuget);
        AddManagerProp("cleaner", null, cleanerCommon);
        AddManagerProp("cleaner_delphi", null, cleanerCommonDelphi);
        AddManagerProp("test_runner", null, testRunnerPropsCommon);
        AddManagerProp("project_cache", null, projectCacheCommon);
    }

    BuilderDotnetProps builderDotNet => new() { 
        Name = "builder_csharp_main" 
    };

    BuilderMsDelphiProps builderDelphiCommon => new() {
        Name = "builder_delphi_common",
        BuilderVersion = "23.0",
        MsBuilderPath = readerJson.LocalVars["msbuilder_path"],
        EnvBdsPath = readerJson.LocalVars["env_bds"],
        RsVarsPath = readerJson.LocalVars["rsvars_path"],
        AutoClean = true,
        BuildParams = new Dictionary<string, string?>
        {
            ["-verbosity"] = "normal",
            ["-consoleloggerparameters"] = "ErrorsOnly",
            ["-nologo"] = "true",
            ["/t:build"] = "true",
            // ["/p:DCC_Warnings"] = "false",
            ["/p:DCC_Hints"] = "false",
            ["/p:DCC_MapFile"] = "3",
            ["/p:DCC_AssertionsAtRuntime"] = "true",
            ["/p:DCC_IOChecking"] = "true",
            ["/p:DCC_WriteableConstants"] = "true"
        }
    };

    BuilderMsDelphiProps builderDelphiRelease {
        get {
            var bdr = new BuilderMsDelphiProps(builderDelphiCommon); // inherited
            bdr.Name = "builder_delphi_release";
            bdr.BuildParams.Add("/p:DCC_Optimize", "true");
            bdr.BuildParams.Add("/p:DCC_GenerateStackFrames", "true");
            bdr.BuildParams.Add("/p:DCC_DebugInformation", "0");
            bdr.BuildParams.Add("/p:DCC_DebugDCUs", "false");
            bdr.BuildParams.Add("/p:DCC_LocalDebugSymbols", "false");
            bdr.BuildParams.Add("/p:DCC_SymbolReferenceInfo", "0");
            bdr.BuildParams.Add("/p:DCC_IntegerOverflowCheck", "false");
            bdr.BuildParams.Add("/p:DCC_RangeChecking", "false");
            return bdr;
        }
    }

    BuilderMsDelphiProps builderDelphiDebug {
        get {
            var bdd = new BuilderMsDelphiProps(builderDelphiCommon); // inherited
            bdd.Name = "builder_delphi_debug";
            bdd.BuildParams.Add("/p:DCC_Optimize", "false");
            bdd.BuildParams.Add("/p:DCC_GenerateStackFrames", "true");
            bdd.BuildParams.Add("/p:DCC_DebugInformation", "2");
            bdd.BuildParams.Add("/p:DCC_DebugDCUs", readerJson.LocalVars.GetValueOrDefault("UseDebugDCU", "true"));
            bdd.BuildParams.Add("/p:DCC_LocalDebugSymbols", "true");
            bdd.BuildParams.Add("/p:DCC_SymbolReferenceInfo", "2");
            bdd.BuildParams.Add("/p:DCC_IntegerOverflowCheck", "true");
            bdd.BuildParams.Add("/p:DCC_RangeChecking", "true");
            return bdd;
        }
    }

    /// <summary> Builder object for delphi packages, generated from Idl project </summary>
    BuilderMsDelphiProps builderDelphiIDL {
        get {
            var bdelphi = new BuilderMsDelphiProps(builderDelphiRelease);
            bdelphi.Name = "builder_delphi_midl";
            bdelphi.BuildParams.Add("/p:DCC_DefaultNamespace", "IDL;$(DCC_DefaultNamespace)");
            bdelphi.BuildParams.Add("/p:DCC_BplOutput", FPath("../CuraEngineConnection/build/debug"));
            bdelphi.BuildParams.Add("/p:DCC_DcpOutput", FPath("../CuraEngineConnection/build/debug/dcu"));
			bdelphi.BuildParams.Add("/p:DCC_DcuOutput", FPath("../CuraEngineConnection/build/debug/dcu/$native_project:name$"));
			bdelphi.BuildParams.Add("/p:DCC_UnitSearchPath", FPath("../CuraEngineConnection/build/debug/dcu"));
            return bdelphi;
        }
    }

    TlbGenPasLibImpProps tlbGenPas => new() {
        Name = "tlb_genpas_main",
        RunAddNameSpaces = true,
        EnvBdsPath = readerJson.LocalVars["env_bds"],
        GenDoc = false
    };

    TlbGenBplThroughPasProps tlbGenBpl => new() {
        Name = "tlb_genbpl_main",
        BuilderProps = builderDelphiIDL,
        GenPasProps = tlbGenPas,
        GenDoc = false
    };

    TlbGenDotnetDllTlbImpProps tlbGenDotnetDll => new() {
        Name = "tlb_gen_dotnet_dll_main",
        GenDoc = false
    };

    BuilderMidlProps builderIdl => new() {
        BuilderVersion = "MIDL_6.00.0366;TLIBIMP_12.16581;TLDotNet_4.8.4084.0",
        Name = "builder_midl_main",
        TlbGenPasProps = tlbGenPas,
        TlbGenBplProps = tlbGenBpl,
        PropsTlbGenDotnetDll = tlbGenDotnetDll,
        SearchDirIdl =    FPath($"../CuraEngineConnection/build/{config}/idl"),
        SearchDirTlb =    FPath($"../CuraEngineConnection/build/{config}/tlb"),
        IdlOutput =       FPath($"../CuraEngineConnection/build/{config}/idl"),
        TlbOutput =       FPath($"../CuraEngineConnection/build/{config}/tlb"),
        DotnetDllOutput = FPath($"../CuraEngineConnection/build/{config}"),
        PasOutput =       FPath($"../CuraEngineConnection/build/{config}/dcu/$native_project:name$"),
        HOutput =         FPath($"../CuraEngineConnection/include/utils"),  
        PasFileName = "IDL.$native_project:name$"
    };

    TestRunnerCommonProps testRunnerPropsCommon => new() {
        Name = "test_runner_common",
        Compile = false,
        BuilderDprojProps = builderDelphiRelease
    };

    ProjectCacheCommonProps projectCacheCommon => new() {
        Name = "project_cache_main",
        TempDir = "./hash"
    };

    HashGeneratorCommonProps hashGeneratorCommon => new() {
        Name = "hash_generator_main",
        HashAlgorithmType = HashAlgorithmType.Sha256
    };

    CleanerCommonProps cleanerCommon => new() {
        Name = "cleaner_default_main",
        AllBuildResults = true
    };

    CleanerCommonProps cleanerCommonDelphi => new() {
        Name = "cleaner_delphi_main",
        AllBuildResults = true,
        Paths = new Dictionary<string, List<string>>
        {
            ["$project:output_dcu$"] = new() { "*.dcu", "*.res", "*.dfm" }
        }
    };

    RestorerNugetProps restorerNuget => new() { 
        Name = "restorer_main",
        DepsProp = new()
    };

}