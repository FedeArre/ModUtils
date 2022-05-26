# [SPL](api/spl.md).FirstLoad

*public static event FirstLoadDelegate FirstLoad*

#### Description
Event called when the game loads for first time, used for dummy parts.

#### Example
```csharp
using SimplePartLoader;

Part example_part;

public ModMain()
{
    example_part = SPL.LoadDummy(MyAssetBundle, "ExamplePart");

    SPL.FirstLoad += OnFirstLoadCall;
}

public void OnFirstLoadCall()
{
    // Most common usage - Copies the Frontpanel08 (Niv's front panel) into our example_part
    SPL.CopyPartToPrefab(example_part, "Frontpanel08");
    
    // After the copy, we can already interact with our new object.
    example_part.CarProps.PartName = "Awesome front-panel"; // Renaming it to this in-game. SPL will localize the parts to all languages, so don't worry!

    // You can still localize it if you want.
    example_part.Localize("Polish", "Not awesome front-panel");
}
```