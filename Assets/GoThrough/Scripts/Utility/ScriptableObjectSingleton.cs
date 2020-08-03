using System.Linq;
using UnityEngine;

namespace GoThrough.Utility
{

    /// <summary>
    /// Abstract class for making reload-proof singletons out of ScriptableObjects
    /// Returns the asset created on the editor, or null if there is none
    /// Based on https://www.youtube.com/watch?v=VBA1QCoEAX4
    /// </summary>
    /// <typeparam name="T">Singleton type</typeparam>

    public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T instance = null;

        public static T Instance
        {
            get
            {
                if (!instance)
                {
                    var assets = Resources.LoadAll<T>("");
                    
                    if (assets.Length > 1) 
                        Debug.LogError("Found multiple " + typeof(T).Name + "s on the resources folder. It is a Singleton ScriptableObject, there should only be one.");

                    if (assets.Length == 0)
                    {
                        instance = CreateInstance<T>();
                        Debug.LogError("Could not find a " + typeof(T).Name + " on the resources folder. It was created at runtime, therefore it will not be visible on the assets folder and it will not persist.");
                    }
                    else 
                        instance = assets[0];
                }

                return instance;
            }
        }
    }
}