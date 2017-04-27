using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;

namespace Lockstep
{
    public class EditorLocalVisualize : Lockstep.EditorVisualize
    {
        public override System.Type TargetType
        {
            get
            {
                return typeof(LocalVisualizeAttribute);
            }
        }

        public override void OnSceneGUI(CerealBehaviour source, UnityEditor.SerializedProperty property, GUIContent label)
        {
            base.OnSceneGUI(source, property, label);
        }
    }
}