using UnityEngine;
using System.Collections; using FastCollections;
using UnityEditor;

namespace Lockstep.Data {
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
                EditorGUI.LabelField (drawPos, "No attribute found");
            } else if (EditorLSDatabaseWindow.Window == null || EditorLSDatabaseWindow.Window.IsLoaded == false) {
                if (GUI.Button (drawPos, new GUIContent ("No database loaded", "Click to open the database manager and load a database."))) {
                    if (EditorLSDatabaseWindow.Window == null) {
                        EditorLSDatabaseWindow.Menu ();
                    } else {
                        EditorLSDatabaseWindow.Window.Show ();
                    }
                }
            } else {
                DataHelper helper;
                if (!EditorLSDatabaseWindow.Window.DatabaseEditor.DataHelpers.TryGetValue (dca.TargetDataName, out helper)) {
                    Debug.LogError("Data code '" + dca.TargetDataName + "' was not found in the current database");
                }
                else {
                    DataItem[] data = helper.Data;
                    GUIContent[] dataContents = new GUIContent[data.Length];
                    if (property.isArray && property.type != "string") {
                        int arraySize = property.arraySize;
                        arraySize = EditorGUI.IntField (drawPos, "Size", arraySize);
                        property.arraySize = arraySize;
                        for (int n = 0; n < arraySize; n++) {
                            SerializedProperty element = property.GetArrayElementAtIndex (n);
                    
                            string curName = element.stringValue;
                            int index = -1;
                            for (int i = 0; i < data.Length; i++) {
                                string name = data [i].Name;
                                if (curName.Equals (name)) {
                                    index = i;
                                }
                        
                                GUIContent content = new GUIContent (name);
                                dataContents [i] = content;
                            }
                            //label.tooltip += dca.TargetDataName;
                            int newIndex = EditorGUI.Popup (drawPos, label, index, dataContents);
                            if (newIndex >= 0 && newIndex < dataContents.Length) {
                                element.stringValue = dataContents [newIndex].text;
                            }
                        }
                    
                    } else {
                
                        string curName = property.stringValue;
                        int index = -1;
                        for (int i = 0; i < data.Length; i++) {
                            string name = data [i].Name;
                            if (curName.Equals (name)) {
                                index = i;
                            }
                    
                            GUIContent content = new GUIContent (name);
                            dataContents [i] = content;
                        }
                        //label.tooltip += dca.TargetDataName;
                        int newIndex = EditorGUI.Popup (drawPos, label, index, dataContents);
                        if (newIndex >= 0 && newIndex < dataContents.Length) {
                            property.stringValue = dataContents [newIndex].text;
                        }
                    }
                }
            }
            accumulatedHeight = drawPos.yMax - position.yMin;
        }
    }
}