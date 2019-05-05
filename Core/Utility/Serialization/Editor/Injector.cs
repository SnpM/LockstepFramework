using UnityEngine;
using UnityEditor;

namespace Lockstep
{
	public static class Injector
	{
		public static void SetTarget(UnityEngine.Object target)
		{
			Target = target;
		}

		public static Object Target { get; private set; }
		static SerializedObject so;
		public static SerializedProperty GetProperty(string name)
		{
			so = new SerializedObject(Target);
			SerializedProperty prop = so.FindProperty(name);
			return prop;
		}
		public static void Apply()
		{
			so.ApplyModifiedProperties();
			EditorUtility.SetDirty(Target);
		}
		public static void SetField(string name, float value, FieldType fieldType)
		{
			SerializedProperty prop = GetProperty(name);
			switch (fieldType)
			{
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
			Apply();
		}

		public static float GetField(string name, FieldType fieldType)
		{
			SerializedProperty prop = GetProperty(name);
			switch (fieldType)
			{
				case FieldType.FixedNumber:
					return prop.longValue.ToFloat();
				//break;
				case FieldType.Interval:
					return prop.intValue / (float)LockstepManager.FrameRate;
				//break;
				case FieldType.Rate:
					return 1 / (prop.intValue / (float)LockstepManager.FrameRate);
					//break;
			}
			return 0;
		}
		public static void SetVector3(string name, Vector3 value)
		{
			SerializedProperty prop = GetProperty(name);
			Vector3d vec = new Vector3d(value);
			prop.FindPropertyRelative("x").longValue = vec.x;
			prop.FindPropertyRelative("y").longValue = vec.y;
			prop.FindPropertyRelative("z").longValue = vec.z;
			Apply();
		}

		public static Vector3 GetVector3(string name)
		{
			SerializedProperty prop = GetProperty(name);
			Vector3d vec = new Vector3d(
				prop.FindPropertyRelative("x").longValue,
				prop.FindPropertyRelative("y").longValue,
				prop.FindPropertyRelative("z").longValue);
			return vec.ToVector3();
		}

	}
	public enum FieldType
	{
		FixedNumber,
		Interval,
		Rate,

	}
}