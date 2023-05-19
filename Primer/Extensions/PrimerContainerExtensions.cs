using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class PrimerContainerExtensions
    {
        public static void DisableChildren<T>(this Container<T> container) where T : Component {
            foreach (var child in container.transform.GetChildren()) {
                child.SetScale(0);
                child.SetActive(false);
            }
        }

        private static readonly Dictionary<string, Component> cachedPrefabs = new();

        public static T AddPrefab<T>(this Container container, string prefabName, string name,
            bool worldPositionStays = false)
            where T : Component
        {
            return container.AddPrefab<Transform, T>(prefabName, name, worldPositionStays);
        }

        public static TResult AddPrefab<TContainer, TResult>(this Container<TContainer> container, string prefabName,
            string name, bool worldPositionStays = false)
            where TContainer : Component
            where TResult : Component
        {
            var prefab = GetPrefabByName<TResult>(prefabName);
            return container.Add(prefab, name, worldPositionStays);
        }

        private static T GetPrefabByName<T>(string prefabName) where T : Component
        {
            if (cachedPrefabs.ContainsKey(prefabName)) {
                return cachedPrefabs[prefabName] as T;
            }

            var prefab = Resources.Load<T>(prefabName);
            cachedPrefabs.Add(prefabName, prefab);
            return prefab;
        }
    }
}
