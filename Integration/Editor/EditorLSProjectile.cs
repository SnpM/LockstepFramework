using UnityEngine;
using System.Collections; using FastCollections;
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
					so.PropertyField("_speed");
					break;
                case TargetingType.Positional:
                case TargetingType.Homing:
                    so.PropertyField("_speed");
					so.PropertyField("_visualArc");
                    break;
                case TargetingType.Timed:
                    so.PropertyField("_delay");
                    so.PropertyField ("_lastingDuration");
					so.PropertyField("_tickRate");
                    break;
            }
            EditorGUILayout.Space ();
            
            //Damage
            EditorGUILayout.LabelField ("Damage Settings", EditorStyles.boldLabel);
            so.PropertyField("_hitBehavior");
            switch ((HitType)so.FindProperty("_hitBehavior").enumValueIndex) {
                case HitType.Cone:
                    so.PropertyField("_angle");
					so.PropertyField("_radius");
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

			SerializedProperty useEffectProp = so.FindProperty("UseEffects");

			EditorGUILayout.PropertyField(useEffectProp);


			if (useEffectProp.boolValue)
			{
				so.PropertyField("_startFX");
				so.PropertyField("_hitFX");
				so.PropertyField("_attachEndEffectToTarget");
			}

			//PAPPS ADDED THIS:
			so.PropertyField("DoReleaseChildren");

            if (EditorGUI.EndChangeCheck ()) {
                serializedObject.ApplyModifiedProperties ();
                EditorUtility.SetDirty (target);
			}
        }
    }
}