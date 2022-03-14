# [SPL](api/spl.md).LoadPart

*public static Part LoadPart(AssetBundle bundle, string prefabName)*

#### Description
Creates a new [Part](api/part.md) given a bundle and a valid prefab.

#### Parameters
Name | Description | Type | Default
---- | ---- | ----  | ---- 
bundle | The AssetBundle that contains the prefab | AssetBundle | None
prefabName | The name of the prefab in the AssetBundle | string | None

#### Example
```csharp
using SimplePartLoader;

public ModMain()
{
    Part example_part = SPL.LoadPart(MyAssetBundle, "ExamplePart");
    
    // The prefab requires a CarProperties and a Partinfo component.
    // You also need to use a mesh collider. Game does not support other types of colliders.
    // SPL will not check if you are missing the collider!
    // Also do not add a Rigidbody to the part or it will mess with saving & loading!
}
```