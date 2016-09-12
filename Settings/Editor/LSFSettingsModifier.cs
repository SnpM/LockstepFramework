using UnityEngine;
using System.Collections;
using UnityEditor;
namespace Lockstep.Data {
    public static class LSFSettingsModifier {

        internal static void Save () {
			if (EditorLSDatabaseWindow.CanSave) {
				EditorUtility.SetDirty (LSFSettingsManager.GetSettings ());
                EditorApplication.SaveAssets ();
				AssetDatabase.Refresh ();
			}
        }
    }
}