#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using Lockstep;
using System;
using UnityEditor;
namespace Lockstep.Integration
{
	public static class LSEditorUtility
	{
		public static void FixedNumberField (GUIContent content, ref long Value)
		{
			Value = FixedMath.Create (EditorGUILayout.DoubleField (content, Math.Round (FixedMath.ToDouble (Value), 2, MidpointRounding.AwayFromZero)));
		}
		public static void FixedNumberField (GUIContent content, int Rounding, ref long Value)
		{
			Value = FixedMath.Create (EditorGUILayout.DoubleField (content, Math.Round (FixedMath.ToDouble (Value), Rounding, MidpointRounding.AwayFromZero)));
		}

		public static void FixedNumberField (string label, ref long Value)
		{
			Value = FixedMath.Create (EditorGUILayout.DoubleField (label, Math.Round (FixedMath.ToDouble (Value), 2, MidpointRounding.AwayFromZero)));
		}

		public static void Vector2dField (string Label, ref Vector2d vector)
		{
			vector = new Vector2d (EditorGUILayout.Vector2Field (Label, vector.ToVector2 ()));
		}

		public static double Round (long value)
		{
			return Math.Round (FixedMath.ToDouble (value), 2, MidpointRounding.AwayFromZero);
		}

		public static void GizmoCircle (Vector3 position, float Radius)
		{
			float theta = 0;
			float x = Radius * Mathf.Cos (theta);
			float y = Radius * Mathf.Sin (theta);
			Vector3 pos = position + new Vector3 (x, 0, y);
			Vector3 newPos = pos;
			Vector3 lastPos = pos;
			for (theta = 0.1f; theta<Mathf.PI*2f; theta+=0.1f) {
				x = Radius * Mathf.Cos (theta);
				y = Radius * Mathf.Sin (theta);
				newPos = position + new Vector3 (x, 0, y);
				Gizmos.DrawLine (pos, newPos);
				pos = newPos;
			}
			Gizmos.DrawLine (pos, lastPos);
		}

		public static void GizmoPolyLine (Vector3[] polyLine)
		{
			for (int i = 0; i < polyLine.Length; i++) {
				if (i + 1 == polyLine.Length) {
					Gizmos.DrawLine (polyLine [i], polyLine [0]);
				} else {
					Gizmos.DrawLine (polyLine [i], polyLine [i + 1]);
				}
			}
		}
	}
}
#endif