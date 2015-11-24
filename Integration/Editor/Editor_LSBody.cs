using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using UnityEditor;

namespace Lockstep.Integration
{


	[CustomEditor(typeof(LSBody))]
	public class Editor_LSCollider : Editor
	{
		LSBody leTarget;
		public static Color MoveColor = Color.yellow;
		public static Color FrameColor = Color.blue;

		public override void OnInspectorGUI ()
		{
			if (Application.isPlaying) {
				EditorGUILayout.LabelField("Debug Information",EditorStyles.boldLabel);

				base.DrawDefaultInspector();
				return;
			}
			EditorGUI.BeginChangeCheck ();

			if (leTarget == null)
			{
				leTarget = (LSBody)target;
			}

			if (leTarget.cachedTransform == null)
				leTarget.cachedTransform = leTarget.GetComponent<Transform>();
			if (leTarget.cachedGameObject == null)
				leTarget.cachedGameObject = leTarget.GetComponent<GameObject>();
			

			Vector3 transformPos = leTarget.cachedTransform.position;
			leTarget.Position.x = FixedMath.Create (transformPos.x);
			leTarget.Position.y = FixedMath.Create (transformPos.z);

			Vector3 transformRot = leTarget.cachedTransform.eulerAngles;
			leTarget.Rotation = Vector2d.up;
			leTarget.Rotation.Rotate (FixedMath.Create (Mathf.Sin (transformRot.y * Mathf.Deg2Rad)), FixedMath.Create (Mathf.Cos (transformRot.y * Mathf.Deg2Rad)));
			//leTarget.Interpolate = EditorGUILayout.Toggle ("Interpolate", leTarget.Interpolate);


			EditorGUILayout.Space();

			
			EditorGUILayout.LabelField ("Collider Settings",EditorStyles.boldLabel);
			leTarget.Shape = (ColliderType)EditorGUILayout.EnumPopup("Shape", leTarget.Shape);
			if (leTarget.Shape == ColliderType.None) return;
			leTarget.IsTrigger = EditorGUILayout.Toggle ("Is Trigger", leTarget.IsTrigger);


			if (!leTarget.IsTrigger && !leTarget.Immovable)
			{
				GUIContent PriorityContent = new GUIContent ("Priority", "The priority of this object in collisions. Objects of lower priority yield to objects of higher priority.");
				leTarget.Priority = EditorGUILayout.IntField (PriorityContent, leTarget.Priority);
			}

			switch (leTarget.Shape) {
			case ColliderType.Circle:
				GUIContent ImmovableContent = new GUIContent ("Immovable", "Is this object immovable, i.e. a wall. Note: non-immovable objects are not supported for any shape except circle.");
				leTarget.Immovable = EditorGUILayout.Toggle (ImmovableContent, leTarget.Immovable);
				EditorGUILayout.BeginHorizontal ();
				LSEditorUtility.FixedNumberField ("Radius", ref leTarget.Radius);
				EditorGUILayout.EndHorizontal ();
				break;
			case ColliderType.AABox:
				leTarget.Immovable = true;
				EditorGUILayout.BeginHorizontal ();
				LSEditorUtility.FixedNumberField ("Half Width", ref leTarget.HalfWidth);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				LSEditorUtility.FixedNumberField ("Half Height", ref leTarget.HalfHeight);
				EditorGUILayout.EndHorizontal ();
				break;
			case ColliderType.Polygon:
				leTarget.Immovable = true;
				if (leTarget.Vertices == null || leTarget.Vertices.Length == 0)
				{
					leTarget.Vertices = new Vector2d[4];
					leTarget.Vertices[0] = new Vector2d(FixedMath.Half, FixedMath.Half);
					leTarget.Vertices[1] = new Vector2d (FixedMath.Half, -FixedMath.Half);
					leTarget.Vertices[2] = new Vector2d (-FixedMath.Half, -FixedMath.Half);
					leTarget.Vertices[3] = new Vector2d (-FixedMath.Half, FixedMath.Half);
				}

				EditorGUILayout.BeginHorizontal();
				int VerticesCount = EditorGUILayout.IntField ("Vertices count", leTarget.Vertices.Length);
				EditorGUILayout.EndHorizontal();
				if (VerticesCount > leTarget.Vertices.Length)
				{
					Vector2d[] NewVertices = new Vector2d[VerticesCount];
					leTarget.Vertices.CopyTo (NewVertices, 0);
					for (int i = leTarget.Vertices.Length; i < VerticesCount; i++)
					{
						NewVertices[i] = new Vector2d (-FixedMath.One, 0);
					}
					leTarget.Vertices = NewVertices;
				}
				else if (VerticesCount < leTarget.Vertices.Length)
				{
					Vector2d[] NewVertices = new Vector2d[VerticesCount];
					for (int i = 0; i < VerticesCount; i++)
					{
						NewVertices[i] = leTarget.Vertices[i];
					}
					leTarget.Vertices = NewVertices;
				}
				for (int i = 0; i < leTarget.Vertices.Length; i++)
				{
					EditorGUILayout.BeginHorizontal();
					LSEditorUtility.Vector2dField ("V" + i.ToString () + ":", ref leTarget.Vertices[i]);
					EditorGUILayout.EndHorizontal();
				}
				break;
			}


			SceneView.RepaintAll ();
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo (leTarget,"LSBody Change");
				EditorUtility.SetDirty (leTarget);

			}
		}

