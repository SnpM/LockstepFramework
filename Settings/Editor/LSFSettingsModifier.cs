using UnityEngine;
using UnityEditor;

namespace Lockstep.Data
{
	public static class LSFSettingsModifier
	{

		internal static void Save()
		{
			if (Application.isPlaying)
				return;
			if (EditorLSDatabaseWindow.CanSave)
			{
				EditorUtility.SetDirty(LSFSettingsManager.GetSettings());

#if UNITY_5_5_OR_NEWER
				AssetDatabase.SaveAssets();
#else
				EditorApplication.SaveAssets ();
#endif
				AssetDatabase.Refresh();
			}
		}
	}
}