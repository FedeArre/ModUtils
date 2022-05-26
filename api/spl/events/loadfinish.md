# [SPL](api/spl.md).LoadFinish

*public static event LoadFinishDelegate LoadFinish*

#### Description
Event called when the SimplePartLoader's parts loading code fully finished loading the parts.

#### Example
```csharp
using SimplePartLoader;

Part example_part;
bool hasPartLoaded;

public ModMain()
{
    example_part = SPL.LoadDummy(MyAssetBundle, "ExamplePart");

    SPL.LoadFinish += OnLoadFinish;
}

public void OnLoadFinish()
{
    if(!SPL.GetPlayerTools().MapMagic && !hasPartLoaded) // Check if player is not on survival mode
    {
        // We first need to copy the object to our part.
        SPL.CopyPartToPrefab(example_part, "HubCap06");

        // Then we need to manually register it into SPL (For saving and loading, localization and more)
        SPL.ForcePartRegister(example_part);

        hasPartLoaded = true; // We only need to do this once.

        // This introduces a small issue on where a part that does not exist in survival can exist by just loading first default and then survival. Remember that!
    }
}
```