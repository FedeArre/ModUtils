# [SPL](api/spl.md).LoadDummy

*public static Part LoadPart(AssetBundle bundle, string prefabName)*

#### Description
Creates a new [Part](api/part.md) given a bundle and a prefab. The prefab should be an empty GameObject that will be later used on [CopyPartToPrefab](api/spl/copyparttoprefab.md) unless it has a Prefab Generator component (Part created on editor, [guide here](guides/prefab_generator.md))

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
    Part example_part = SPL.LoadDummy(MyAssetBundle, "ExamplePart");
}
```