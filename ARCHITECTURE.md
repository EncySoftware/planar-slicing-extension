# Architecture

## Project structure

```
planar-slicing-extension/
├── CuraConnectionInterface/   IDL definition → .NET COM interop assembly (MIDL + tlbimp)
├── CuraEngineConnection/      Native C++ DLL — socket communication with CuraEngine process
├── CuraEngineNetWrapper/      C# wrapper — loads the native DLL at runtime via LoadLibrary
├── CuraEngineParametersLibrary/ C# — parses Cura JSON parameter definitions
├── CuraEngineOperation/       C# — main ENCY extension entry point; produces the NuGet package
├── CuraEngineConnectionTest/  C# — integration test harness
├── resources/                 Pre-built native binaries tracked in git LFS (see below)
└── .stbuild/                  Nuke-based build system (EncySoftware.BuildSystem)
```

---

## Toolchains

| Component | Toolchain | Built in CI |
|---|---|---|
| `CuraConnectionInterface.dll` | MIDL + tlbimp (Windows SDK) | **Yes** — Windows SDK tools are available on `windows-latest` |
| `CuraConnectionInterface.bpl` | Delphi 23.0 | No — Delphi is not available on GitHub Actions; `.bpl` is not required for the extension |
| `CuraEngineConnection.dll` | C++ / Conan 1.x / CMake + MSVC | No — see note below |
| `CuraEngineNetWrapper.dll` | .NET 8 / MSBuild | Yes |
| `CuraEngineParametersLibrary.dll` | .NET 8 / MSBuild | Yes |
| `CuraEngineOperation.dll` | .NET 8 / MSBuild | Yes |

### CuraConnectionInterface.dll — built in CI

`CuraConnectionInterface.idl` is compiled in two steps using Windows SDK tools that ship with
`windows-latest`:

1. `midl.exe` — compiles the IDL file into a `.tlb` type library.
2. `tlbimp.exe` — generates a .NET COM interop assembly (`CuraConnectionInterface.dll`).

The Delphi `.bpl` (Delphi package for the ENCY native side) is **not built in CI**. Delphi 23.0 is
not available on GitHub Actions and the `.bpl` is not required for the NuGet extension package.
Rebuild the `.bpl` locally and deploy it separately when needed.

### Why CuraEngineConnection.dll is not built in CI

`CuraEngineConnection.dll` is a C++ shared library that uses the full CuraEngine conan recipe
(originally from Ultimaker). Its dependency graph makes automated CI builds impractical:

- Requires **Conan 1.x** (`>=1.58.0 <2.0.0`). Conan 2.x is a breaking change and cannot be used.
- Pulls in a large dependency tree: `boost/1.82.0`, `protobuf/3.21.12`, `arcus`, `clipper`,
  `openssl/3.2.0`, `spdlog`, `fmt`, `range-v3`, `zlib`, and others.
- Several packages (e.g. `clipper/6.4.2@ultimaker/stable`, `mapbox-wagyu@ultimaker/stable`,
  `standardprojectsettings@ultimaker/stable`) require a custom Conan remote to be registered.
- Without a populated Conan cache, `--build=missing` triggers full source builds of all
  dependencies. First-run build time is **30–60 minutes**.
- The resulting binary is sensitive to the exact MSVC version on the runner, which can change
  between GitHub Actions image updates.

**Decision**: `CuraEngineConnection.dll` is stored in `resources/` as a pre-built binary tracked
via **git LFS**. Developers update it manually after rebuilding locally.

---

## resources/ folder (git LFS)

Files in `resources/` are binary assets that cannot be produced by the CI toolchain.
All `.dll` files are stored through git LFS (`.gitattributes`: `*.dll filter=lfs`).

| File | Description | When to update |
|---|---|---|
| `CuraEngineConnection.dll` | Native C++ socket bridge to CuraEngine | After modifying `CuraEngineConnection/` sources and rebuilding with Conan/CMake |
| `UltimakerCuraPlugin_icon.png` | UI icon | When the icon changes |
| `UserLocalization/en-US.po` | English UI strings | When adding or changing UI text |
| `UserLocalization/ru_RU.po` | Russian UI strings | When adding or changing UI text |

`CuraConnectionInterface.dll` is **not** stored here — it is generated during the build by the IDL
step (MIDL + tlbimp) and placed in `$(OutDir)`.

---

## NuGet package: EncySoftware.CuraEngineOperation

ENCY loads the extension by reading `CuraEngineOperation.settings.json`, which points to
`${extensionJsonFolder}\CuraEngineOperation.dll`. All files must be in the **same flat directory**.

The package is structured so that the ENCY `RestorerNuget` template
(`CopyLocalLockFileAssemblies=true`) places every required file in a single output folder:

