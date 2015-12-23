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
        SerializedProperty Height;
        //long
        SerializedProperty PositionalTransform;
        //transform
        SerializedProperty RotationalTransform;
        //transform

        SerializedObject so { get { return base.serializedObject; } }

        bool MoreThanOne;

        void OnEnable()
        {
            MoreThanOne = targets.Length > 1;
            Shape = so.FindProperty("_shape");
            IsTrigger = so.FindProperty("_isTrigger");
            Layer = so.FindProperty("_layer");
            HalfWidth = so.FindProperty("_halfWidth");
            HalfHeight = so.FindProperty("_halfHeight");
            Radius = so.FindProperty("_radius");
            Immovable = so.FindProperty("_immovable");
            Vertices = so.FindProperty("_vertices");
            Height = so.FindProperty("_height");
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
                Height.Draw();

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
                    EditorGUIUtility.LookLikeControls();


                    Vertices.Draw();
                }
                    
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
            
            if (MoreThanOne) return;

            //Have to reinitialize everything because can't apply modified properties on base.serializedObject
            SerializedObject so = new SerializedObject(target);
            so.Update();
            SerializedProperty Shape = so.FindProperty("_shape");
            SerializedProperty IsTrigger = so.FindProperty("_isTrigger");
            SerializedProperty Layer = so.FindProperty("_layer");
            SerializedProperty HalfWidth = so.FindProperty("_halfWidth");
            SerializedProperty HalfHeight = so.FindProperty("_halfHeight");
            SerializedProperty Radius = so.FindProperty("_radius");
            SerializedProperty Immovable = so.FindProperty("_immovable");
            SerializedProperty Vertices = so.FindProperty("_vertices");
            SerializedProperty Height = so.FindProperty("_height");
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
            int spreadMin = -1;
            int spreadMax = 1;
            Handles.DrawCapFunction dragCap = Handles.SphereCap;

            float xModifier = 0f;
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
                float baseHeight = targetPos.y;
                for (int i = spreadMin; i <= spreadMax; i++)
                {
                    Handles.CircleCap(
                        1,
                        new Vector3(targetPos.x, baseHeight + (float)i * spread, targetPos.z)
                        , Quaternion.Euler(90, 0, 0), Radius.longValue.ToFloat());
                }
                baseHeight = targetPos.y + Height.longValue.ToFloat();
                for (int i = spreadMin; i <= spreadMax; i++)
                {
                    Handles.CircleCap(
                        1,
                        new Vector3(targetPos.x, baseHeight + (float)i * spread, targetPos.z)
                        , Quaternion.Euler(90, 0, 0), Radius.longValue.ToFloat());
                }
                xModifier = Radius.longValue.ToFloat();

            } else if (shape == ColliderType.AABox)
            {
                HalfWidth.longValue =
                    FixedMath.Create(
                        (double)Mathf.Abs(
                            Handles.FreeMoveHandle(
                                new Vector3(targetPos.x - (float)HalfWidth.longValue.ToFormattedDouble(), targetPos.y, targetPos.z),
                            Quaternion.identity,
                            dragHandleSize,
                            Vector3.zero,
                            dragCap)
                            .x - targetPos.x)
                );
                HalfHeight.longValue =
                    FixedMath.Create(
                        (double)System.Math.Abs(
                            Handles.FreeMoveHandle(
                                new Vector3(targetPos.x, targetPos.y, targetPos.z - (float)HalfHeight.longValue.ToFormattedDouble()),
                            Quaternion.identity,
                            dragHandleSize,
                            Vector3.zero,
                            dragCap)
                            .z - targetPos.z)
                );
                float halfWidth = HalfWidth.longValue.ToFloat();
                float halfHeight = HalfHeight.longValue.ToFloat();
                for (int i = 0; i < 1; i++)
                {
                    float height = targetPos.y + (float)i * spread;
                    Vector3[] lines = new Vector3[]
                    {
                        new Vector3(targetPos.x + halfWidth, height, targetPos.z + halfHeight),
                        new Vector3(targetPos.x + halfWidth, height, targetPos.z - halfHeight),

                        new Vector3(targetPos.x + halfWidth, height, targetPos.z - halfHeight),
                        new Vector3(targetPos.x - halfWidth, height, targetPos.z - halfHeight),

                        new Vector3(targetPos.x - halfWidth, height, targetPos.z - halfHeight),
                        new Vector3(targetPos.x - halfWidth, height, targetPos.z + halfHeight),

                        new Vector3(targetPos.x - halfWidth, height, targetPos.z + halfHeight),
                        new Vector3(targetPos.x + halfWidth, height, targetPos.z + halfHeight)
                    };
                    Handles.DrawLines(lines);
                }
                for (int i = 0; i < 1; i++)
                {
                    float height = targetPos.y + (float)i * spread + Height.longValue.ToFloat();
                    Vector3[] lines = new Vector3[]
                    {
                        new Vector3(targetPos.x + halfWidth, height, targetPos.z + halfHeight),
                        new Vector3(targetPos.x + halfWidth, height, targetPos.z - halfHeight),

                        new Vector3(targetPos.x + halfWidth, height, targetPos.z - halfHeight),
                        new Vector3(targetPos.x - halfWidth, height, targetPos.z - halfHeight),

                        new Vector3(targetPos.x - halfWidth, height, targetPos.z - halfHeight),
                        new Vector3(targetPos.x - halfWidth, height, targetPos.z + halfHeight),

                        new Vector3(targetPos.x - halfWidth, height, targetPos.z + halfHeight),
                        new Vector3(targetPos.x + halfWidth, height, targetPos.z + halfHeight)
                    };
                    Handles.DrawLines(lines);
                }

                xModifier = halfWidth;
            }
            Handles.DrawLine(
                new Vector3(targetPos.x + xModifier, targetPos.y, targetPos.z), 
                new Vector3(targetPos.x + xModifier, targetPos.y + Height.longValue.ToFloat(), targetPos.z));

            Vector3 movePos = targetPos;
            movePos.x += xModifier;
            movePos.y += (float)Height.longValue.ToFormattedDouble();
            movePos = 
                Handles.FreeMoveHandle(
                    movePos,
                    Quaternion.identity,
                    dragHandleSize,
                    Vector3.zero,
                    dragCap
                );
            Height.longValue = FixedMath.Create(Mathf.Max(Mathf.Abs(movePos.y - targetPos.y)));
            so.ApplyModifiedProperties();
        }
    }

    internal static class SerializedPropertyDraw
    {
        public static void Draw(this SerializedProperty prop)
        {
            EditorGUILayout.PropertyField(prop,true);

        }
    }
}