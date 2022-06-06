# Painting

Game uses [PaintIn3D](https://carloswilkes.com/Documentation/PaintIn3D) for all painting related things (This also includes rust, dirt and decals). SimplePartLoader contains full PaintIn3D support since v1.3.0.

All the functions related to painting features are located on [PaintingSystem](api/paintingsystem.md) (Excluding [Part.EnablePartPainting](api/part/enablepartpainting.md))

For starting to add painting to your part you will need to do a extra bit of work, starting from your 3d model. For a very simple and limited painting support (Only paint) you only need to have a proper UV map, provide a valid material index (This material will become paintable) and that's pretty much it.

```csharp
using SimplePartLoader;

public ModMain()
{
    Part myPart = SPL.LoadPart(MyBundle, "Part");
    myPart.EnablePartPainting(PaintingSystem.Types.OnlyPaint, 2); // Now the material on the index 2 will become paintable.
}
```

Note that this way provides **very** limited support. For full PaintIn3D support you have to provide a 3d model having 3 submeshes (One for paint, other for rust and other for dirt) that properly overlaps, an UV map **on channel 0** (Will not work on other channels) that contains the data of all 3 submeshes and also manually assignate through code the materials.

```csharp
using SimplePartLoader;

public ModMain()
{
    Part myPart = SPL.LoadPart(MyBundle, "Part");
    Material[] newMaterials = new Material[3];
    newMaterials[0] = PaintingSystem.GetBodymatMaterial(); // Paint
    newMaterials[1] = PaintingSystem.GetRustMaterial(); // Rust
    newMaterials[2] = PaintingSystem.GetDirtMaterial(); // Dirt

    myPart.EnablePartPainting(PaintingSystem.Types.FullPaintingSupport); // No need to specify index here.
}
```

If you are not interested on adding all 3 meshes, you can choose between all the available options on the PaintingSystem API. This simple table will help you visualize all the available types and how many submeshes your part needs to enable painting / rust / dirt on it.

Type | Paint | Rust | Dirt
---- | ---- | ---- | -----
FullPaintingSupport | ✅ | ✅ | ✅
OnlyPaint | ☑️ | ❌ | ❌
OnlyPaintAndRust | ✅ | ✅ | ❌
OnlyDirt | ❌ | ❌ | ✅
OnlyPaintAndDirt | ✅ | ❌ | ✅

✅ - Requires submesh and specific material

❌ - Not needed

☑️ - Requires submesh but does not require specific material