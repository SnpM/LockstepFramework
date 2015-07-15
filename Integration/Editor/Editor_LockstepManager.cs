using UnityEngine;
using System.Collections;
using UnityEditor;
using Lockstep;
using System;

[CustomEditor (typeof (LockstepManager))]
public class Editor_LockstepManager : Editor {
	static AgentCode[] AllAgentCodes = (AgentCode[]) Enum.GetValues (typeof (AgentCode));
	static string[] AllAgentNames = Enum.GetNames (typeof(AgentCode));
	static GUIStyle TitleStyle = new GUIStyle();

	bool ShowAgentObjects;
	public override void OnInspectorGUI ()
	{
		TitleStyle.fontStyle = FontStyle.Bold;
		LockstepManager Target = (LockstepManager)target;
		EditorGUI.BeginChangeCheck (); 
		#region AgentObjects
		EditorGUILayout.Space ();


		ShowAgentObjects = EditorGUILayout.Foldout (ShowAgentObjects,"Spawnable Agents");

		if (ShowAgentObjects)
		{
		if (Target.AgentObjects.Length != AllAgentCodes.Length)
		{
			Array.Resize (ref Target.AgentObjects, AllAgentCodes.Length);
		}
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Space (15f);
		for (int i = 0; i < AllAgentCodes.Length; i++)
		{
			GameObject go = (GameObject)EditorGUILayout.ObjectField (
				AllAgentNames[i],
				Target.AgentObjects[i], 
				typeof (GameObject), 
				false);
			if (go != null && go != Target.AgentObjects[i] && go.GetComponent<LSAgent>() == null)
			{
				Debug.LogError (go + " does not have the LSAgent component attached and cannot be created.");
				continue;
			}
			Target.AgentObjects[i] = go;

		}
			EditorGUILayout.EndHorizontal ();
		}
		Target.AllAgentCodes = AllAgentCodes;
		
		#endregion
		if (EditorGUI.EndChangeCheck ())
		serializedObject.ApplyModifiedProperties ();

	}
}
