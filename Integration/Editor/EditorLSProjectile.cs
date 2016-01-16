using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Lockstep.Integration
{
    [CustomEditor (typeof(LSProjectile))]
    public class EditorLSProjectile : Editor
    {
        SerializedObject so {get {return base.serializedObject;}}
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck ();
            
            //Targeting
            EditorGUILayout.LabelField ("Targeting Settings", EditorStyles.boldLabel);
            so.PropertyField("_targetingBehavior");
            TargetingType targetingBehavior = (TargetingType) so.FindProperty("_targetingBehavior").enumValueIndex;
            switch (targetingBehavior) {
                case TargetingType.Free:
                case TargetingType.Seeking:
                    so.PropertyField("_speed");
                    break;
                case TargetingType.Timed:
                    so.PropertyField("_delay");
                    break;
            }
            EditorGUILayout.Space ();
            
            //Damage
            EditorGUILayout.LabelField ("Damage Settings", EditorStyles.boldLabel);
            so.PropertyField("_hitBehavior");
            switch ((HitType)so.FindProperty("_hitBehavior").enumValueIndex) {
                case HitType.Cone:
                    so.PropertyField("_angle");
                    break;
                case HitType.Area:
                    so.PropertyField("_radius"); 
                    break;
                    
                case HitType.Single:
                    
                    break;
            }
            EditorGUILayout.Space ();
            
            //Trajectory
            EditorGUILayout.LabelField ("Trajectory Settings", EditorStyles.boldLabel);

            EditorGUILayout.Space ();
            
            //Visuals
            EditorGUILayout.LabelField ("Visuals Settings", EditorStyles.boldLabel);
            so.PropertyField("_startEffect");
            so.PropertyField("_endEffect");
            so.PropertyField("_attachEndEffectToTarget");
            
            if (EditorGUI.EndChangeCheck ()) {
                serializedObject.ApplyModifiedProperties ();
                EditorUtility.SetDirty (target);
            }
        }
    }
}