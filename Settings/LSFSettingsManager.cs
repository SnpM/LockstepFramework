using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Lockstep {
    public static class LSFSettingsManager {
        public const string SETTINGS_NAME = "LockstepFrameworkSettings";
        static LSFSettings MainSettings;

        static LSFSettingsManager () {
            LSFSettings settings = Resources.Load<LSFSettings> (SETTINGS_NAME);
#if UNITY_EDITOR
            if (Application.isPlaying == false) {
                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance <LSFSettings> ();
                    if (!System.IO.Directory.Exists(Application.dataPath + "/Resources"))
                        AssetDatabase.CreateFolder ("Assets","Resources");
                    AssetDatabase.CreateAsset (settings,"Assets/Resources/" + SETTINGS_NAME);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh ();
                }
            }
#endif
            MainSettings = settings;
            if (MainSettings == null) {
                throw new System.NullReferenceException ("No LockstepFrameworkSettings detected. Make sure there is one in the root directory of a Resources folder");
            }
        }

        public static LSFSettings GetSettings () {
            return MainSettings;
        }

    }
}