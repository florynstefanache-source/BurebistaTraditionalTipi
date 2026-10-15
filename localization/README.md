# Traducción / Localization

`Program.cs` applies exact display-text replacements and verifies original method instructions and embedded assets. It tests generated methods with a managed localization stub; it does not run the game. Runtime helper types are embedded inside each mod, without a global patch or extra DLL dependency.

Use .NET 10 and the game’s Mono.Cecil dependency. From the repository root, with the four unmodified base DLLs in a separate INPUT directory:

```powershell
dotnet run --project localization/Patch.csproj -- --base-dir INPUT --catalog localization/translations.json --output-dir release/Mods --report-dir localization
```

Keep test-only `jit-tests` assemblies out of the game’s Mods folder. Base versions: tipi 1.2.3; rack 0.1.1; workbench 0.3.3; bed 0.1.1. The .modcomponent localization is stored in `modcomponent/localizations/Localization.json`; its recipe and asset bundles are preserved.
