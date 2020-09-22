using System.Collections.Generic;
using UnityEngine;

namespace GoThrough.Utility
{
    /// <summary>
    /// Some helper methods for UnityEngine.GameObject class.
    /// </summary>
    internal static class GameObjectExtensions
    {
        /// <summary>
        /// Get a list of all materials curretly attached to <paramref name="gameObject"/> or any of it's children.
        /// </summary>
        /// <param name="gameObject">The GameObject.</param>
        /// <returns>A list of materials attached to <paramref name="gameObject"/> or any of it's children.</returns>
        internal static List<Material> GetMaterials(this GameObject gameObject)
        {
            var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            var materials = new List<Material>();

            foreach (var renderer in renderers)
                foreach (var mat in renderer.materials)
                    materials.Add(mat);

            return materials;
        }

        /// <summary>
        /// Get's component of type <typeparamref name="T"/> if it exists and adds one if it doesn't.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="gameObject">The GameObject.</param>
        /// <returns>The existing or created Component.</returns>
        static internal T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        }
    }
}