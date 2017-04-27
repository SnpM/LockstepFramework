using UnityEngine;

using System;
using Lockstep;
using System.Collections; using FastCollections;

using System.Collections.Generic;
using System.Reflection;
namespace Lockstep.Data {
    public static class LSDatabaseManager {
        public const string DatabaseFileName = "Lockstep_Database.asset";

        private static LSDatabase _currentDatabase;

        public static LSDatabase CurrentDatabase {
            get {
				if (_currentDatabase == null) {
					Setup ();
				}
				return _currentDatabase;
            }
        }

        public static bool TryGetDatabase <TDatabase> (out TDatabase database) where TDatabase : class
        {
            database = CurrentDatabase as TDatabase;
            if (database.IsNull()) {
                return false;
            }
            return true;
        }

        public static void Setup () {
            //LSFSettingsManagers.GetSettings().Database is the most recent database loaded/created.
            //You can also set this manually by dragging the desired database onto the saved LSFSettings in...
            //'Assets/Resources/LockstepFrameworkSettings'
            LSDatabase database = LSFSettingsManager.GetSettings ().Database;
            _currentDatabase = database;
        }

		public static string ToJson (LSDatabase database)
		{
			return JsonUtility.ToJson (database, true);
		}
		public static TDatabase FromJson<TDatabase> (string json) where TDatabase : LSDatabase, new()
		{
			var database = new TDatabase ();
			return database;
		}
		public static void ApplyJson (string json, object database)
		{
			JsonUtility.FromJsonOverwrite (json, database);
		}
		public static void ApplyJson (string json)
		{
			ApplyJson (json, _currentDatabase);
		}
		public static void ApplyNewJson (string json, Type databaseType)
		{
			//todo guard
			object database = JsonUtility.FromJson (json, databaseType);
			database.ToString ();
		}

        public static void Initialize () {

        }


#if UNITY_EDITOR

#endif
    }
}