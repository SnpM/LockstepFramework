#if true
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;

//TODO: Optimize GetPropertyHeight
namespace Lockstep.Data {
    [CustomPropertyDrawer (typeof(DataItem), true)]
    public class EditorDataItem : PropertyDrawer {
        bool initialized = false;

        private void Initialize () {
            initialized = true;
            OnInitialize ();
        }

        protected  void OnInitialize () {
        
        }

        const float defaultPropertyHeight = 16;
    
        public sealed override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            float height = LSEditorUtility.GetPersistentValue (defaultPropertyHeight, property.propertyPath);
            return height;
        }

        private List<SerializedProperty> serializedProperties = new List<SerializedProperty> ();
        const BindingFlags OpenBinding = System.Reflection.BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        DataItemAttribute dataItemAttribute;
    
        public sealed override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        
            if (initialized == false) {
                Initialize ();
            }
        
            dataItemAttribute =
            dataItemAttribute
                ?? (DataItemAttribute)Attribute.GetCustomAttribute (LSEditorUtility.GetPropertyType (property), typeof(DataItemAttribute), false)
                ?? new DataItemAttribute ();
            ;
           
            Rect pos = position;
            pos.height = defaultPropertyHeight;
            serializedProperties.Clear ();
        
        
            float height = defaultPropertyHeight;
        
            string saveID = property.propertyPath;
        
            SerializedProperty nameProp = property.FindPropertyRelative ("_name");
            SerializedProperty iterationProperty = nameProp.Copy ();
        
        
            if (EditorLSDatabase.foldAll) {
                LSEditorUtility.SetPersistentFlag (saveID, false);
            }
        
            if (LSEditorUtility.PersistentFoldout (pos,
                                               nameProp.stringValue,
                                               saveID
            )) {
                pos.y += defaultPropertyHeight;
                if (dataItemAttribute.WritableName) {
                    serializedProperties.Add (nameProp);
                }


                int beginningDepth = iterationProperty.depth;
                while (iterationProperty.NextVisible(true)) {
                    if (iterationProperty.depth <= beginningDepth - 1) {
                        //serializedProperties.RemoveAt(serializedProperties.Count - 1);
                        break;
                    }
                    if (iterationProperty.depth > beginningDepth) {
                        continue;
                    }
                    serializedProperties.Add (iterationProperty.Copy ());

                }
                pos.x += defaultPropertyHeight;
                pos.width -= defaultPropertyHeight;
                for (int i = 0; i < serializedProperties.Count; i++) {
                    SerializedProperty curProp = serializedProperties[i];
                    float propertyHeight = EditorGUI.GetPropertyHeight (curProp,new GUIContent(),true);
                    EditorGUI.PropertyField (pos, curProp, true);
                    pos.y += propertyHeight;
                    height += propertyHeight;
                }
            }
        
            LSEditorUtility.SetPersistentValue (saveID, height);
        
        }   
    }
}
#endif
