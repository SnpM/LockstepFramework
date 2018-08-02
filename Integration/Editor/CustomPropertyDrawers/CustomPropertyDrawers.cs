#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Lockstep
{
	namespace Lockstep
	{
		[CustomPropertyDrawer(typeof(FixedNumberAttribute))]
		public class EditorFixedNumber : PropertyDrawer
		{
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				FixedNumberAttribute a = ((FixedNumberAttribute)this.attribute);
				long orgValue = property.longValue;
				long value = orgValue;
				LSEditorUtility.DoubleField(
					position,
					label,
					ref value,
					a.Timescaled
				);
				if (a.Ranged)
				{
					if (value > a.Max)
						value = a.Max;
					else if (value < a.Min)
						value = a.Min;
				}
				if (orgValue != value)
					property.longValue = value;
			}
		}

		[CustomPropertyDrawer(typeof(FixedNumberAngleAttribute))]
		public class EditorFixedNumberAngle : PropertyDrawer
		{
			private const double Pi = 3.14159265359d;
			private const double RadToDeg = 180d / Pi;
			private const double DegToRad = Pi / 180d;

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				double scale = ((FixedNumberAngleAttribute)this.attribute).Timescaled ? LockstepManager.FrameRate : 1;
				double angle = Math.Round(Math.Asin(property.longValue.ToDouble()) * RadToDeg, 2, MidpointRounding.AwayFromZero);

				angle = EditorGUI.DoubleField(position, label, angle * scale) / scale;
				double max = ((FixedNumberAngleAttribute)this.attribute).Max;
				if (max > 0 && angle > max)
					angle = max;
				property.longValue = FixedMath.Create(Math.Sin(DegToRad * angle));
			}

		}

		[CustomPropertyDrawer(typeof(FrameCountAttribute))]
		public class EditorFrameCount : PropertyDrawer
		{
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				FrameCountAttribute a = attribute as FrameCountAttribute;
				if (a.IsRate == false)
				{

					int set =
						(int)(EditorGUI.DoubleField(position, label,
							property.intValue / (double)LockstepManager.FrameRate) * LockstepManager.FrameRate);
					if (property.intValue != set)
						property.intValue = set;

				}
				else
				{

					double showVal = 1d / (property.intValue / (double)LockstepManager.FrameRate);
					showVal = EditorGUI.DoubleField(position, label, showVal);

					int set = (int)((1d / showVal) * LockstepManager.FrameRate);
					if (property.intValue != set)
						property.intValue = set;
				}

			}
		}

		[CustomPropertyDrawer(typeof(Vector2d))]
		public class EditorVector2d : PropertyDrawer
		{
			//Thanks stranger! (https://gamedev.stackexchange.com/a/123609/57906)

			SerializedProperty X, Y;
			string name;
			bool cache = false;

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				if (!cache)
				{
					//get the name before it's gone
					name = property.displayName;

					//get the X and Y values
					property.Next(true);
					X = property.Copy();
					property.Next(true);
					Y = property.Copy();

					cache = false;
				}
				bool changed = false;
				Rect contentPosition = EditorGUI.PrefixLabel(position, new GUIContent(name));

				//Check if there is enough space to put the name on the same line (to save space)
				if (position.height > 16f)
				{
					position.height = 16f;
					EditorGUI.indentLevel += 1;
					contentPosition = EditorGUI.IndentedRect(position);
					contentPosition.y += 18f;
				}

				float half = contentPosition.width / 2;
				GUI.skin.label.padding = new RectOffset(3, 3, 6, 6);

				//show the X and Y from the point
				EditorGUIUtility.labelWidth = 14f;
				contentPosition.width *= 0.5f;
				EditorGUI.indentLevel = 0;

				// Begin/end property & change check make each field
				// behave correctly when multi-object editing.
				EditorGUI.BeginProperty(contentPosition, label, X);
				{
					EditorGUI.BeginChangeCheck();
					var newVal = FixedNumberField(contentPosition, new GUIContent("X"), X.longValue);
					if (EditorGUI.EndChangeCheck())
					{
						X.longValue = newVal;
						changed = true;
					}
				}
				EditorGUI.EndProperty();

				contentPosition.x += half;

				EditorGUI.BeginProperty(contentPosition, label, Y);
				{
					EditorGUI.BeginChangeCheck();
					var newVal = FixedNumberField(contentPosition, new GUIContent("Y"), Y.longValue);
					if (EditorGUI.EndChangeCheck())
					{
						Y.longValue = newVal;
						changed = true;
					}
				}
				EditorGUI.EndProperty();

			}

			long FixedNumberField(Rect position, GUIContent label, long value)
			{
				return FixedMath.Create(EditorGUI.DoubleField(position, label, value.ToDouble()));
			}
		}

		[CustomPropertyDrawer(typeof(VectorRotationAttribute))]
		public class EditorVectorRotation : PropertyDrawer
		{
			float height = 0f;

			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				float h = height;
				return h;
			}

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				VectorRotationAttribute at = this.attribute as VectorRotationAttribute;
				bool timescaled = at.Timescaled;
				double scale = LSEditorUtility.Scale(timescaled);
				SerializedProperty x = property.FindPropertyRelative("x");
				SerializedProperty y = property.FindPropertyRelative("y");

				double angleInRadians = Math.Atan2(y.longValue.ToDouble(), x.longValue.ToDouble());

				double angleInDegrees = (angleInRadians * 180d / Math.PI) * scale;
				height = 15f;
				position.height = height;
				angleInDegrees = (EditorGUI.DoubleField(position, "Angle", angleInDegrees)) / scale;

				double newAngleInRadians = angleInDegrees * Math.PI / 180d;
				if (Math.Abs(newAngleInRadians - angleInRadians) >= .001f)
				{
					long cos = FixedMath.Create(Math.Cos(newAngleInRadians));
					long sin = FixedMath.Create(Math.Sin(newAngleInRadians));
					x.longValue = cos;
					y.longValue = sin;
				}
			}
		}



		/// <summary>
		/// Adds scene handle to adjust position.
		/// </summary>
		public class EditorVisualize : LSScenePropertyDrawer
		{
			public override Type TargetType
			{
				get
				{
					return typeof(VisualizeAttribute);
				}
			}

			public override void OnSceneGUI(CerealBehaviour source, SerializedProperty property, GUIContent label)
			{
				switch (property.propertyType)
				{
					case SerializedPropertyType.Vector3:
						property.vector3Value = Handles.PositionHandle(property.vector3Value, Quaternion.identity);
						property.serializedObject.ApplyModifiedProperties();
						Handles.Label(property.vector3Value, label);
						break;
					default:
						throw new System.ArgumentException(string.Format("The visualization behavior of Type {0} is not implemented", property.propertyType));
						//break;
				}
			}

			public override void OnDrawGizmos(CerealBehaviour source, SerializedProperty property, GUIContent label)
			{
				switch (property.propertyType)
				{
					case SerializedPropertyType.Vector3:
						Gizmos.color = Color.blue;
						Gizmos.DrawSphere(property.vector3Value, .5f);
						break;
					default:
						throw new System.ArgumentException(string.Format("The visualization behavior of Type {0} is not implemented", property.propertyType));
						//break;
				}
			}
		}

		[CustomPropertyDrawer(typeof(EnumMaskAttribute))]
		public class EditorEnumMask : PropertyDrawer
		{
			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				return 15f;
			}

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				Enum mask = null;
				Type enumType = fieldInfo.FieldType;
				Type underlyingType = Enum.GetUnderlyingType(enumType);
				if (enumType.IsEnum == false)
				{
					base.OnGUI(position, property, label);
					return;
				}
				bool isLong = underlyingType == typeof(long);
				if (isLong)
				{
					if (property.longValue == -1)
					{
						property.longValue = 0;
					}
					mask = (Enum)Enum.ToObject(enumType, property.longValue);
				}
				else
				{
					if (property.intValue == -1)
					{
						property.intValue = ~0;
					}
					mask = (Enum)Enum.ToObject(enumType, property.intValue);
				}

				mask = EditorGUI.EnumMaskPopup(position, label, mask);

				if (isLong)
				{
					property.longValue = Convert.ToInt64(mask);
				}
				else
				{
					property.intValue = Convert.ToInt32(mask);
				}
			}
		}

	}
}
#endif