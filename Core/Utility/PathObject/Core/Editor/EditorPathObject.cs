using UnityEngine;
using UnityEditor;

namespace Lockstep
{
	[CustomPropertyDrawer(typeof(PathObjectAttribute))]
	public class EditorPathObject : UnityEditor.PropertyDrawer
	{
		const float defaultPropertyHeight = 16f;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return defaultPropertyHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			PathObjectAttribute Atb = attribute as PathObjectAttribute;
			SerializedProperty prefabNameProp = property.FindPropertyRelative("_path");
			//SerializedProperty prefabProp = property.FindPropertyRelative ("_editorPrefab");
			//string lastName = prefabNameProp.stringValue;
			UnityEngine.Object obj = PathObjectFactory.Load(prefabNameProp.stringValue) as UnityEngine.Object;
			obj = (UnityEngine.Object)EditorGUI.ObjectField(position, label, obj, Atb.ObjectType, false);
			string relativePath = "";
			if (obj != null)
			{
				if (PathObjectUtility.TryGetPath(obj, out relativePath) == false)
				{
					Debug.LogErrorFormat("Object '{0}' is not detected to be in a Resources folder.", obj);
					return;
				}
			}
			prefabNameProp.stringValue = obj != null ? relativePath : "";

			/*if (lastName != prefabNameProp.stringValue) {
            if (PathPrefabFactory.Load (prefabNameProp.stringValue) == null) {
                prefabProp.objectReferenceValue = null;
            }
        }*/
		}
	}
}