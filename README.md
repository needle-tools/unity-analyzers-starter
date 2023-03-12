
Unity Analyzer test/dev package.

The included solution is setup to copy the dll to the package on build time

## Introduction
- Analyzers are dlls in Unity
- They apply globally if they are outside of an asmdef (can be inside a package) or locally when inside a package
- Dll settings: turn everything off (Auto Reference, Validate References, Any Platform, Editor, Standalone) and add an AssetTag named "RoslynAnalyzer" (the tags ui is not visible inside packages...)

## Updating the Analyzer 
- Open ``analyzers/UnityAnalyzers.sln``
- Build ``UnityAnalyzers.csproj``
- The dll is copied to the ``package`` folder on build and Unity reimports the dll. Done.

## Unity Documentation
- https://docs.unity3d.com/2021.2/Documentation/Manual/roslyn-analyzers.html 
- NOTE: to add the necessary AssetLabel ``RoslynAnalyzer`` the dll must be copied to the Assets folder to show the UI (and can then go to the package)

## Other Resources
- https://github.com/microsoft/Microsoft.Unity.Analyzers
- https://github.com/Cybermaxs/awesome-analyzers