| NuGet location | Files | Mechanism |
|---|---|---|
| `lib/net8.0-windows/` | `CuraEngineOperation.dll` | Default SDK packaging |
| `contentFiles/any/any/` | Everything else (see below) | `CopyToOutputDirectory=Always` |

Files packed as `contentFiles/any/any/`:

- `CuraEngineNetWrapper.dll`, `CuraEngineParametersLibrary.dll` — from `$(OutDir)` after C# build
- `CuraConnectionInterface.dll` — from `$(OutDir)` after IDL build (MIDL + tlbimp)
- `NCalc.dll`, `Antlr4.Runtime.Standard.dll` — from `$(OutDir)` (restored by NCalcSync package)
- `CuraEngineConnection.dll` — from `resources/` (git LFS, C++ / Conan)
- `CuraEngineOperation.settings.json`, `CuraEngineToolpath_ExtOp.xml`, `CuraSettings.json`
- `UltimakerCuraPlugin_icon.png`, `UserLocalization/en-US.po`, `UserLocalization/ru_RU.po` — from `resources/`

### Zero runtime NuGet dependencies

The `.nuspec` declares **no runtime dependencies**. All required assemblies are bundled directly
in the package as `contentFiles`. This is intentional: ENCY's RestorerNuget template must be able
to deploy the extension into any flat directory without performing additional NuGet restores.

This is enforced by marking every build-time package reference as `PrivateAssets="all"`:

| Package | Project | Reason |
|---|---|---|
| `EncySoftware.CAMAPI.SDK.Net` | `CuraEngineOperation`, `CuraEngineNetWrapper` | Provided by the ENCY host process at runtime |
| `NCalcSync` | `CuraEngineParametersLibrary` | DLLs bundled as contentFiles; declaring it as a dependency would cause ENCY to resolve it separately |

**Rule**: whenever a new `PackageReference` is added to any project in this solution, it must be
marked `PrivateAssets="all"` and its output DLL must be added as a `contentFiles` entry in
`CuraEngineOperation.csproj`.

---

## Build system

The build system is **Nuke** (`Nuke.Common`) extended by **EncySoftware.BuildSystem**.
Entry point: `.stbuild/build.cmd` → `build.ps1` → bootstraps and runs `stbuild.exe`.

### Targets

| Target | Depends on | Description |
|---|---|---|
| `Restore` | `SetBuildInfo` | Restores NuGet dependencies for all C# projects |
| `Compile` | `SetBuildInfo` | Full build: IDL (MIDL + tlbimp) + C++ (Conan/CMake) + C# |
| `CompileDotnet` | `SetBuildInfo` | IDL + C# only — used by CI (no Conan required) |
| `Pack` | `Compile` | Full build + create `.nupkg` (no publish) |
| `Push` | `CompileDotnet` | IDL + C# build + create `.nupkg` + publish to NuGet feed |
| `Clean` | `SetBuildInfo` | Remove all build artifacts |

### NuGet feeds

Package discovery uses `SetStorageInfoFunc` in `buildspace.cs`:

- **Primary** — `NUGET_FEED_URL` environment variable (publish destination; `nuget.org` in prod CI)
- **Secondary** (restore only) — `https://nexus.encycam.com/repository/master/index.json`
  (public; hosts `EncySoftware.*` packages)

The stbuild bootstrap (`dotnet restore stbuild.csproj`) resolves `EncySoftware.BuildSystem` from
`nuget.org` or `nexus.encycam.com` without a `NuGet.config` because both are publicly accessible.

---

## CI pipeline

Both `publish_prod.yml` and `publish_test.yml` run on `windows-latest` and execute `--Target Push`.

```
checkout (lfs: true)          ← required to download resources/CuraEngineConnection.dll from LFS
  ↓
build.ps1 --Target Push --Variant Release_x64
  ├─ dotnet restore stbuild.csproj   (EncySoftware.BuildSystem from nexus.encycam.com)
  ├─ dotnet build stbuild.csproj
  └─ stbuild Push
       ├─ CompileDotnet
       │    ├─ midl.exe + tlbimp.exe → CuraConnectionInterface.dll  (IDL step)
       │    └─ dotnet build CuraEngineOperation.csproj              (C# step)
       │         (feeds: NUGET_FEED_URL + nexus.encycam.com via SetStorageInfoFunc)
       └─ Deploy(onlyCreate: false)
            ├─ dotnet pack   → EncySoftware.CuraEngineOperation.*.nupkg
            └─ dotnet nuget push → NUGET_FEED_URL
```

Publish to `nuget.org` requires the `NUGET_AUTH_TOKEN` secret.
Publish to `apiint.nugettest.org` requires the `NUGET_TEST_AUTH_TOKEN` secret.
