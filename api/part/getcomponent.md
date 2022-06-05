# [Part](api/part.md).GetComponent

*public T GetComponent<T>()*

#### Description
A shortcut for Prefab.GetComponent<T>

#### Example
```csharp
using SimplePartLoader;

public ModMain()
{
    Part example_part = SPL.LoadPart(MyAssetBundle, "ExamplePart");

    // Both do the same!
    example_part.Prefab.GetComponent<Renderer>().material = null;
    example_part.GetComponent<Renderer>().material = null;
}
```