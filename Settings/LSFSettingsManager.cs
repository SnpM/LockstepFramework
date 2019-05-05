using UnityEngine;

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
			Initialize();
		}
		static void Initialize()
		{
			LSFSettings settings = Resources.Load<LSFSettings>(SETTINGS_NAME);

#if UNITY_EDITOR
			if (settings == null)
			{
				if (Application.isPlaying == false)
				{

					settings = ScriptableObject.CreateInstance<LSFSettings>();
					if (!System.IO.Directory.Exists(Application.dataPath + "/Resources"))
						AssetDatabase.CreateFolder("Assets", "Resources");
					AssetDatabase.CreateAsset(settings, "Assets/Resources/" + SETTINGS_NAME + ".asset");
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					Debug.Log("Successfuly created new settings file.");
				}
				else
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

			if (MainSettings == null)
				Initialize();
			return MainSettings;
		}

	}
}