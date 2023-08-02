using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public partial class Gnome
    {
        public bool disableWhenDisposed = true;
        private readonly List<Action> onCleanup = new();
        private readonly List<Action<bool>> onPurge = new();

        public void RemoveAllChildren(bool defer = false)
        {
            Reset();
            Purge(defer);
        }

        public void Purge(bool defer = false)
        {
            foreach (var child in unusedChildren)
                OnRemove(child, defer);

            foreach (var listener in onPurge)
                listener(defer);

            unusedChildren.Clear();
        }

        public void OnCleanup(Action action)
        {
            onCleanup.Add(action);
        }

        public void Dispose()
        {
            if (disableWhenDisposed)
                transform.SetActive(false);

            foreach (var broomUp in onCleanup)
                broomUp();
        }

        internal void RegisterChildContainer(Gnome gnome)
        {
            onPurge.Add(gnome.Purge);
        }

        public static void Dispose(Component component)
        {
            if (component != null)
                new Gnome(component, setActive: false).Dispose();
        }
    }
}
