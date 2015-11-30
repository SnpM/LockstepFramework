#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Lockstep.Integration;
using System.Collections.Generic;
using Lockstep.Data;
namespace Lockstep {
    [CustomPropertyDrawer(typeof(FixedNumberAttribute))]
    public class EditorFixedNumber : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            long value = property.longValue;
            LSEditorUtility.DoubleField (
                position,
                label,
                ref value,
                ((FixedNumberAttribute)this.attribute).Timescaled
            );
            property.longValue = value;
        }
    }

    [CustomPropertyDrawer(typeof(FixedNumberAngleAttribute))]
    public class EditorFixedNumberAngle : PropertyDrawer {
        private const double Pi = 3.14159265359d;
        private const double RadToDeg = 180d / Pi;
        private const double DegToRad = Pi / 180d;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            double scale = ((FixedNumberAngleAttribute)this.attribute).Timescaled ? LockstepManager.FrameRate : 1;
            double angle = Math.Round(Math.Asin(property.longValue.ToDouble()) * RadToDeg, 2, MidpointRounding.AwayFromZero);

            angle = EditorGUI.DoubleField (position, label, angle * scale) / scale;
            double max = ((FixedNumberAngleAttribute)this.attribute).Max;
            if (max > 0 && angle > max) angle = max;
            property.longValue = FixedMath.Create(Math.Sin(DegToRad * angle));
        }

    }

    [CustomPropertyDrawer(typeof(FrameCountAttribute))]
    public class EditorFrameCount : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            property.intValue =
                (int)(EditorGUI.DoubleField (position, label,
                                       property.intValue / (double)LockstepManager.FrameRate) * LockstepManager.FrameRate);
        }
    }

    [CustomPropertyDrawer (typeof (VectorRotation))]
    public class EditorVectorAngle : PropertyDrawer {
        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {

            SerializedProperty angle = property.FindPropertyRelative("_angle");
            SerializedProperty timescaled = property.FindPropertyRelative ("_timescaled");
            double scale = LSEditorUtility.Scale (timescaled.boolValue);
            angle.doubleValue = EditorGUILayout.DoubleField (label, angle.doubleValue * scale) / scale;
            timescaled.boolValue = EditorGUILayout.Toggle ("Timescaled", timescaled.boolValue);

            double angleInRadians = angle.doubleValue * Math.PI / 180d;
            property.FindPropertyRelative ("_cos").longValue = FixedMath.Create (Math.Cos(angleInRadians));
            property.FindPropertyRelative ("_sin").longValue = FixedMath.Create (Math.Sin(angleInRadians));
        }
    }

    [CustomPropertyDrawer (typeof (HideInInspectorGUI))]
    public class EditorHideInInspectorGUI : PropertyDrawer{
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

        }
    }

    public class EditorVisualize : LSScenePropertyDrawer{
        public override Type TargetType
        {
            get
            {
                return typeof (VisualizeAttribute);
            }
        }
        public override void OnSceneGUI(CerealBehaviour source, SerializedProperty property, GUIContent label)
        {
            switch (property.propertyType) {
                case SerializedPropertyType.Vector3:
                    property.vector3Value = Handles.PositionHandle (property.vector3Value, Quaternion.identity);
                    property.serializedObject.ApplyModifiedProperties();
                    Handles.Label(property.vector3Value, label);
                    break;
                default:
                    throw new System.ArgumentException(string.Format("The visualization behavior of Type {0} is not implemented", property.propertyType));
                    break;
            }
        }
        public override void OnDrawGizmos (CerealBehaviour source, SerializedProperty property, GUIContent label) {
            switch (property.propertyType) {
                case SerializedPropertyType.Vector3:
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(property.vector3Value, .5f);
                    break;
                default:
                    throw new System.ArgumentException(string.Format("The visualization behavior of Type {0} is not implemented", property.propertyType));
                    break;
            }
        }
    }

    [CustomPropertyDrawer(typeof(DataCodeAttribute))]
    public class EditorDataCode : PropertyDrawer {
        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return accumulatedHeight;
        }
        float accumulatedHeight = 15f;
        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            float height = 15f;
            Rect drawPos = position;
            drawPos.yMax = drawPos.yMin + height;
            DataCodeAttribute dca = this.attribute as DataCodeAttribute;
            if (dca == null) {
                EditorGUI.LabelField (drawPos,"No attribute found");
            }
            else if (EditorLSDatabaseWindow.Window == null || EditorLSDatabaseWindow.Window.IsLoaded == false) {
                EditorGUI.LabelField (drawPos,"No database loaded.");
            }
            else {
                DataHelper helper;
                EditorLSDatabaseWindow.Window.DatabaseEditor.DataHelpers.TryGetValue (dca.TargetDataName, out helper);
                DataItem[] data = helper.Data;
                GUIContent[] dataContents = new GUIContent[data.Length];
                if (property.isArray && property.type != "string") {
                    int arraySize = property.arraySize;
                    arraySize = EditorGUI.IntField (drawPos,"Size",arraySize);
                    property.arraySize = arraySize;
                    for (int n = 0; n < arraySize; n++) {
                        SerializedProperty element = property.GetArrayElementAtIndex (n);

                        Debug.Log (element.type);
                        string curName = element.stringValue;
                        int index = -1;
                        for (int i = 0; i < data.Length; i++) {
                            string name = data[i].Name;
                            if (curName.Equals (name))
                                index = i;
                            
                            GUIContent content = new GUIContent (name);
                            dataContents[i] = content;
                        }
                        label.tooltip += dca.TargetDataName;
                        int newIndex = EditorGUI.Popup (drawPos,label,index,dataContents);
                        if (newIndex >= 0 && newIndex < dataContents.Length) {
                            element.stringValue = dataContents[newIndex].text;
                        }
                    }
                }
                else {

                    string curName = property.stringValue;
                    int index = -1;
                    for (int i = 0; i < data.Length; i++) {
                        string name = data[i].Name;
                        if (curName.Equals (name))
                            index = i;

                        GUIContent content = new GUIContent (name);
                        dataContents[i] = content;
                    }
                    label.tooltip += dca.TargetDataName;
                    int newIndex = EditorGUI.Popup (drawPos,label,index,dataContents);
                    if (newIndex >= 0 && newIndex < dataContents.Length) {
                        property.stringValue = dataContents[newIndex].text;
                    }
                }
            }
            accumulatedHeight = drawPos.yMax - position.yMin;
        }
    }

}

#endif