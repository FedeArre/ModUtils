using UnityEngine;
using SimplePartLoader;
using System;

/// <summary>
/// Example validation script demonstrating the CatalogGameObjectManager functionality.
/// This would typically be used by mod developers to test the catalog injection feature.
/// </summary>
public class CatalogGameObjectManagerValidation : MonoBehaviour
{
    public void Start()
    {
        RunValidationTests();
    }

    private void RunValidationTests()
    {
        Debug.Log("=== CatalogGameObjectManager Validation Tests ===");

        TestBasicRegistration();
        TestValidationFailures();
        TestDuplicateRegistration();
        TestUnregistration();
        TestCount();

        Debug.Log("=== Validation Tests Complete ===");
    }

    private void TestBasicRegistration()
    {
        Debug.Log("Testing basic registration...");
        
        // Create a valid GameObject with Partinfo
        GameObject testObject = CreateValidGameObject("TestObject1");
        
        try
        {
            CatalogGameObjectManager.RegisterGameObjectForCatalog(testObject);
            Debug.Log("✓ Basic registration successful");
            
            int count = CatalogGameObjectManager.GetRegisteredCount();
            Debug.Log($"✓ Registration count: {count}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Basic registration failed: {ex.Message}");
        }
    }

    private void TestValidationFailures()
    {
        Debug.Log("Testing validation failures...");
        
        // Test null GameObject
        try
        {
            CatalogGameObjectManager.RegisterGameObjectForCatalog(null);
            Debug.LogError("✗ Null GameObject validation should have failed");
        }
        catch (ArgumentNullException)
        {
            Debug.Log("✓ Null GameObject validation working correctly");
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Unexpected exception for null GameObject: {ex.Message}");
        }

        // Test GameObject without Partinfo
        GameObject invalidObject = new GameObject("InvalidObject");
        try
        {
            CatalogGameObjectManager.RegisterGameObjectForCatalog(invalidObject);
            Debug.LogError("✗ GameObject without Partinfo validation should have failed");
        }
        catch (ArgumentException)
        {
            Debug.Log("✓ Missing Partinfo validation working correctly");
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Unexpected exception for missing Partinfo: {ex.Message}");
        }
        
        // Clean up
        DestroyImmediate(invalidObject);
    }

    private void TestDuplicateRegistration()
    {
        Debug.Log("Testing duplicate registration...");
        
        GameObject testObject = CreateValidGameObject("TestObject2");
        
        try
        {
            // Register once
            CatalogGameObjectManager.RegisterGameObjectForCatalog(testObject);
            int countBefore = CatalogGameObjectManager.GetRegisteredCount();
            
            // Try to register again
            CatalogGameObjectManager.RegisterGameObjectForCatalog(testObject);
            int countAfter = CatalogGameObjectManager.GetRegisteredCount();
            
            if (countBefore == countAfter)
            {
                Debug.Log("✓ Duplicate registration prevention working correctly");
            }
            else
            {
                Debug.LogError("✗ Duplicate registration prevention failed");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Duplicate registration test failed: {ex.Message}");
        }
    }

    private void TestUnregistration()
    {
        Debug.Log("Testing unregistration...");
        
        GameObject testObject = CreateValidGameObject("TestObject3");
        
        try
        {
            // Register object
            CatalogGameObjectManager.RegisterGameObjectForCatalog(testObject);
            int countBefore = CatalogGameObjectManager.GetRegisteredCount();
            
            // Unregister object
            bool removed = CatalogGameObjectManager.UnregisterGameObjectForCatalog(testObject);
            int countAfter = CatalogGameObjectManager.GetRegisteredCount();
            
            if (removed && countAfter == countBefore - 1)
            {
                Debug.Log("✓ Unregistration working correctly");
            }
            else
            {
                Debug.LogError("✗ Unregistration failed");
            }
            
            // Test unregistering non-existent object
            bool removedAgain = CatalogGameObjectManager.UnregisterGameObjectForCatalog(testObject);
            if (!removedAgain)
            {
                Debug.Log("✓ Unregistering non-existent object handled correctly");
            }
            else
            {
                Debug.LogError("✗ Unregistering non-existent object should return false");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Unregistration test failed: {ex.Message}");
        }
    }

    private void TestCount()
    {
        Debug.Log("Testing count functionality...");
        
        int initialCount = CatalogGameObjectManager.GetRegisteredCount();
        Debug.Log($"Initial count: {initialCount}");
        
        GameObject testObject1 = CreateValidGameObject("CountTest1");
        GameObject testObject2 = CreateValidGameObject("CountTest2");
        
        try
        {
            CatalogGameObjectManager.RegisterGameObjectForCatalog(testObject1);
            int countAfterFirst = CatalogGameObjectManager.GetRegisteredCount();
            
            CatalogGameObjectManager.RegisterGameObjectForCatalog(testObject2);
            int countAfterSecond = CatalogGameObjectManager.GetRegisteredCount();
            
            if (countAfterFirst == initialCount + 1 && countAfterSecond == initialCount + 2)
            {
                Debug.Log("✓ Count functionality working correctly");
            }
            else
            {
                Debug.LogError($"✗ Count functionality failed. Expected {initialCount + 1}, {initialCount + 2}, got {countAfterFirst}, {countAfterSecond}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"✗ Count test failed: {ex.Message}");
        }
    }

    private GameObject CreateValidGameObject(string name)
    {
        GameObject obj = new GameObject(name);
        
        // Add required Partinfo component
        Partinfo partInfo = obj.AddComponent<Partinfo>();
        partInfo.price = 25.0f;
        partInfo.RenamedPrefab = name;
        
        // Add optional CarProperties for better integration
        CarProperties carProps = obj.AddComponent<CarProperties>();
        carProps.PartName = name + " Part";
        carProps.PrefabName = name;
        
        return obj;
    }

    private void OnDestroy()
    {
        // Clean up any remaining test objects when this script is destroyed
        Debug.Log("Cleaning up validation test objects...");
    }
}