		static Quaternion CapRotation = Quaternion.Euler (90, 0, 0);

		public void OnSceneGUI ()
		{

			if (Application.isPlaying) return;

			if (leTarget == null)
				leTarget = (LSBody)(target);
			Vector3 TargetPosition = leTarget.cachedTransform.position;
			TargetPosition.y += .55f;
			Vector3[] PolyLine;
			switch (leTarget.Shape) {
			case ColliderType.Circle:
				Handles.color = FrameColor;
				Handles.CircleCap (1, TargetPosition, CapRotation, FixedMath.ToFloat (leTarget.Radius));
				Handles.CircleCap (1, TargetPosition, CapRotation, FixedMath.ToFloat (leTarget.Radius) + -.005f);
				Handles.CircleCap (1, TargetPosition, CapRotation, FixedMath.ToFloat (leTarget.Radius) + .005f);


				Vector3 NewVec = TargetPosition;
				NewVec.x -= (float)Math.Round(FixedMath.ToDouble (leTarget.Radius), 4, MidpointRounding.AwayFromZero);
				Handles.color = MoveColor;
				NewVec = Handles.FreeMoveHandle (NewVec, Quaternion.identity, .35f, Vector3.zero, Handles.SphereCap);
				leTarget.Radius = FixedMath.Create (NewVec.x - TargetPosition.x);
				if (leTarget.Radius < 0) leTarget.Radius = -leTarget.Radius;
				break;
			case ColliderType.AABox:
				float halfWidth = FixedMath.ToFloat(leTarget.HalfWidth);
				float halfHeight = FixedMath.ToFloat (leTarget.HalfHeight);
				PolyLine = new Vector3[5];
				for (int i = 0; i < 4; i++)
				{
					PolyLine[i] = TargetPosition;
				}

				PolyLine[0].x += halfWidth;
				PolyLine[0].z += halfHeight;

				PolyLine[1].x += halfWidth;
				PolyLine[1].z -= halfHeight;

				PolyLine[2].x -= halfWidth;
				PolyLine[2].z -= halfHeight;

				PolyLine[3].x -= halfWidth;
				PolyLine[3].z += halfHeight;

				PolyLine[4] = PolyLine[0];
				Handles.color = FrameColor;
				Handles.DrawAAPolyLine (5f,PolyLine);
				Handles.color = MoveColor;

				Vector3 WidthScaler = TargetPosition;
				WidthScaler.x -= (float)LSEditorUtility.Round (leTarget.HalfWidth);
				WidthScaler = Handles.FreeMoveHandle (WidthScaler, Quaternion.identity, .35f, Vector3.zero, Handles.SphereCap);
				leTarget.HalfWidth = FixedMath.Create (WidthScaler.x - TargetPosition.x);

				Vector3 HeightScaler = TargetPosition;
				HeightScaler.z -= (float)LSEditorUtility.Round (leTarget.HalfHeight);
				HeightScaler = Handles.FreeMoveHandle (HeightScaler, Quaternion.identity, .35f, Vector3.zero, Handles.SphereCap);
				leTarget.HalfHeight = FixedMath.Create (HeightScaler.z - TargetPosition.z);
				if (leTarget.HalfWidth <= 0) leTarget.HalfWidth = -leTarget.HalfWidth;
				if (leTarget.HalfHeight < 0) leTarget.HalfHeight = -leTarget.HalfHeight;

				break;
			case ColliderType.Polygon:
				if (leTarget.Vertices == null || leTarget.Vertices.Length == 0)
					break;

				PolyLine = new Vector3[leTarget.Vertices.Length + 1];
				Handles.color = MoveColor;
				for (int i = 0; i < leTarget.Vertices.Length; i++)
				{
					Vector3 vertPos = leTarget.Vertices[i].ToVector3(0f) + TargetPosition;
					Vector3 freemoveVec = Handles.FreeMoveHandle (vertPos, Quaternion.identity, .35f, Vector3.zero, Handles.SphereCap) - TargetPosition;
					leTarget.Vertices[i] = new Vector2d (FixedMath.Create (freemoveVec.x), FixedMath.Create(freemoveVec.z));
					Handles.Label (vertPos, "V" + i.ToString());
					PolyLine[i] = vertPos;
				
				}

				PolyLine[leTarget.Vertices.Length] = PolyLine[0];
				Handles.color = FrameColor;
				Handles.DrawAAPolyLine (5f,PolyLine);
				break;
			}
		}
	}

}