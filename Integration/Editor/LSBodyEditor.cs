#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Lockstep.Integration
{
    [CustomEditor(typeof(LSBody))]//, CanEditMultipleObjects]
	public class LSBodyEditor : Editor
    {
        public static Color MoveColor = Color.yellow;
        public static Color FrameColor = Color.blue;

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Debug Information", EditorStyles.boldLabel);
				
                base.DrawDefaultInspector();
                return;
            }
            EditorGUI.BeginChangeCheck();
            LSBody[] Bodies = GenerateTargets();
            LSBody Body = Bodies [0];
			
            if (Bodies.Length == 1)
            {
                Body._positionalTransform = (Transform)EditorGUILayout.ObjectField(
                    "Positional Transform",
                    Body._positionalTransform,
                    typeof(Transform),
                    true);
                Body._rotationalTransform = (Transform)EditorGUILayout.ObjectField(
                    "Rotational Transform",
                    Body._rotationalTransform,
                    typeof(Transform),
                    true);

                EditorGUILayout.Space();
            }
            Body.Layer = EditorGUILayout.LayerField("Layer", Body.Layer);
			
            for (int i = 0; i < Bodies.Length; i++)
            {
                LSBody body = Bodies [i];
                Transform posTransform = body._positionalTransform != null ? body._positionalTransform : body.transform;
                body.Position = new Vector2d(posTransform.position);
                Transform rotTransform = body._rotationalTransform != null ? body._rotationalTransform : body.transform;
                body.Rotation = new Vector2d(Mathf.Sin(rotTransform.rotation.x), Mathf.Cos(rotTransform.rotation.y));
            }

            EditorGUILayout.LabelField("Collider Settings", EditorStyles.boldLabel);
            Body.Shape = (ColliderType)EditorGUILayout.EnumPopup("Shape", Body.Shape);
            for (int i = 1; i < Bodies.Length; i++)
            {
                Bodies [i].Shape = Body.Shape;
            }
            if (Body.Shape == ColliderType.None)
            {
                return;
            }
            Body.IsTrigger = EditorGUILayout.Toggle("Is Trigger", Body.IsTrigger);
			
            if (!Body.IsTrigger)
            {
                var ImmovableContent = new GUIContent("Immovable", "Is this object immovable, i.e. a wall. Note: non-immovable objects are not supported for any shape except circle.");
                Body.Immovable = EditorGUILayout.Toggle(ImmovableContent, Body.Immovable);
                if (!Body.Immovable)
                {
                    var PriorityContent = new GUIContent("Priority", "The priority of this object in collisions. Objects of lower priority yield to objects of higher priority.");
                    SerializedProperty sp = serializedObject.FindProperty("_priority");
                    sp.intValue = EditorGUILayout.IntField(PriorityContent, sp.intValue);
                }
            }
            if (Body.Shape != ColliderType.None)
            {

            }
            switch (Body.Shape)
            {
                case ColliderType.Circle:

                    LSEditorUtility.FixedNumberField("Radius", ref Body.Radius);
                    for (int i = 1; i < Bodies.Length; i++)
                    {
                        Bodies [i].Radius = Body.Radius;
                    }
                    break;
                case ColliderType.AABox:
                    LSEditorUtility.FixedNumberField("Half Width", ref Body.HalfWidth);
                    LSEditorUtility.FixedNumberField("Half Height", ref Body.HalfHeight);
                    for (int i = 1; i < Bodies.Length; i++)
                    {
                        Bodies [i].HalfWidth = Body.HalfWidth;
                        Bodies [i].HalfHeight = Body.HalfHeight;
                    }
                    break;
                case ColliderType.Polygon:
                    if (Body.Vertices == null || Body.Vertices.Length == 0)
                    {
                        Body.Vertices = new Vector2d[4];
                        Body.Vertices [0] = new Vector2d(FixedMath.Half, FixedMath.Half);
                        Body.Vertices [1] = new Vector2d(FixedMath.Half, -FixedMath.Half);
                        Body.Vertices [2] = new Vector2d(-FixedMath.Half, -FixedMath.Half);
                        Body.Vertices [3] = new Vector2d(-FixedMath.Half, FixedMath.Half);
                    }
				
                    int VerticesCount = EditorGUILayout.IntField("Vertices count", Body.Vertices.Length);
				
                    if (VerticesCount > Body.Vertices.Length)
                    {
                        var NewVertices = new Vector2d[VerticesCount];
                        Body.Vertices.CopyTo(NewVertices, 0);
                        for (int i = Body.Vertices.Length; i < VerticesCount; i++)
                        {
                            NewVertices [i] = new Vector2d(-FixedMath.One, 0);
                        }
                        Body.Vertices = NewVertices;
                    } else if (VerticesCount < Body.Vertices.Length)
                    {
                        var NewVertices = new Vector2d[VerticesCount];
                        for (int i = 0; i < VerticesCount; i++)
                        {
                            NewVertices [i] = Body.Vertices [i];
                        }
                        Body.Vertices = NewVertices;
                    }
                    for (int i = 0; i < Body.Vertices.Length; i++)
                    {
                        LSEditorUtility.Vector2dField("V" + i.ToString() + ":", ref Body.Vertices [i]);
                    }
                    for (int i = 1; i < Bodies.Length; i++)
                    {
                        Bodies [i].Vertices = Body.Vertices;
                    }
                    break;
            }
            for (int i = 1; i < Bodies.Length; i++)
            {
                Bodies [i].IsTrigger = Body.IsTrigger;
                Bodies [i].Immovable = Body.Immovable;
                Bodies [i].IsTrigger = Body.IsTrigger;
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Body, "LSBody Change");
                serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }
        }

        private static readonly Quaternion CapRotation = Quaternion.Euler(90, 0, 0);

        public void OnSceneGUI()
        {
            if (Application.isPlaying)
            {
                return;
            }
            LSBody Body = (LSBody)(target);
            Transform cachedTransform = Body.transform;
            Vector3 TargetPosition = cachedTransform.position;
            TargetPosition.y += .55f;
            Vector3[] PolyLine;
            switch (Body.Shape)
            {
                case ColliderType.Circle:
                    Handles.color = FrameColor;
                    Handles.CircleCap(1, TargetPosition, CapRotation, FixedMath.ToFloat(Body.Radius));
                    Handles.CircleCap(1, TargetPosition, CapRotation, FixedMath.ToFloat(Body.Radius) + -.005f);
                    Handles.CircleCap(1, TargetPosition, CapRotation, FixedMath.ToFloat(Body.Radius) + .005f);
					
                    Vector3 NewVec = TargetPosition;
                    NewVec.x -= (float)Math.Round(FixedMath.ToDouble(Body.Radius), 4, MidpointRounding.AwayFromZero);
                    Handles.color = MoveColor;
                    NewVec = Handles.FreeMoveHandle(NewVec, Quaternion.identity, .35f, Vector3.zero, Handles.SphereCap);
                    Body.Radius = FixedMath.Create(NewVec.x - TargetPosition.x);
                    if (Body.Radius < 0)
                    {
                        Body.Radius = -Body.Radius;
                    }
                    break;
                case ColliderType.AABox:
                    float halfWidth = FixedMath.ToFloat(Body.HalfWidth);
                    float halfHeight = FixedMath.ToFloat(Body.HalfHeight);
                    PolyLine = new Vector3[5];
                    for (int i = 0; i < 4; i++)
                    {
                        PolyLine [i] = TargetPosition;
                    }
					
                    PolyLine [0].x += halfWidth;
                    PolyLine [0].z += halfHeight;
					
                    PolyLine [1].x += halfWidth;
                    PolyLine [1].z -= halfHeight;
					
                    PolyLine [2].x -= halfWidth;
                    PolyLine [2].z -= halfHeight;
					
                    PolyLine [3].x -= halfWidth;
                    PolyLine [3].z += halfHeight;
					
                    PolyLine [4] = PolyLine [0];
                    Handles.color = FrameColor;
                    Handles.DrawAAPolyLine(5f, PolyLine);
                    Handles.color = MoveColor;
					
                    Vector3 WidthScaler = TargetPosition;
                    WidthScaler.x -= (float)LSEditorUtility.Round(Body.HalfWidth);
                    WidthScaler = Handles.FreeMoveHandle(WidthScaler, Quaternion.identity, .35f, Vector3.zero, Handles.SphereCap);
                    Body.HalfWidth = FixedMath.Create(WidthScaler.x - TargetPosition.x);
					
                    Vector3 HeightScaler = TargetPosition;
                    HeightScaler.z -= (float)LSEditorUtility.Round(Body.HalfHeight);
                    HeightScaler = Handles.FreeMoveHandle(HeightScaler, Quaternion.identity, .35f, Vector3.zero, Handles.SphereCap);
                    Body.HalfHeight = FixedMath.Create(HeightScaler.z - TargetPosition.z);
                    if (Body.HalfWidth <= 0)
                    {
                        Body.HalfWidth = -Body.HalfWidth;
                    }
                    if (Body.HalfHeight < 0)
                    {
                        Body.HalfHeight = -Body.HalfHeight;
                    }
					
                    break;
                case ColliderType.Polygon:
                    if (Body.Vertices == null || Body.Vertices.Length == 0)
                    {
                        break;
                    }
					
                    PolyLine = new Vector3[Body.Vertices.Length + 1];
                    Handles.color = MoveColor;
                    for (int i = 0; i < Body.Vertices.Length; i++)
                    {
                        //TODO: Handle world scaling
                        Vector3 vertPos = Body.Vertices [i].ToVector3(0f) + TargetPosition;
                        Vector3 freemoveVec = Handles.FreeMoveHandle(vertPos, Quaternion.identity, .35f, Vector3.zero, Handles.SphereCap) - TargetPosition;
                        Body.Vertices [i] = new Vector2d(FixedMath.Create(freemoveVec.x), FixedMath.Create(freemoveVec.z));
                        Handles.Label(vertPos, "V" + i.ToString());
                        PolyLine [i] = vertPos;
                    }
					
                    PolyLine [Body.Vertices.Length] = PolyLine [0];
                    Handles.color = FrameColor;
                    Handles.DrawAAPolyLine(5f, PolyLine);
                    break;
            }

        }

        private LSBody[] GenerateTargets()
        {
            return Array.ConvertAll<UnityEngine.Object,LSBody>(targets, (x) =>
            {
                return (LSBody)x;
            });
        }
    }
}
#endif