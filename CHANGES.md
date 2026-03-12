# Changes: self-contained NuGet package and CI pipeline

## Goal

Produce a single `EncySoftware.CuraEngineOperation` NuGet package that, when deployed to a
directory, contains everything ENCY needs to load the extension — no manual file copying required.
Fix the broken CI `Push` target at the same time.

---

## 1. `resources/` folder (new)

Native binaries and localisation files that are not built by the C# toolchain are now stored under
`resources/` and tracked in git via LFS (`.gitattributes` already declares `*.dll filter=lfs`).

| File | Source |
|---|---|
| `resources/CuraConnectionInterface.dll` | `CuraEngineConnection/build/Debug/` |
| `resources/CuraEngineConnection.dll` | `CuraEngineConnection/build/test/` |
| `resources/UltimakerCuraPlugin_icon.png` | `CuraEngineConnection/build/test/` |
| `resources/UserLocalization/en-US.po` | `CuraEngineConnection/build/Debug/UserLocalization/` |
| `resources/UserLocalization/ru_RU.po` | `CuraEngineConnection/build/test/UserLocalization/` |

**TODO**: whenever native components are rebuilt, copy the new binaries into `resources/` and commit
them through LFS before packing/publishing.

---

## 2. `CuraEngineNetWrapper/CuraEngineNetWrapper.csproj`

- **Reference to `CuraConnectionInterface.dll`** changed from `$(OutDir)\CuraConnectionInterface.dll`
  (build output, requires Delphi toolchain) to `..\resources\CuraConnectionInterface.dll`
  (git-tracked). `Private=false` prevents MSBuild from re-copying it to `OutDir`.
- **`EncySoftware.CAMAPI.SDK.Net`** marked `PrivateAssets="all"`: CAMAPI assemblies are provided by
  the ENCY host process and must not appear in the package or be copied to the output folder.

---

## 3. `CuraEngineOperation/CuraEngineOperation.csproj`

### Compile-time section
- Duplicate `PackageReference` to `EncySoftware.CAMAPI.SDK.Net` removed (was present in two
  `ItemGroup` elements).
- `EncySoftware.CAMAPI.SDK.Net` marked `PrivateAssets="all"` (same reason as above).
- Reference to `CuraConnectionInterface.dll` changed to `resources/` with `Private=false`.

### NuGet package contents (all new `contentFiles/any/any/`)
Every file listed below is packed with `CopyToOutputDirectory=Always` so that consumers using
`CopyLocalLockFileAssemblies=true` (the ENCY `RestorerNuget` template) receive all files in a flat
output directory alongside `CuraEngineOperation.dll`.

| Group | Files |
|---|---|
| Extension descriptors | `CuraEngineToolpath_ExtOp.xml`, `CuraEngineOperation.settings.json`, `CuraSettings.json` |
| Managed deps (from `$(OutDir)`) | `CuraEngineNetWrapper.dll`, `CuraEngineParametersLibrary.dll`, `NCalc.dll`, `Antlr4.Runtime.Standard.dll` |
| Native DLLs (from `resources/`) | `CuraConnectionInterface.dll`, `CuraEngineConnection.dll` |
| Resources (from `resources/`) | `UltimakerCuraPlugin_icon.png`, `UserLocalization/en-US.po`, `UserLocalization/ru_RU.po` |

`CuraEngineOperation.dll` itself stays in `lib/net8.0-windows/` (default SDK behaviour) and is also
copied to the output folder via `CopyLocalLockFileAssemblies=true`.

---

## 4. `.stbuild/build/build.cs`

### Removed
- `Deploy` target — it called `BSpace.Projects.Deploy(Variant, true, ...)` which only created the
  package (`onlyCreate=true`) but never pushed it, and the project filter
  `project => project.Scope.Contains("main")` matched nothing because no project JSON defines a
  `scope` field.

### Added

**`CompileCSharp`** (private helper)
Compiles only projects whose `Type` equals `"CSharp"`. Used by CI where the Delphi IDL toolchain
and Conan/CMake are not available. Native DLLs come from `resources/`.

**`Pack`**
Depends on `Compile` (full build: IDL + Conan + C#). Calls
`BSpace.Projects.Deploy(Variant, onlyCreate: true, _ => true)` — creates the `.nupkg` locally
without publishing. Intended for local development and pre-publish verification.

**`Push`**
Depends on `CompileCSharp` (C# only). Calls
`BSpace.Projects.Deploy(Variant, onlyCreate: false, _ => true)` — creates and publishes the
package. Intended for CI. Requires `NUGET_FEED_URL` and `NUGET_AUTH_TOKEN` environment variables.

> `IProjectList.Deploy(variantName, onlyCreate, filter)` — `onlyCreate=false` means pack + push,
> `onlyCreate=true` means pack only (source: `BuildSystem.ProjectList.xml` doc comment).

---

## 5. `commands/build.cmd` and `commands/pack.cmd`

Variant strings corrected from `Debug` / `Release` to `Debug_x64` / `Release_x64`.
`BuildUtils.Configuration(variant)` calls `variant[..variant.IndexOf('_')]` which throws
`ArgumentOutOfRangeException` when the underscore is absent.

---

## 6. `.github/workflows/publish_prod.yml` and `publish_test.yml`

- `--Variant Release` → `--Variant Release_x64` (same fix as above).
- `--Target Push` was already correct in intent; the target now exists in `build.cs`.

---

## CI execution flow (after these changes)

```
build.ps1
  dotnet restore stbuild.csproj     # EncySoftware.BuildSystem from nuget.org or nexus.encycam.com
  dotnet build stbuild.csproj
  dotnet run stbuild -- Push Release_x64
    CompileCSharp
      BSpace.Projects.Compile (CSharp filter only)
        dotnet build CuraEngineOperation.csproj
          # feeds provided by SetStorageInfoFunc in buildspace.cs:
          #   NUGET_FEED_URL (nuget.org for prod)
          #   nexus.encycam.com/repository/master/index.json (for EncySoftware.* packages)
    Deploy(onlyCreate: false)
      dotnet pack CuraEngineOperation.csproj -c Release
        # lib/net8.0-windows/CuraEngineOperation.dll
        # contentFiles: all managed + native DLLs, config files, localisation
      dotnet nuget push *.nupkg --source NUGET_FEED_URL
```

## Potential issues to check

1. **`$(OutDir)` DLLs at pack time** — `NCalc.dll` and `Antlr4.Runtime.Standard.dll` are restored
   by `dotnet build` into `OutDir`. Verify they are present there after `CompileCSharp` finishes
   and before `dotnet pack` runs. If stbuild separates restore and build phases this may require
   an explicit restore step.

2. **`CuraEngineNetWrapper.dll` in contentFiles vs lib** — the `ProjectReference` in
   `CuraEngineOperation.csproj` may cause the SDK to also place `CuraEngineNetWrapper.dll` in
   `lib/`. If so, it appears twice in the restored output (harmless, but worth verifying with
   `dotnet pack --verbosity detailed`).

3. **LFS on CI** — `actions/checkout@v4` does not fetch LFS objects by default. Add
   `lfs: true` to the checkout step if `resources/*.dll` are not being included in the package:
   ```yaml
   - uses: actions/checkout@v4
     with:
       lfs: true
   ```
