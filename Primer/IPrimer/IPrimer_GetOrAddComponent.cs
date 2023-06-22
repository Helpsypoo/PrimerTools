using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_GetOrAddComponentExtensions
    {
        public static IEnumerable<T> GetOrAddComponent<T>(this IEnumerable<IPrimer> self) where T : Component
        {
            return self.Select(x => x.transform.gameObject.GetOrAddComponent<T>());
        }

        public static IEnumerable<T> GetOrAddComponent<T>(this IEnumerable<Component> self) where T : Component
        {
            return self.Select(x => x.gameObject.GetOrAddComponent<T>());
        }

        public static T GetOrAddComponent<T>(this IPrimer self) where T : Component
        {
            return self as T ?? self.transform.gameObject.GetOrAddComponent<T>();
        }

        public static T GetOrAddComponent<T>(this IPrimer self, ref T cache) where T : Component
        {
            if (cache == null)
                cache = self as T ?? self.transform.gameObject.GetOrAddComponent<T>();

            return cache;
        }

        public static T GetOrAddComponent<T>(this Component self) where T : Component
        {
            return self as T ?? self.gameObject.GetOrAddComponent<T>();
        }

        public static T GetOrAddComponent<T>(this Component self, ref T cache) where T : Component
        {
            if (cache == null)
                cache = self as T ?? self.gameObject.GetOrAddComponent<T>();

            return cache;
        }

        // Actual implementation
        // All overloads redirect here
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var found = gameObject.GetComponent<T>();
            return found == null ? gameObject.AddComponent<T>() : found;
        }
    }
}
