// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TypeReferences {

	/// <summary>
	/// Custom property drawer for <see cref="ClassTypeReference"/> properties.
	/// </summary>
	[CustomPropertyDrawer(typeof(ClassTypeReference))]
	[CustomPropertyDrawer(typeof(ClassTypeConstraintAttribute), true)]
	public sealed class ClassTypeReferencePropertyDrawer : PropertyDrawer {

		#region Type Filtering

		/// <summary>
		/// Gets or sets a function that returns a collection of types that are
		/// to be excluded from drop-down. A value of <c>null</c> specifies that
		/// no types are to be excluded.
		/// </summary>
		/// <remarks>
		/// <para>This property must be set immediately before presenting a class
		/// type reference property field using <see cref="EditorGUI.PropertyField"/>
		/// or <see cref="EditorGUILayout.PropertyField"/> since the value of this
		/// property is reset to <c>null</c> each time the control is drawn.</para>
		/// <para>Since filtering makes extensive use of <see cref="ICollection{Type}.Contains"/>
		/// it is recommended to use a collection that is optimized for fast
		/// lookups such as <see cref="HashSet{Type}"/> for better performance.</para>
		/// </remarks>
		/// <example>
		/// <para>Exclude a specific type from being selected:</para>
		/// <code language="csharp"><![CDATA[
		/// private SerializedProperty _someClassTypeReferenceProperty;
		/// 
		/// public override void OnInspectorGUI() {
		///     serializedObject.Update();
		/// 
		///     ClassTypeReferencePropertyDrawer.ExcludedTypeCollectionGetter = GetExcludedTypeCollection;
		///     EditorGUILayout.PropertyField(_someClassTypeReferenceProperty);
		/// 
		///     serializedObject.ApplyModifiedProperties();
		/// }
		/// 
		/// private ICollection<Type> GetExcludedTypeCollection() {
		///     var set = new HashSet<Type>();
		///     set.Add(typeof(SpecialClassToHideInDropdown));
		///     return set;
		/// }
		/// ]]></code>
		/// </example>
		public static Func<ICollection<Type>> ExcludedTypeCollectionGetter { get; set; }

		private static List<Type> GetFilteredTypes(ClassTypeConstraintAttribute filter) {
			var types = new List<Type>();

			var excludedTypes = (ExcludedTypeCollectionGetter != null ? ExcludedTypeCollectionGetter() : null);

			var assembly = Assembly.GetExecutingAssembly();
			FilterTypes(assembly, filter, excludedTypes, types);

			foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
				FilterTypes(Assembly.Load(referencedAssembly), filter, excludedTypes, types);

			types.Sort((a, b) => a.FullName.CompareTo(b.FullName));

			return types;
		}

		private static void FilterTypes(Assembly assembly, ClassTypeConstraintAttribute filter, ICollection<Type> excludedTypes, List<Type> output) {
			foreach (var type in assembly.GetTypes()) {
				if (!type.IsPublic || !type.IsClass)
					continue;

				if (filter != null && !filter.IsConstraintSatisfied(type))
					continue;

				if (excludedTypes != null && excludedTypes.Contains(type))
					continue;

				output.Add(type);
			}
		}

		#endregion

		#region Type Utility

		private static Dictionary<string, Type> s_TypeMap = new Dictionary<string, Type>();

		private static Type ResolveType(string classRef) {
			Type type;
			if (!s_TypeMap.TryGetValue(classRef, out type)) {
				type = !string.IsNullOrEmpty(classRef) ? Type.GetType(classRef) : null;
				s_TypeMap[classRef] = type;
			}
			return type;
		}

		#endregion

		#region Control Drawing / Event Handling

		private static readonly int s_ControlHint = typeof(ClassTypeReferencePropertyDrawer).GetHashCode();
		private static GUIContent s_TempContent = new GUIContent();

		private static string DrawTypeSelectionControl(Rect position, GUIContent label, string classRef, ClassTypeConstraintAttribute filter) {
			if (label != null && label != GUIContent.none)
				position = EditorGUI.PrefixLabel(position, label);

			int controlID = GUIUtility.GetControlID(s_ControlHint, FocusType.Keyboard, position);

			bool triggerDropDown = false;

			switch (Event.current.GetTypeForControl(controlID)) {
				case EventType.ExecuteCommand:
					if (Event.current.commandName == "TypeReferenceUpdated") {
						if (s_SelectionControlID == controlID) {
							if (classRef != s_SelectedClassRef) {
								classRef = s_SelectedClassRef;
								GUI.changed = true;
							}

							s_SelectionControlID = 0;
							s_SelectedClassRef = null;
						}
					}
					break;

				case EventType.MouseDown:
					if (GUI.enabled && position.Contains(Event.current.mousePosition)) {
						GUIUtility.keyboardControl = controlID;
						triggerDropDown = true;
						Event.current.Use();
					}
					break;

				case EventType.KeyDown:
					if (GUI.enabled && GUIUtility.keyboardControl == controlID) {
						if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Space) {
							triggerDropDown = true;
							Event.current.Use();
						}
					}
					break;

				case EventType.Repaint:
					// Remove assembly name from content of popup control.
					var classRefParts = classRef.Split(',');

					s_TempContent.text = classRefParts[0].Trim();
					if (s_TempContent.text == "")
						s_TempContent.text = "(None)";
					else if (ResolveType(classRef) == null)
						s_TempContent.text += " {Missing}";

					EditorStyles.popup.Draw(position, s_TempContent, controlID);
					break;
			}

			if (triggerDropDown) {
				s_SelectionControlID = controlID;
				s_SelectedClassRef = classRef;
	
				var filteredTypes = GetFilteredTypes(filter);
				DisplayDropDown(position, filteredTypes, ResolveType(classRef), filter.Grouping);
			}

			return classRef;
		}

		private static void DrawTypeSelectionControl(Rect position, SerializedProperty property, GUIContent label, ClassTypeConstraintAttribute filter) {
			try {
				bool restoreShowMixedValue = EditorGUI.showMixedValue;
				EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

				property.stringValue = DrawTypeSelectionControl(position, label, property.stringValue, filter);

				EditorGUI.showMixedValue = restoreShowMixedValue;
			}
			finally {
				ExcludedTypeCollectionGetter = null;
			}
		}

		private static void DisplayDropDown(Rect position, List<Type> types, Type selectedType, ClassGrouping grouping) {
			var menu = new GenericMenu();

			menu.AddItem(new GUIContent("(None)"), selectedType == null, s_OnSelectedTypeName, null);
			menu.AddSeparator("");

			for (int i = 0; i < types.Count; ++i) {
				var type = types[i];

				string menuLabel = FormatGroupedTypeName(type, grouping);
				if (string.IsNullOrEmpty(menuLabel))
					continue;

				var content = new GUIContent(menuLabel);
				menu.AddItem(content, type == selectedType, s_OnSelectedTypeName, type);
			}

			menu.DropDown(position);
		}

		private static string FormatGroupedTypeName(Type type, ClassGrouping grouping) {
			string name = type.FullName;

			switch (grouping) {
				default:
				case ClassGrouping.None:
					return name;

				case ClassGrouping.ByNamespace:
					return name.Replace('.', '/');

				case ClassGrouping.ByNamespaceFlat:
					int lastPeriodIndex = name.LastIndexOf('.');
					if (lastPeriodIndex != -1)
						name = name.Substring(0, lastPeriodIndex) + "/" + name.Substring(lastPeriodIndex + 1);

					return name;

				case ClassGrouping.ByAddComponentMenu:
					var addComponentMenuAttributes = type.GetCustomAttributes(typeof(AddComponentMenu), false);
					if (addComponentMenuAttributes.Length == 1)
						return ((AddComponentMenu)addComponentMenuAttributes[0]).componentMenu;

					return "Scripts/" + type.FullName.Replace('.', '/');
			}
		}

		private static int s_SelectionControlID;
		private static string s_SelectedClassRef;

		private static readonly GenericMenu.MenuFunction2 s_OnSelectedTypeName = OnSelectedTypeName;

		private static void OnSelectedTypeName(object userData) {
			var selectedType = userData as Type;

			s_SelectedClassRef = ClassTypeReference.GetClassRef(selectedType);
			
			var typeReferenceUpdatedEvent = EditorGUIUtility.CommandEvent("TypeReferenceUpdated");
			EditorWindow.focusedWindow.SendEvent(typeReferenceUpdatedEvent);
		}

		#endregion

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorStyles.popup.CalcHeight(GUIContent.none, 0);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			DrawTypeSelectionControl(position, property.FindPropertyRelative("_classRef"), label, attribute as ClassTypeConstraintAttribute);
		}

	}

}
