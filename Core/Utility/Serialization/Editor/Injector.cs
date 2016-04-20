using UnityEngine;
using System.Collections;
using UnityEditor;
namespace Lockstep
{
    public static class Injector
    {
        public static void SetTarget (UnityEngine.Object target) {
            Target = target;
        }

        public static Object Target{ get; private set;}
        public static SerializedProperty GetProperty (string name) {
            SerializedObject so = new SerializedObject (Target);
            SerializedProperty prop = so.FindProperty(name);
            return prop;
        }
        public static void SetField (string name, float value, FieldType fieldType) {
            SerializedProperty prop = GetProperty (name);
            switch (fieldType) {
                case FieldType.FixedNumber:
                    prop.longValue = FixedMath.Create(value);
                    break;
                case FieldType.Interval:
                    prop.intValue = Mathf.RoundToInt(value * LockstepManager.FrameRate);
                    break;
                case FieldType.Rate:
                    prop.intValue = Mathf.RoundToInt((1 / value) * LockstepManager.FrameRate);
                    break;
            }
        }

        public static float GetField (string name, FieldType fieldType) {
            SerializedProperty prop = GetProperty (name);
            switch (fieldType) {
                case FieldType.FixedNumber:
                    return prop.longValue.ToFloat();
                    break;
                case FieldType.Interval:
                    return prop.intValue / (float) LockstepManager.FrameRate;
                    break;
                case FieldType.Rate:
                    return 1 / (prop.intValue / (float)LockstepManager.FrameRate);
                    break;
            }
            return 0;
        }

    }
    public enum FieldType {
        FixedNumber,
        Interval,
        Rate,

    }
}