using UnityEngine;
using System.Collections; using FastCollections;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Lockstep
{
    public static class LSFSettingsManager
    {
        public const string DEFAULT_SETTINGS_NAME = "DefaultLockstepFrameworkSettings";
        public const string SETTINGS_NAME = "LockstepFrameworkSettings";
        static LSFSettings MainSettings;

        static LSFSettingsManager()
        {
            LSFSettings settings = Resources.Load<LSFSettings>(SETTINGS_NAME);

#if UNITY_EDITOR
            if (settings == null)
            {
                if (Application.isPlaying == false)
                {

                    settings = ScriptableObject.CreateInstance <LSFSettings>();
                    if (!System.IO.Directory.Exists(Application.dataPath + "/Resources"))
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    AssetDatabase.CreateAsset(settings, "Assets/Resources/" + SETTINGS_NAME + ".asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                
                } else
                {
                }
            }
#endif

            MainSettings = settings;
            if (MainSettings == null)
            {
                settings = Resources.Load<LSFSettings>(DEFAULT_SETTINGS_NAME);
                Debug.Log("No settings found. Loading default settings.");
            }

        }

        public static LSFSettings GetSettings()
        {
            return MainSettings;
        }

    }
}