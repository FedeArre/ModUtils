# [SPL](api/spl.md).ForcePartRegister

*public static void ForcePartRegister(Part p, bool addToCatalog = false)*

#### Description
Forces the register of a part into the interal car parts list. Used for parts that do not exist on both gamemodes.

#### Parameters
Name | Description | Type | Default
---- | ---- | ----  | ---- 
p | The part that will be registered | Part | None
addToCatalog | If the part has to be added to the catalog | bool | false

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