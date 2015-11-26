using UnityEngine;
using System.Collections;
using UnityEditor;
namespace Lockstep {
    public static class LSFSettingsModifier {

        internal static void Save () {
            EditorUtility.SetDirty (LSFSettingsManager.GetSettings ());
            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh ();
        }
    }
}