
Unity Analyzer test/dev package.

The included solution is setup to copy the dll to the package on build time

## Introduction
- Analyzers are dlls in Unity
- They apply globally if they are outside of an asmdef (can be inside a package) or locally when inside a package
- Dll settings: turn everything off (Auto Reference, Validate References, Any Platform, Editor, Standalone) and add an AssetTag named "RoslynAnalyzer" (the tags ui is not visible inside packages...)

### Examples of Analyzers usage (included in this template)

- Analyzers allow you to run fixes in your IDE:   
  ![image](https://user-images.githubusercontent.com/5083203/224544009-966f7c18-f654-4cfa-a88c-697d736b33fa.png)

- Analyzers allow you to produce logs, warnings, errors or even compiler errors
  ![image](https://user-images.githubusercontent.com/5083203/224544054-8b02e1b0-767c-48f3-a3cc-afd16b83c884.png)


## Updating the Analyzer 
- Open ``analyzers/UnityAnalyzers.sln``
- Build ``UnityAnalyzers.csproj``
- The dll is copied to the ``package`` folder on build and Unity reimports the dll. Done.

## Unity Documentation
- https://docs.unity3d.com/2021.2/Documentation/Manual/roslyn-analyzers.html 
- **NOTE**: to add the necessary AssetLabel ``RoslynAnalyzer`` the dll must be copied to the Assets folder to show the UI (and can then go to the package)   
  There are also [two more labels mentiond](https://github.com/needle-mirror/com.unity.entities/blob/2b7ad3ab445aff771ddffa3dd9d330f21fb1dd70/Unity.Entities/SourceGenerators/Source~/README.md?plain=1#L59) `SourceGenerator` and `RunOnlyOnAssembliesWithReference`  
  Alternativelly you can edit the dll meta manually to add the label:
  ```meta
    guid: ...
    labels:
    - RoslynAnalyzer
    PluginImporter:
      ...
   
  ```  
- [Debugging SourceGenerators](https://github.com/needle-mirror/com.unity.entities/blob/2b7ad3ab445aff771ddffa3dd9d330f21fb1dd70/Unity.Entities/SourceGenerators/Source~/README.md?plain=1#L25)

## Debugging


```csharp
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif 
```

When using Rider set Rider as the default JIT debugger in Settings/Debugger and clicking the `Set As Default Debugger` button
![image](https://user-images.githubusercontent.com/5083203/226258311-a702ab99-255a-48a8-9467-a0e5266ee0a3.png)


## Other Resources
- https://github.com/microsoft/Microsoft.Unity.Analyzers
- https://github.com/Cybermaxs/awesome-analyzers
