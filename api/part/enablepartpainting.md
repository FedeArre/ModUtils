# [Part](api/part.md).EnablePartPainting

*public void EnablePartPainting(int materialIndex, string slotTextureType = "_MainTex")*

#### Description
Makes a part paintable if the part already has the proper materials for it (Unless the paint only type)

#### Parameters
Name | Description | Type | Default
---- | ---- | ----  | ---- 
type | The type of painting that the part will have | [PaintingSystem.Types](api/paintingsystem/types.md) | None
paintMaterial | The index of the material that will become paintable, only required if using only paint type | int | -1

#### Example
```csharp
using SimplePartLoader;

public ModMain()
{
    Part example_part = SPL.LoadPart(MyAssetBundle, "ExamplePart");
    Renderer partRenderer = example_part.Prefab.GetComponent<Renderer>();

    Material[] partMaterials = partRenderer.materials;
    partMaterials[0] = PaintingSystem.GetPaintRustMaterial();
    partMaterials[1] = PaintingSystem.GetDirtMaterial();
    partMaterials[2] = PaintingSystem.GetBodymatMaterial();
    partRenderer.materials = partMaterials; // Has to be assigned back since partRenderer.materials returns a copy of the materials array, not a reference.

    example_part.MakePartPaintable(PaintingSystem.Types.FullPaintingSupport);
}
```