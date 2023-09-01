﻿using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Primer
{
    // This part contains the methods to define new children
    public partial class SimpleGnome
    {
        public Transform Add(string name)
        {
            var child = FindChild<Transform>(name) ?? new GameObject(name).transform;
            child.SetParent(transform);
            child.gameObject.SetActive(true);
            return child;
        }

        public TChild Add<TChild>(string name)
            where TChild : Component
        {
            var child = FindChild<TChild>(name) ??  new GameObject(name).AddComponent<TChild>();
            child.transform.SetParent(transform);
            child.gameObject.SetActive(true);
            return child;
        }

        public TChild Add<TChild>(GameObject prefab, string name)
            where TChild : Component
        {
            // This simply doesn't use the prefab if an object already exists
            // with the correct name and that has the correct component.
            // It doesn't track whether the object is actually in instance of the prefab.
            // But like, that mistake should be rare, obvious,
            // and easy to fix by just deleting the object and letting it be re-created.
            
            // This also doesn't a TChild component to be present on the prefab.
            // Which is nice. Since you might want to add Creature to a PrimerBlob, for example.
            var child = FindChild<TChild>(name) ?? Object.Instantiate(prefab, transform).GetOrAddComponent<TChild>();
            child.name = name;
            child.transform.SetParent(transform);
            child.gameObject.SetActive(true);
            return child;
        }
        
        public TChild Add<TChild>(string prefabName, string name)
            where TChild : Component
        {
            return Add<TChild>(Resources.Load<GameObject>(prefabName), name);
        }

        private TChild FindChild<TChild>(string childName) where TChild : Component
        {
            var hasName = !string.IsNullOrWhiteSpace(childName);
            var isTransform = typeof(TChild) == typeof(Transform);

            for (var i = 0; i < transform.GetChildren().Length; i++) {
                var child = transform.GetChild(i);

                if (!child || hasName && child.name != childName)
                    continue;

                if (isTransform)
                    return child as TChild;

                if (child.TryGetComponent<TChild>(out var childComponent))
                    return childComponent;
                
                Debug.LogError("Found child with name " + childName + " but it doesn't have the component " + typeof(TChild).Name + " attached.");
            }

            return null;
        }
    }
}