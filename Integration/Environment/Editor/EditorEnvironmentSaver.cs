using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Lockstep
{
    [CustomEditor(typeof (EnvironmentSaver))]
    public class EditorEnvironmentSaver : Editor
    {

        public override void OnInspectorGUI()
        {
            EnvironmentSaver saver = this.target as EnvironmentSaver;
            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Scan and Save")) {
                saver.ScanAndSave();
                EditorUtility.SetDirty(target);
                serializedObject.Update();
            }

        }

    }
}