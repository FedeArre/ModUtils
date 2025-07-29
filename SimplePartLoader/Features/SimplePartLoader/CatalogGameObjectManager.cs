using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePartLoader
{
    /// <summary>
    /// Manages direct injection of GameObjects into the catalog system.
    /// This allows objects that are not traditional parts (e.g., brake pad boxes) to be sold in the catalog.
    /// </summary>
    public static class CatalogGameObjectManager
    {
        /// <summary>
        /// List of GameObjects that should be injected directly into the catalog.
        /// These objects will be added during the OnLoad phase before OnFirstLoad is called.
        /// </summary>
        private static List<GameObject> gameObjectsToInject = new List<GameObject>();

        /// <summary>
        /// Registers a GameObject to be injected directly into the catalog.
        /// The GameObject must have a Partinfo component for proper catalog integration.
        /// </summary>
        /// <param name="gameObject">The GameObject to inject into the catalog</param>
        /// <exception cref="ArgumentNullException">Thrown when gameObject is null</exception>
        /// <exception cref="ArgumentException">Thrown when gameObject lacks required Partinfo component</exception>
        public static void RegisterGameObjectForCatalog(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject), "GameObject cannot be null");
            }

            // Validate that the GameObject has the required Partinfo component
            Partinfo partInfo = gameObject.GetComponent<Partinfo>();
            if (partInfo == null)
            {
                throw new ArgumentException($"GameObject '{gameObject.name}' must have a Partinfo component to be injected into catalog", nameof(gameObject));
            }

            // Check if already registered to avoid duplicates
            if (gameObjectsToInject.Contains(gameObject))
            {
                CustomLogger.AddLine("CatalogGameObjectManager", $"GameObject '{gameObject.name}' is already registered for catalog injection");
                return;
            }

            gameObjectsToInject.Add(gameObject);
            CustomLogger.AddLine("CatalogGameObjectManager", $"Registered GameObject '{gameObject.name}' for catalog injection");
        }

        /// <summary>
        /// Removes a GameObject from the injection list.
        /// </summary>
        /// <param name="gameObject">The GameObject to remove from injection list</param>
        /// <returns>True if the GameObject was removed, false if it wasn't in the list</returns>
        public static bool UnregisterGameObjectForCatalog(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return false;
            }

            bool removed = gameObjectsToInject.Remove(gameObject);
            if (removed)
            {
                CustomLogger.AddLine("CatalogGameObjectManager", $"Unregistered GameObject '{gameObject.name}' from catalog injection");
            }

            return removed;
        }

        /// <summary>
        /// Gets all GameObjects currently registered for catalog injection.
        /// This is called internally by PartManager during the catalog setup phase.
        /// </summary>
        /// <returns>Read-only list of GameObjects to inject into catalog</returns>
        internal static IReadOnlyList<GameObject> GetGameObjectsToInject()
        {
            return gameObjectsToInject.AsReadOnly();
        }

        /// <summary>
        /// Clears all registered GameObjects from the injection list.
        /// This should only be used for cleanup or reset scenarios.
        /// </summary>
        internal static void ClearAll()
        {
            int count = gameObjectsToInject.Count;
            gameObjectsToInject.Clear();
            
            if (count > 0)
            {
                CustomLogger.AddLine("CatalogGameObjectManager", $"Cleared {count} GameObjects from catalog injection list");
            }
        }

        /// <summary>
        /// Gets the current count of registered GameObjects.
        /// </summary>
        /// <returns>Number of GameObjects registered for injection</returns>
        public static int GetRegisteredCount()
        {
            return gameObjectsToInject.Count;
        }

        /// <summary>
        /// Validates that all registered GameObjects still have required components.
        /// This is called internally before injection to ensure data integrity.
        /// </summary>
        /// <returns>True if all GameObjects are valid, false otherwise</returns>
        internal static bool ValidateAllGameObjects()
        {
            bool allValid = true;
            List<GameObject> invalidObjects = new List<GameObject>();

            foreach (GameObject gameObject in gameObjectsToInject)
            {
                if (gameObject == null)
                {
                    invalidObjects.Add(gameObject);
                    allValid = false;
                    CustomLogger.AddLine("CatalogGameObjectManager", "Found null GameObject in injection list");
                    continue;
                }

                if (gameObject.GetComponent<Partinfo>() == null)
                {
                    invalidObjects.Add(gameObject);
                    allValid = false;
                    CustomLogger.AddLine("CatalogGameObjectManager", $"GameObject '{gameObject.name}' missing Partinfo component");
                }
            }

            // Remove invalid objects
            foreach (GameObject invalidObject in invalidObjects)
            {
                gameObjectsToInject.Remove(invalidObject);
            }

            return allValid;
        }
    }
}