using UnityEngine;

using System;
using Lockstep;
using System.Collections;

using System.Collections.Generic;
using System.Reflection;
namespace Lockstep.Data {
    public static class LSDatabaseManager {
        public const string DatabaseFileName = "Lockstep_Database.asset";

        private static LSDatabase _currentDatabase;

        private static LSDatabase CurrentDatabase {
            get {
                return _currentDatabase/* ?? (_currentDatabase = (LSDatabase)Resources.Load<LSDatabase> (LSDatabaseManager.DATABASE_NAME))*/;
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


        public static void Initialize () {

        }


#if UNITY_EDITOR

#endif
    }
}