using UnityEngine;
using System.Collections; using FastCollections;
using System.Collections.Generic;
using System;
using UnityEditor;
namespace Lockstep {
    public abstract class LSScenePropertyDrawer {
        static LSScenePropertyDrawer () {
            List<Type> drawerTypes = LSEditorUtility.GetFilteredTypes (typeof(LSScenePropertyDrawer));
            foreach (Type type in drawerTypes) {
                LSScenePropertyDrawer drawer = (LSScenePropertyDrawer)Activator.CreateInstance (type);
                Type targetType = drawer.TargetType;
                MappedDrawers.Add (targetType, drawer);
            }
        }

        public static LSScenePropertyDrawer GetDrawer (Type targetType) {
            LSScenePropertyDrawer drawer;
            while (!MappedDrawers.TryGetValue (targetType, out drawer)) {
                targetType = targetType.BaseType;
                if (targetType.BaseType == null) {
                    return null;
                }
            }
            return drawer;
        }
    
        static readonly Dictionary<Type,LSScenePropertyDrawer> MappedDrawers = new Dictionary<Type,LSScenePropertyDrawer> ();

        public abstract Type TargetType { get; }
    
        public virtual void OnSceneGUI (CerealBehaviour source, SerializedProperty property, GUIContent label) {
        
        }

        public virtual void OnDrawGizmos (CerealBehaviour source, SerializedProperty property, GUIContent label) {
        
        }
    }
}