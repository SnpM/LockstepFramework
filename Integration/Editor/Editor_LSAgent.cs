using UnityEngine;
using System.Collections;
using System;
using Lockstep;
using UnityEditor;
using Lockstep.Integration;

[CustomEditor(typeof(LSAgent))]
public class Editor_LSAgent : Editor
{
	static bool ShowDetail;
	public override void OnInspectorGUI ()
	{
		LSAgent agent = (LSAgent)target;
		EditorGUI.BeginChangeCheck();

		agent.SelectionRadius = EditorGUILayout.FloatField("Selection Radius",agent.SelectionRadius);
	}
}
