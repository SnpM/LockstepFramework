#if true

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
namespace Lockstep.Data {
    [CustomPropertyDrawer (typeof (DataItem), true)]
public class EditorDataItem : PropertyDrawer {
    bool initialized = false;
    private void Initialize () {
        initialized = true;
        OnInitialize ();
    }
    protected  void OnInitialize () {
        
    }
    const float propertyHeight = 15;
    
    public sealed override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
        return LSEditorUtility.GetPersistentValue (propertyHeight, property.propertyPath);
    }
    private List<SerializedProperty> serializedProperties = new List<SerializedProperty> ();
    
    const BindingFlags OpenBinding = System.Reflection.BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    DataItemAttribute dataItemAttribute;
    
    public sealed override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        
        if (initialized == false)
            Initialize ();
        
        dataItemAttribute =
            dataItemAttribute
                ??(DataItemAttribute)Attribute.GetCustomAttribute(LSEditorUtility.GetPropertyType(property), typeof (DataItemAttribute), false)
                ?? new DataItemAttribute ();
        ;
           
        Rect pos = position;
        pos.height = propertyHeight;
        serializedProperties.Clear ();
        
        
        float height = propertyHeight;
        
        string saveID = property.propertyPath;
        
        SerializedProperty nameProp = property.FindPropertyRelative("_name");
        SerializedProperty iterationProperty = nameProp.Copy();
        
        
        if (EditorLSDatabase.foldAll) 
        {
            LSEditorUtility.SetPersistentFlag (saveID, false);
        }
        
        if (LSEditorUtility.PersistentFoldout (pos,
                                               nameProp.stringValue,
                                               saveID
                                               )) {
            pos.y += propertyHeight;
            if (dataItemAttribute.WritableName)
                serializedProperties.Add(nameProp);
            /*for (int i = 0; i < extraProperties.Count; i++) {
                    serializedProperties.Add (property.FindPropertyRelative (extraProperties[i]));
                }*/
            int beginningDepth = iterationProperty.depth;
            while (iterationProperty.NextVisible(false))
            {
                if (iterationProperty.depth != beginningDepth) break;
                serializedProperties.Add(iterationProperty.Copy());
            }
            pos.x += 15;
            pos.width -= 15;
            for (int i = 0; i < serializedProperties.Count; i++) {
                EditorGUI.PropertyField (pos, serializedProperties[i]);
                pos.y += propertyHeight;
                height += propertyHeight;
            }
        }
        
        LSEditorUtility.SetPersistentValue (saveID, height);
        
    }   
}
}
#endif
