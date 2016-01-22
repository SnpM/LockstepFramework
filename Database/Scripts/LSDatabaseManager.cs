using UnityEngine;

using System;
using Lockstep;
using System.Collections;

using System.Collections.Generic;
using System.Reflection;
namespace Lockstep.Data {
    public static class LSDatabaseManager {
        static readonly Dictionary<string,object> DatabaseCache = new Dictionary<string, object>();
        public const string DatabaseFileName = "Lockstep_Database.asset";

        private static LSDatabase _currentDatabase;

        public static LSDatabase CurrentDatabase {
            get {
                return _currentDatabase/* ?? (_currentDatabase = (LSDatabase)Resources.Load<LSDatabase> (LSDatabaseManager.DATABASE_NAME))*/;
            }
        }

        public static void Setup () {
            //LSFSettingsManagers.GetSettings().Database is the most recent database loaded/created.
            //You can also set this manually by dragging the desired database onto the saved LSFSettings in...
            //'Assets/Resources/LockstepFrameworkSettings'
            LSDatabase database = LSFSettingsManager.GetSettings ().Database;
            _currentDatabase = database;
        }


        public static void Initialize () {
            foreach (System.Reflection.FieldInfo info in _currentDatabase.GetType().GetFields()) {
                
                object[] registerDataAtts = (info.GetCustomAttributes(typeof (Lockstep.Data.RegisterDataAttribute),true));
                if (registerDataAtts.Length > 0) {
                    RegisterDataAttribute at = registerDataAtts[0] as RegisterDataAttribute;
                    DatabaseCache.Add(at.DataName,info.GetValue(_currentDatabase));
                }
            }
        }

        public static object GetData (string dataName) {
            object ret;
            if (DatabaseCache.TryGetValue(dataName, out ret)) {
                return ret;
            }
            //Throw exception?
            return null;
        }

#if UNITY_EDITOR

#endif
    }
}