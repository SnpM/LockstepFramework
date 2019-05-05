#if true
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace Lockstep
{
	[UnityEditor.CanEditMultipleObjects]
	[CustomEditor(typeof(CerealBehaviour), true)]
	public sealed class EditorCerealBehaviour : UnityEditor.Editor
	{
		//static Dictionary<CerealBehaviour,EditorCerealBehaviour> instances = new Dictionary<CerealBehaviour,EditorCerealBehaviour>();
		private bool inited = false;
		bool useCustomFieldNames;
		List<string> customFieldNames = new List<string>();
		SerializedObject _so;

		SerializedObject so { get { return _so ?? (_so = new SerializedObject(target)); } }
		CerealBehaviour _cereal;
		CerealBehaviour cereal { get { return _cereal ?? (_cereal = (CerealBehaviour)target); } }

		private void Init()
		{
			_Init();

		}
		private void _Init()
		{
			useCustomFieldNames = ((CerealBehaviour)target).GetSerializedFieldNames(customFieldNames);
			inited = true;
		}

		public override void OnInspectorGUI()
		{
			if (inited == false)
				Init();
			cereal.BeforeSerialize();
			cereal.BeforeGUI();
			so.Update();
			so.ApplyModifiedProperties();

			foreach (SerializedProperty p in properties())
			{
				DrawProperty(p);
			}
			so.ApplyModifiedProperties();

			cereal.AfterSerialize();
			cereal.AfterGUI();

			so.Update();
		}

		private IEnumerable<SerializedProperty> properties()
		{
			if (useCustomFieldNames == false)
			{
				SerializedProperty iterator = so.GetIterator();
				if (iterator.NextVisible(true))
					while (iterator.NextVisible(false))
					{
						yield return iterator;
					}
			}
			else
			{
				for (int i = 0; i < customFieldNames.Count; i++)
				{
					SerializedProperty p = so.FindProperty(customFieldNames[i]);
					yield return p;
				}
			}
		}

		private static IEnumerable<LSScenePropertyDrawer> GetPropertyDrawers(SerializedProperty p)
		{
			Type propertyObjectType = LSEditorUtility.GetPropertyType(p);
			if (propertyObjectType == null)
				yield break;
			LSScenePropertyDrawer drawer = LSScenePropertyDrawer.GetDrawer(propertyObjectType);
			yield return drawer;
			IEnumerable<PropertyAttribute> propertyAttributes = LSEditorUtility.GetPropertyAttributes<PropertyAttribute>(p);
			foreach (PropertyAttribute propertyAttribute in propertyAttributes)
			{
				drawer = LSScenePropertyDrawer.GetDrawer(propertyAttribute.GetType());
				yield return drawer;
			}

		}


		private void OnSceneGUI()
		{
			CerealBehaviour cereal = (CerealBehaviour)target;
			cereal.BeforeScene();
			cereal.BeforeSerialize();
			so.Update();
			foreach (SerializedProperty p in properties())
			{
				foreach (LSScenePropertyDrawer drawer in GetPropertyDrawers(p))
				{
					if (drawer != null)
						drawer.OnSceneGUI(cereal, p, new GUIContent(p.displayName, p.tooltip));
				}
			}
			so.ApplyModifiedProperties();
			cereal.AfterSerialize();
			cereal.AfterScene();
			so.Update();
		}

		/*
        [DrawGizmo((GizmoType)~0, typeof (CerealBehaviour))]
        private static void GizmoWrapper (CerealBehaviour behaviour, GizmoType gizmoType) 
        {
            foreach (EditorCerealBehaviour editor in instances.Values)
            {

                    editor.DrawGizmo();
            }
        }
        private void DrawGizmo () {
            foreach (SerializedProperty p in properties ())
            {
                foreach (LSScenePropertyDrawer drawer in GetPropertyDrawers (p))
                {
                    if (drawer != null)
                        drawer.OnDrawGizmos(p, new GUIContent(p.displayName, p.tooltip));
                }
            }

        }*/

		private static void DrawProperty(SerializedProperty property)
		{
			EditorGUILayout.PropertyField(property, true);
		}


	}

}

#endif