using UnityEngine;
using System.Collections;
using UnityEditor;
using Lockstep.Legacy;
using System.Collections.Generic;
using System;
namespace Lockstep.Legacy.Integration
{
	[CustomEditor (typeof(Legacy.LSBody), true),UnityEditor.CanEditMultipleObjects]
	public class EditorLSBody : Editor
	{
		static int boom = 0;

		public override void OnInspectorGUI ()
		{
			boom--;
			if (GUILayout.Button ("Update LSBody")) {
				if (boom <= 0) {
					boom = 10;
					ReplaceLegacy ();
				}
			}
		}

		void ReplaceLegacy ()
		{
			//special thanks to hpjohn <3
			//http://forum.unity3d.com/threads/editor-want-to-check-all-prefabs-in-a-project-for-an-attached-monobehaviour.253149/
			string[] allPrefabs = GetAllPrefabs ();
			List<string> listResult = new List<string> ();
			MonoScript targetScript = null;

			foreach (var monoScript in Resources.FindObjectsOfTypeAll<MonoScript>()) {
				if (monoScript.GetClass () == typeof(LSBody)) {
					targetScript = monoScript;
				}
			}
			string targetPath = AssetDatabase.GetAssetPath (targetScript);

			foreach (string prefab in allPrefabs) {
				string[] single = new string[] { prefab };
				string[] dependencies = AssetDatabase.GetDependencies (single);
				foreach (string dependedAsset in dependencies) {
					if (dependedAsset == targetPath) {
						listResult.Add (prefab);
					}
				}
			}
			foreach (var path in listResult) {
				var source = AssetDatabase.LoadAssetAtPath<GameObject> (path);
				var fab = GameObject.Instantiate(source);
				var legacy = fab.GetComponent<LSBody> ();
				legacy.Replace ();
				DestroyImmediate (legacy);
				PrefabUtility.MergeAllPrefabInstances(source);
				GameObject.DestroyImmediate(fab);
			}

		}

		public static string[] GetAllPrefabs ()
		{
			string[] temp = AssetDatabase.GetAllAssetPaths ();
			List<string> result = new List<string> ();
			foreach (string s in temp) {
				if (s.Contains (".prefab"))
					result.Add (s);
			}
			return result.ToArray ();
		}
	}
}