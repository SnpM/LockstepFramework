using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Lockstep.Integration
{
    [CustomEditor(typeof(LSBody), true),UnityEditor.CanEditMultipleObjects]
    public class EditorLSBody : Editor
    {

        SerializedProperty Shape;
        //Enum
        SerializedProperty IsTrigger;
        //bool
        SerializedProperty Layer;
        //int
        SerializedProperty HalfWidth;
        //long
        SerializedProperty HalfHeight;
        //long
        SerializedProperty Radius;
        //long
        SerializedProperty Immovable;
        //bool
        SerializedProperty Vertices;
        //Vector2d[]
        SerializedProperty PositionalTransform;
        //transform
        SerializedProperty RotationalTransform;
        //transform

        SerializedObject so { get { return base.serializedObject; } }

        void OnEnable()
        {
            
            Shape = so.FindProperty("_shape");
            IsTrigger = so.FindProperty("_isTrigger");
            Layer = so.FindProperty("_layer");
            HalfWidth = so.FindProperty("_halfWidth");
            HalfHeight = so.FindProperty("_halfHeight");
            Radius = so.FindProperty("_radius");
            Immovable = so.FindProperty("_immovable");
            Vertices = so.FindProperty("_vertices");
            PositionalTransform = so.FindProperty("_positionalTransform");
            RotationalTransform = so.FindProperty("_rotationalTransform");
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying == false)
            {

                EditorGUI.BeginChangeCheck();
                if (targets.Length == 1)
                {
                    PositionalTransform.Draw();
                    RotationalTransform.Draw();
                }

                Shape.Draw();
                ColliderType shape = (ColliderType)Shape.intValue;
                if (shape == ColliderType.None)
                    return;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("General Collider Settings", EditorStyles.boldLabel);
                Layer.Draw();
                IsTrigger.Draw();
                if (IsTrigger.boolValue == false)
                    Immovable.Draw();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Collider Settings", EditorStyles.boldLabel); 
                if (shape == ColliderType.Circle)
                {
                    Radius.Draw();
                } else if (shape == ColliderType.AABox)
                {
                    HalfWidth.Draw();
                    HalfHeight.Draw();
                } else if (shape == ColliderType.Polygon)
                {
                    Vertices.Draw();
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Note: Scene editing is temporarily disabled.", EditorStyles.boldLabel);
                SceneView.RepaintAll();
                if (true)//EditorGUI.EndChangeCheck())
                {
                    so.ApplyModifiedProperties();
                }
            } else
            {
                //Debug view when playing
                EditorGUILayout.LabelField("Runtime Debugging", EditorStyles.boldLabel);
                base.OnInspectorGUI();
            }
        }

        void OnSceneGUI()
        {
            //Have to reinitialize everything because can't apply modified properties on base.serializedObject
            SerializedObject so = new SerializedObject(target);
            SerializedProperty Shape = so.FindProperty("_shape");
            SerializedProperty IsTrigger = so.FindProperty("_isTrigger");
            SerializedProperty Layer = so.FindProperty("_layer");
            SerializedProperty HalfWidth = so.FindProperty("_halfWidth");
            SerializedProperty HalfHeight = so.FindProperty("_halfHeight");
            SerializedProperty Radius = so.FindProperty("_radius");
            SerializedProperty Immovable = so.FindProperty("_immovable");
            SerializedProperty Vertices = so.FindProperty("_vertices");
            SerializedProperty PositionalTransform = so.FindProperty("_positionalTransform");
            SerializedProperty RotationalTransform = so.FindProperty("_rotationalTransform");

            ColliderType shape = (ColliderType)Shape.intValue;
            if (shape == ColliderType.None)
                return;
            Handles.color = Color.blue;
            Vector3 targetPos = (target as LSBody).transform.position;
            const int ImprecisionLimit = 100000;
            if (Mathf.Abs(targetPos.x) >= ImprecisionLimit ||
                Mathf.Abs(targetPos.y) >= ImprecisionLimit ||
                Mathf.Abs(targetPos.z) >= ImprecisionLimit)
                return;
            const float dragHandleSize = .5f;
            const float spread = .02f;
            Handles.DrawCapFunction dragCap = Handles.SphereCap;
            if (shape == ColliderType.Circle)
            {
                //targetPos.x - Radius.longValue.ToFloat ():
                //Minus so the move handle doesn't end up on the same axis as the transform.position move handle
                Radius.longValue =
                    FixedMath.Create(
                    Mathf.Abs(
                        (Handles.FreeMoveHandle(
                            new Vector3(targetPos.x - Radius.longValue.ToFloat(), targetPos.y, targetPos.z)
                                , Quaternion.identity,
                                dragHandleSize,
                                Vector3.zero,
                                Handles.SphereCap))
                            .x - targetPos.x) 
                    );
                Handles.DrawLine(targetPos, new Vector3(targetPos.x + Radius.longValue.ToFloat(), targetPos.y, targetPos.z));
                for (int i = -1; i < 2; i++)
                    Handles.CircleCap(1,targetPos + Vector3.up * (float)i * spread,Quaternion.Euler(90,0,0),Radius.longValue.ToFloat());
            }
            else if (shape == ColliderType.AABox) {
                HalfWidth.longValue =
                    FixedMath.Create (
                        Mathf.Abs (
                            Handles.FreeMoveHandle (
                                new Vector3(targetPos.x - HalfWidth.longValue.ToFloat(), targetPos.y, targetPos.z),
                                Quaternion.identity,
                                dragHandleSize,
                                Vector3.zero,
                                dragCap)
                            .x - targetPos.x)
                    );
                HalfHeight.longValue =
                    FixedMath.Create (
                        Mathf.Abs (
                            Handles.FreeMoveHandle (
                                new Vector3(targetPos.x, targetPos.y, targetPos.z - HalfHeight.longValue.ToFloat()),
                                Quaternion.identity,
                                dragHandleSize,
                                Vector3.zero,
                                dragCap)
                            .z - targetPos.z)
                    );
                float halfWidth = HalfWidth.longValue.ToFloat ();
                float halfHeight = HalfHeight.longValue.ToFloat ();
                for (int i = -1; i < 2; i++) {
                    float height = targetPos.y + (float)i * spread;
                    Vector3[] lines = new Vector3[] {
                        new Vector3 (targetPos.x + halfWidth, height, targetPos.z + halfHeight),
                        new Vector3 (targetPos.x + halfWidth, height, targetPos.z - halfHeight),

                        new Vector3 (targetPos.x + halfWidth, height, targetPos.z - halfHeight),
                        new Vector3 (targetPos.x - halfWidth, height, targetPos.z - halfHeight),

                        new Vector3 (targetPos.x - halfWidth, height, targetPos.z - halfHeight),
                        new Vector3 (targetPos.x - halfWidth, height, targetPos.z + halfHeight),

                        new Vector3 (targetPos.x - halfWidth, height, targetPos.z + halfHeight),
                        new Vector3 (targetPos.x + halfWidth, height, targetPos.z + halfHeight)
                    };
                    Handles.DrawLines (lines);
                }
            }

            so.ApplyModifiedProperties();
        }
    }

    internal static class SerializedPropertyDraw
    {
        public static void Draw(this SerializedProperty prop)
        {
            EditorGUILayout.PropertyField(prop);

        }
    }
}