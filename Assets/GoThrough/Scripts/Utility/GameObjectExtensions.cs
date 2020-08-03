using System.Collections.Generic;
using UnityEngine;

namespace GoThrough.Utility
{
    public static class GameObjectExtensions
    {
        public static List<Material> GetMaterials(this GameObject gameObject)
        {
            var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            var materials = new List<Material>();

            foreach (var renderer in renderers)
                foreach (var mat in renderer.materials)
                    materials.Add(mat);

            return materials;
        }

        static public T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        }
    }
}