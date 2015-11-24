using UnityEngine;

using System;
using Lockstep;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using Lockstep.Integration;
#endif
namespace Lockstep.Data
{
	public static class LSDatabaseManager
	{
		public const string DATABASE_FOLDER_PARENT = "Lockstep/";
		public const string DATABASE_NAME = "Lockstep_Database";
        public const string DATABASE_FOLDER = DATABASE_FOLDER_PARENT + "Database/";
        public const string DATABASE_RESOURCES_FOLDER = LSDatabaseManager.DATABASE_FOLDER + "Resources/";
        public const string DATABASE_FILE_NAME = DATABASE_NAME + ".asset";
        public const string DATABASE_RESOURCES_PATH = "Assets/" + LSDatabaseManager.DATABASE_RESOURCES_FOLDER + LSDatabaseManager.DATABASE_FILE_NAME;


		public static bool Loaded { get; private set; }

		public static Terrain Terrain { get; private set; }

        private static LSDatabase _currentDatabase;
        public static LSDatabase CurrentDatabase {
            get {
                return _currentDatabase/* ?? (_currentDatabase = (LSDatabase)Resources.Load<LSDatabase> (LSDatabaseManager.DATABASE_NAME))*/;
            }
        }


        public static void Initialize ()
		{
			if (Loaded == false) {
                const string path = DATABASE_NAME;
                LSDatabase database = Resources.Load (path) as LSDatabase;
                _currentDatabase = Resources.Load (path) as LSDatabase;
				Loaded = true;
                LockstepManager.Setup();
			}
			Terrain = GameObject.FindObjectOfType<Terrain> ();
		}



#if UNITY_EDITOR

#endif
	}
}