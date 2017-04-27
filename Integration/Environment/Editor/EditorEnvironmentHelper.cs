using UnityEngine;
using System.Collections; using FastCollections;
using UnityEditor;

namespace Lockstep
{
    [CustomEditor(typeof (EnvironmentHelper))]
    public class EditorEnvironmentHelper : Editor
    {

        public override void OnInspectorGUI()
        {
            SerializedProperty saverObjectProperty = serializedObject.FindProperty("_saverObject");
            EditorGUILayout.PropertyField(saverObjectProperty);

            serializedObject.ApplyModifiedProperties();

            EnvironmentHelper saver = this.target as EnvironmentHelper;
            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Scan and Save")) {
                
                saver.ScanAndSave();

			}

        }

    }
}