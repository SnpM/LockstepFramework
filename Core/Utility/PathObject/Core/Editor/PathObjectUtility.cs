using UnityEngine;
using System.Collections; using FastCollections;
using UnityEditor;

namespace Lockstep
{
    public static class PathObjectUtility
    {
        public static bool TryGetPath(UnityEngine.Object obj, out string output)
        {
            string path = AssetDatabase.GetAssetPath(obj); 
            int relativeRootIndex = path.IndexOf("Resources");
            if (relativeRootIndex < 0)
            {
                output = "";
                return false;
            }
            string relativePath = path.Substring(relativeRootIndex + "Resources/".Length);
            relativePath = relativePath.Remove(relativePath.IndexOf(".prefab"));
            output = relativePath;
            return true;
        }
    }
}