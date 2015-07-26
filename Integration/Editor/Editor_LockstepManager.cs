using UnityEngine;
using System.Collections;
using UnityEditor;
using Lockstep;
using System;

[CustomEditor (typeof(LockstepManager))]
public class Editor_LockstepManager : Editor
{
	static AgentCode[] AllAgentCodes = (AgentCode[])Enum.GetValues (typeof(AgentCode));
	static string[] AllAgentNames = Enum.GetNames (typeof(AgentCode));
	static GUIStyle TitleStyle = new GUIStyle ();
	static bool ShowAgentObjects = true;

	public override void OnInspectorGUI ()
	{
		AllAgentCodes = (AgentCode[])Enum.GetValues (typeof(AgentCode));
		TitleStyle.fontStyle = FontStyle.Bold;
		LockstepManager Target = (LockstepManager)target;
		EditorGUI.BeginChangeCheck (); 

		Target.SelectionRing = (GameObject)EditorGUILayout.ObjectField ("Selection Ring Object", Target.SelectionRing,typeof(GameObject),false);

		#region AgentObjects
		ShowAgentObjects = EditorGUILayout.Foldout (ShowAgentObjects, "Spawnable Agents");

		if (ShowAgentObjects) {
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Space (15f);
			EditorGUILayout.BeginVertical ();
			if (Target.AgentObjects.Length != AllAgentCodes.Length) {
				Array.Resize (ref Target.AgentObjects, AllAgentCodes.Length);
			}

			for (int i = 0; i < AllAgentCodes.Length; i++) {
				GameObject go = (GameObject)EditorGUILayout.ObjectField (
				AllAgentNames [i],
				Target.AgentObjects [i], 
				typeof(GameObject), 
				false);
				if (go != null && go.GetComponent<LSAgent> () == null) {
					Debug.LogError (go + " does not have the LSAgent component attached and cannot be created.");
					continue;
				}
				Target.AgentObjects [i] = go;
			}
			EditorGUILayout.EndVertical ();
			EditorGUILayout.EndHorizontal ();
		}

		#endregion
		if (EditorGUI.EndChangeCheck ())
		{
			serializedObject.ApplyModifiedProperties ();
			//EditorUtility.SetDirty (target);
		}

	}
}
