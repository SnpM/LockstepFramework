using UnityEngine;
using System.Collections; using FastCollections;
using UnityEditor;
using Lockstep.Legacy;
using System.Collections.Generic;
using System;
namespace Lockstep.Legacy.Integration
{
	[CustomEditor (typeof(Legacy.LSBody), true),UnityEditor.CanEditMultipleObjects]
	public class EditorLSBody : Editor
	{
		static int guardTimer = 0;

		public override void OnInspectorGUI ()
		{
			guardTimer--;
			if (GUILayout.Button ("Update LSBody")) {
				if (guardTimer <= 0) {
					guardTimer = 10;
					ReplaceLegacy ();
				}
			}
		}

		void ReplaceLegacy ()
		{
			//thanks hpjohn <3
			//http://forum.unity3d.com/threads/editor-want-to-check-all-prefabs-in-a-project-for-an-attached-monobehaviour.253149/
			string[] allPrefabs = GetAllPrefabs ();
			List<string> listResult = new List<string> ();
			MonoScript targetScript = null;

            //todo2: cache
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
                PrefabUtility.ReplacePrefab(fab, source, ReplacePrefabOptions.ConnectToPrefab | ReplacePrefabOptions.ReplaceNameBased );
				GameObject.DestroyImmediate(fab);
			}

            //Now replace legacy LSBodys on in-game objects
            foreach (var legacy in GameObject.FindObjectsOfType<LSBody> ())
            {
                legacy.Replace();
                DestroyImmediate(legacy);

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