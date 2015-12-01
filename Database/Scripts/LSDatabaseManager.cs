using UnityEngine;

using System;
using Lockstep;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
using Lockstep.Integration;
#endif
namespace Lockstep.Data {
    public static class LSDatabaseManager {

        public const string DatabaseFileName = "Lockstep_Database.asset";

        private static LSDatabase _currentDatabase;

        public static LSDatabase CurrentDatabase {
            get {
                return _currentDatabase/* ?? (_currentDatabase = (LSDatabase)Resources.Load<LSDatabase> (LSDatabaseManager.DATABASE_NAME))*/;
            }
        }

        public static void Setup () {
            LSDatabase database = LSFSettingsManager.GetSettings ().Database;
            _currentDatabase = database;
            LockstepManager.Setup ();
        }

        public static void Initialize () {

        }

#if UNITY_EDITOR

#endif
    }
}