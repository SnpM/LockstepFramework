using UnityEngine;
using UnityEditor;
using Lockstep;
using Lockstep.Integration;
using System;

[CustomEditor(typeof(Ability),true)]
public class Editor_Ability : Editor {
	public override void OnInspectorGUI ()
	{
		Type T = target.GetType ();

		EditorGUI.BeginChangeCheck ();

		if (T == typeof(Move))
		{
			Move Target = (Move)target;
			LSEditorUtility.FixedNumberField ("Speed", ref Target.Speed);
		}
		if (T == typeof (Turn))
		{
			Turn Target = (Turn)target;
			LSEditorUtility.FixedNumberField("Turn Rate", ref Target.TurnRate);
		}

		if (EditorGUI.EndChangeCheck ())
		{
			serializedObject.ApplyModifiedProperties ();
		}
	}
}
