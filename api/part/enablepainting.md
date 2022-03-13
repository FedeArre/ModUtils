# [Part](api/part.md).EnablePainting

*public void EnablePainting(int materialIndex, string slotTextureType = "_MainTex")*

#### Description
Makes a part paintable given the material index that will be converted to a paintable one.

#### Parameters
Name | Description | Type | Default
---- | ---- | ----  | ---- 
materialIndex | The index of the material that will be paintable | int | None
slotTextureType | The texture type of the material slot. Will be removed on the future with proper PaintIn3D support | string | _MainTex

#### Example
```csharp
using SimplePartLoader;

public ModMain()
{
    Part example_part = SPL.LoadPart(MyAssetBundle, "ExamplePart");
    example_part.MakePartPaintable(0);
}
```