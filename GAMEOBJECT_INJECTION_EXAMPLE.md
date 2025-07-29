# GameObject Catalog Injection Example

This example shows how to use the new `CatalogGameObjectManager` to inject GameObjects directly into the catalog.

## Basic Usage

```csharp
// Example: Injecting a brake pad box GameObject into the catalog
public class ExampleMod : MonoBehaviour
{
    public void Start()
    {
        // Create or get your GameObject (e.g., brake pad box)
        GameObject brakePadBox = CreateBrakePadBox();
        
        // Ensure it has the required Partinfo component
        if (brakePadBox.GetComponent<Partinfo>() == null)
        {
            // Add Partinfo component with appropriate settings
            Partinfo partInfo = brakePadBox.AddComponent<Partinfo>();
            partInfo.price = 25.0f; // Set price
            partInfo.RenamedPrefab = "BrakePadBox";
            // Configure other properties as needed
        }
        
        // Register the GameObject for catalog injection
        try
        {
            CatalogGameObjectManager.RegisterGameObjectForCatalog(brakePadBox);
            Debug.Log("Successfully registered brake pad box for catalog injection");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to register GameObject: {ex.Message}");
        }
    }
    
    private GameObject CreateBrakePadBox()
    {
        // Your GameObject creation logic here
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "BrakePadBox";
        
        // Add CarProperties if needed for better integration
        CarProperties carProps = box.AddComponent<CarProperties>();
        carProps.PartName = "Brake Pad Box";
        carProps.PrefabName = "BrakePadBox";
        
        return box;
    }
}
```

## Important Notes

1. **Timing**: GameObjects must be registered before the `OnFirstLoad` event is called. The best place is during mod initialization or `Start()` method.

2. **Required Components**: Every GameObject must have a `Partinfo` component. The registration will fail if this component is missing.

3. **Optional Components**: Adding `CarProperties` component helps with better catalog integration and localization.

4. **Validation**: The system automatically validates all registered GameObjects before injection to ensure they still have required components.

## Error Handling

The system provides comprehensive error handling:

```csharp
// Example with error handling
try
{
    CatalogGameObjectManager.RegisterGameObjectForCatalog(myGameObject);
}
catch (ArgumentNullException ex)
{
    Debug.LogError("GameObject is null: " + ex.Message);
}
catch (ArgumentException ex)
{
    Debug.LogError("GameObject validation failed: " + ex.Message);
}
```

## Utility Methods

```csharp
// Check how many objects are registered
int count = CatalogGameObjectManager.GetRegisteredCount();

// Unregister an object if needed
bool removed = CatalogGameObjectManager.UnregisterGameObjectForCatalog(myGameObject);
```