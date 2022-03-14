# [SPL](api/spl.md).CopyPartToPrefab

*public static void CopyPartToPrefab(Part p, string partName, bool p3dSupport = true, bool ignoreBuiltin = false, bool doNotCopyChilds = false)*

#### Description
Copies a part from the game into an empty prefab that has been loaded through [LoadDummy](api/spl/loaddummy.md).

#### Parameters
Name | Description | Type | Default
---- | ---- | ----  | ---- 
p | The part that will receive the copy | Part | None
partName | The name of the part that will be copied. Has to be the name of the GameObject's name of the prefab | string | None
p3dSupport | Enable experimental PaintIn3D support | bool | true
ignoreBuiltin | Disable copy of Unity built-in components (Collider, Renderer, MeshFilter) | bool | false
doNotCopyChilds | Disable copy of the childs of the original object to the new one | bool | false

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