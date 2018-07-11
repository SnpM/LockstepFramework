using UnityEngine;
using System.Collections; using FastCollections;
using UnityEditor;
using Lockstep;
#if true
namespace Lockstep.Integration
{
    [CustomEditor(typeof(UnityLSBody), true),UnityEditor.CanEditMultipleObjects]
	public class EditorLSBody : Editor
    {

        SerializedProperty Shape;
        //Enum
        SerializedProperty IsTrigger;
        //bool
        SerializedProperty Layer;
		//int
		SerializedProperty BasePriority;
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

		SerializedObject so { get {return this.serializedObject;}}

        bool MoreThanOne;

        private static GUIStyle _labelStyle;
        public static GUIStyle LabelStyle {
            get {
                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle(EditorStyles.boldLabel);
                    _labelStyle.fontSize = 20;

                }
                return _labelStyle;
            }
        }
        public static float MoveHandleSize {get {return .6f;}}

        void OnEnable()
        {
            MoreThanOne = targets.Length > 1;
            Shape = so.FindProperty("_internalBody").FindPropertyRelative("_shape");
            IsTrigger = so.FindProperty("_internalBody").FindPropertyRelative("_isTrigger");
            Layer = so.FindProperty("_internalBody").FindPropertyRelative("_layer");
			BasePriority = so.FindProperty("_internalBody").FindPropertyRelative("_basePriority");
            HalfWidth = so.FindProperty("_internalBody").FindPropertyRelative("_halfWidth");
            HalfHeight = so.FindProperty("_internalBody").FindPropertyRelative("_halfHeight");
            Radius = so.FindProperty("_internalBody").FindPropertyRelative("_radius");
            Immovable = so.FindProperty("_internalBody").FindPropertyRelative("_immovable");
            Vertices = so.FindProperty("_internalBody").FindPropertyRelative("_vertices");
            Height = so.FindProperty("_internalBody").FindPropertyRelative("_height");
            PositionalTransform = so.FindProperty("_internalBody").FindPropertyRelative("_positionalTransform");
            RotationalTransform = so.FindProperty("_internalBody").FindPropertyRelative("_rotationalTransform");
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying == false)
            {

                EditorGUI.BeginChangeCheck();
                if (GUILayout.Button("Reset Transforms"))
                {
                    for (int i = 0; i < targets.Length; i++)
                    {
                        SerializedObject ser = new SerializedObject(targets [i]);

                        ser.FindProperty("_internalBody").FindPropertyRelative("_positionalTransform").objectReferenceValue = ((UnityLSBody)targets [i]).transform;
                        ser.FindProperty("_internalBody").FindPropertyRelative("_rotationalTransform").objectReferenceValue = ((UnityLSBody)targets [i]).transform;
                        ser.ApplyModifiedProperties();
                    }
                    so.Update();
                }
                if (targets.Length == 1)
                {
                    PositionalTransform.Draw();
                    RotationalTransform.Draw();
                }

                Shape.Draw();
                ColliderType shape = (ColliderType)Shape.intValue;
                if (shape != ColliderType.None)
                {

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("General Collider Settings", EditorStyles.boldLabel);
                    Layer.Draw();
					BasePriority.Draw();
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
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUIUtility.fieldWidth = 0;

                        Vertices.Draw();
                    }
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
            
            if (MoreThanOne)
                return;

            //Have to reinitialize everything because can't apply modified properties on base.serializedObject
			SerializedObject so = new SerializedObject(target);
            so.Update();
            SerializedProperty Shape = so.FindProperty("_internalBody").FindPropertyRelative("_shape");
            SerializedProperty HalfWidth = so.FindProperty("_internalBody").FindPropertyRelative("_halfWidth");
            SerializedProperty HalfHeight = so.FindProperty("_internalBody").FindPropertyRelative("_halfHeight");
            SerializedProperty Radius = so.FindProperty("_internalBody").FindPropertyRelative("_radius");
            SerializedProperty Height = so.FindProperty("_internalBody").FindPropertyRelative("_height");

            /*
            //Currently unused
            SerializedProperty Layer = so.FindProperty("_internalBody").FindPropertyRelative("_layer");
            SerializedProperty IsTrigger = so.FindProperty("_internalBody").FindPropertyRelative("_isTrigger");
            SerializedProperty Immovable = so.FindProperty("_internalBody").FindPropertyRelative("_immovable");
            SerializedProperty Vertices = so.FindProperty("_internalBody").FindPropertyRelative("_vertices");
            SerializedProperty PositionalTransform = so.FindProperty("_internalBody").FindPropertyRelative("_positionalTransform");
            SerializedProperty RotationalTransform = so.FindProperty("_internalBody").FindPropertyRelative("_rotationalTransform");
            */

            ColliderType shape = (ColliderType)Shape.intValue;
            if (shape == ColliderType.None)
                return;
            Handles.color = Color.blue;
			LSBody Body = ((UnityLSBody)target).InternalBody;
			Transform transform = ((UnityLSBody)target).transform;
            Vector3 targetPos = transform.position;
            const int ImprecisionLimit = 100000;
            if (Mathf.Abs(targetPos.x) >= ImprecisionLimit ||
                Mathf.Abs(targetPos.y) >= ImprecisionLimit ||
                Mathf.Abs(targetPos.z) >= ImprecisionLimit)
                return;
            const float spread = .02f;
            int spreadMin = -1;
            int spreadMax = 1;
            Handles.DrawCapFunction dragCap = Handles.SphereCap;
            float height = targetPos.y;
            float xModifier = 0f;
            if (shape == ColliderType.Circle)
            {
                //Minus so the move handle doesn't end up on the same axis as the transform.position move handle
                float oldRadius = Radius.longValue.ToFloat();
                float newRadius =
                    Mathf.Abs(
                        (Handles.FreeMoveHandle(
                            new Vector3(targetPos.x - Radius.longValue.ToFloat(), targetPos.y, targetPos.z)
                                , Quaternion.identity,
                            MoveHandleSize,
                            Vector3.zero,
                            Handles.SphereCap))
                    .x - targetPos.x);
                if (Mathf.Abs(oldRadius - newRadius) >= .02f) {
                    Radius.longValue = FixedMath.Create(newRadius);
                }
                
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
                xModifier = 0;//Radius.longValue.ToFloat();

            } else if (shape == ColliderType.AABox)
            {
                float oldWidth = HalfWidth.longValue.ToFloat();
                float newWidth =
                    Mathf.Abs(
                        Handles.FreeMoveHandle(
                            new Vector3(targetPos.x - (float)HalfWidth.longValue.ToFormattedDouble(), targetPos.y, targetPos.z),
                            Quaternion.identity,
                            MoveHandleSize,
                            Vector3.zero,
                            dragCap)
                        .x - targetPos.x);
                if (Mathf.Abs(newWidth - oldWidth) >= .02f) {
                    HalfWidth.longValue = FixedMath.Create(newWidth);
                }
                float oldHeight = HalfHeight.longValue.ToFloat();
                float newHeight = 
                    System.Math.Abs(
                        Handles.FreeMoveHandle(
                            new Vector3(targetPos.x, targetPos.y, targetPos.z - (float)HalfHeight.longValue.ToFormattedDouble()),
                            Quaternion.identity,
                            MoveHandleSize,
                            Vector3.zero,
                            dragCap)
                        .z - targetPos.z);
                if (Mathf.Abs(newHeight - oldHeight) >= .02f) {
                    HalfHeight.longValue = FixedMath.Create(newHeight);
                }
                float halfWidth = HalfWidth.longValue.ToFloat();
                float halfHeight = HalfHeight.longValue.ToFloat();
                for (int i = 0; i < 1; i++)
                {
                    height = targetPos.y + (float)i * spread;
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
                    Handles.DrawPolyLine(lines);
                }
                for (int i = 0; i < 1; i++)
                {
                    height = targetPos.y + (float)i * spread + Height.longValue.ToFloat();
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
                    Handles.DrawPolyLine(lines);
                }

                xModifier = 0;//halfWidth;
            } else if (shape == ColliderType.Polygon)
            {
                float yRot = transform.eulerAngles.y * Mathf.Deg2Rad;

                Vector2d rotation = Vector2d.CreateRotation(yRot);
                bool changed = false;


                Vector3[] draws = new Vector3[Body.Vertices.Length + 1];
                    
                for (int i = 0; i < Body.Vertices.Length; i++)
                {
                    Vector2d vertex = Body.Vertices [i];
                    vertex.Rotate(rotation.x, rotation.y);
                    Vector3 drawPos = vertex.ToVector3() + targetPos;
                    Vector3 newDrawPos = Handles.FreeMoveHandle(drawPos, Quaternion.identity, MoveHandleSize, new Vector3(0, float.PositiveInfinity, 0), Handles.SphereCap);
                    if ((newDrawPos - (drawPos)).magnitude >= .01f)
                    {
                        newDrawPos -= targetPos;
                        vertex = new Vector2d(newDrawPos);
                        vertex.RotateInverse(rotation.x, rotation.y);
                        Body.Vertices [i] = vertex;
                        changed = true;
                    }
                    draws[i] = drawPos;
                    Handles.Label(drawPos, "V: " + i.ToString(),LabelStyle);
                }
                if (Body.Vertices.Length > 0) {
                    draws[draws.Length - 1] = draws[0];
                    Handles.DrawPolyLine(draws);
                    for (int i = 0; i < draws.Length; i++)
                    {
                        Vector3 highPos = draws[i];
                        highPos.y += Body.Height.ToFloat();
                        Handles.DrawLine(draws[i],highPos);
                        draws[i] = highPos;
                    }
                    Handles.DrawPolyLine(draws);
                }
                if (changed)
                    so.Update();
            }


            Handles.DrawLine(
                new Vector3(targetPos.x + xModifier, targetPos.y, targetPos.z), 
                new Vector3(targetPos.x + xModifier, targetPos.y + Height.longValue.ToFloat(), targetPos.z));

            Vector3 movePos = targetPos;
            movePos.x += xModifier;
            movePos.y += (float)Height.longValue.ToFormattedDouble();
            Vector3 lastMovePos = movePos;
            movePos = 
                Handles.FreeMoveHandle(
                movePos,
                Quaternion.identity,
                MoveHandleSize,
                Vector3.zero,
                dragCap
            );
            if ((lastMovePos- movePos).sqrMagnitude >= .1f)
            Height.longValue = FixedMath.Create(Mathf.Max(Mathf.Abs(movePos.y - targetPos.y)));
            so.ApplyModifiedProperties();
        }
    }

    internal static class SerializedPropertyDraw
    {
        public static void Draw(this SerializedProperty prop)
        {
            if (prop.hasMultipleDifferentValues) {
                EditorGUILayout.HelpBox("Fields with different values not multi-editable", UnityEditor.MessageType.None);
                //TODO: Throw something like what default inspectors do
                return;
            }
            EditorGUILayout.PropertyField(prop, true);
        }
    }
}
#endif