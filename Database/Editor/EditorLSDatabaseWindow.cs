#if true
using UnityEngine;
using System.Collections;
using UnityEditor;
using Lockstep;
using System;
using System.IO;
using TypeReferences;
namespace Lockstep.Data {
    [System.Serializable]
    public sealed class EditorLSDatabaseWindow : EditorWindow {

        const string databaseTypeKey = "!)^@#^1";
        [SerializeField,ClassImplements (typeof (IDatabase))]
        ClassTypeReference _databaseType;
        Type DatabaseType {get {return _databaseType;}}
        public static EditorLSDatabaseWindow Window {get; private set;}
        
        private EditorLSDatabase _databaseEditor;

        public EditorLSDatabase DatabaseEditor {
            get {
                return _databaseEditor;
            }
        }

        private LSDatabase _database;
        public bool IsLoaded {get; private set;}

        public LSDatabase Database {
            get { return _database;}
        }

        [MenuItem ("Lockstep/Database %#l")]
        public static void Menu () {
            EditorLSDatabaseWindow window = EditorWindow.GetWindow<EditorLSDatabaseWindow> ();
            window.titleContent = new GUIContent ("Lockstep Database");
            window.minSize = new Vector2 (400, 100);
            window.Show ();
        }

        void OnEnable () {
            Window = this;
            DatabaseDirectory = EditorPrefs.GetString ("*DataDir", Application.dataPath);
            LoadDatabaseFromPath (DatabaseDirectory + "/" + LSDatabaseManager.DatabaseFileName);
            _databaseType = new ClassTypeReference (EditorPrefs.GetString (databaseTypeKey));
            if (_databaseType.Type == null) _databaseType = typeof (LSDatabase);
        }
        
        Vector2 scrollPos;
        Rect windowRect = new Rect (0, 0, 500, 500);

        void OnGUI () {
            if (Application.isPlaying) {
                EditorGUILayout.LabelField ("Cannot modify database during runtime", EditorStyles.boldLabel);
                return;
            }
            DrawSettings ();
            if (DatabaseEditor != null) {
                DrawDatabase ();
            } else {
                EditorGUILayout.LabelField ("No database loaded");
            }


        }

        public string DatabaseDirectory {get; private set;}
        bool settingsFoldout = false;

        void DrawSettings () {
            settingsFoldout = EditorGUILayout.Foldout (settingsFoldout, "Settings");
            if (settingsFoldout) {
                GUILayout.BeginHorizontal ();

                const int maxDirLength = 28;
                if (GUILayout.Button (DatabaseDirectory.Length > maxDirLength ? "..." + DatabaseDirectory.Substring (DatabaseDirectory.Length - maxDirLength) : DatabaseDirectory, GUILayout.ExpandWidth (true))) {
                    DatabaseDirectory = EditorUtility.OpenFolderPanel ("Database Folder", DatabaseDirectory, DatabaseDirectory);
                    EditorPrefs.SetString ("*DataDir", DatabaseDirectory);

                    LSFSettingsModifier.Save ();
                }
                if (GUILayout.Button ("Load", GUILayout.MaxWidth (50f))) {
                    string databasePath = DatabaseDirectory + "/" + LSDatabaseManager.DatabaseFileName;
                    if (LoadDatabaseFromPath (databasePath) == false) {
                        CreateDatabase (databasePath);
                        Debug.LogFormat ("Database was not found at directory of {0} so one was created", DatabaseDirectory);
                    }
                }
                GUILayout.EndHorizontal ();

                SerializedObject obj = new SerializedObject(this);

                SerializedProperty databaseTypeProp = obj.FindProperty("_databaseType");
                EditorGUILayout.PropertyField (databaseTypeProp, new GUIContent ("Database Type"));
                EditorPrefs.SetString (databaseTypeKey,_databaseType.Type.AssemblyQualifiedName);
                obj.ApplyModifiedProperties ();

            }

        }

        void DrawDatabase () {

            scrollPos = EditorGUILayout.BeginScrollView (scrollPos);

            DatabaseEditor.Draw ();

            EditorGUILayout.EndScrollView ();
        }

        bool LoadDatabaseFromPath (string absolutePath) {
            string relativePath = absolutePath.GetRelativeUnityAssetPath ();
            LSDatabase database = AssetDatabase.LoadAssetAtPath<LSDatabase> (relativePath);
            if (database != null) {
                LoadDatabase (database);
                return true;
            }
            _databaseEditor = null;
            return false;
        }

        void LoadDatabase (LSDatabase database) {
            _database = database;
            _databaseEditor = (EditorLSDatabase)Activator.CreateInstance (typeof(EditorLSDatabase));
            bool isValid;
            _databaseEditor.Initialize (this,Database, out isValid);
            if (!isValid) {
                this._databaseEditor = null;
                this._database = null;
                IsLoaded = false;
                return;
            }
            LSFSettingsManager.GetSettings ().Database = database;
            LSFSettingsModifier.Save ();
            IsLoaded = true;
        }

        bool CreateDatabase (string absolutePath) {
            LSDatabase database = (LSDatabase)ScriptableObject.CreateInstance (DatabaseType);
            LoadDatabase (database);

            string relativePath = absolutePath.GetRelativeUnityAssetPath ();
            AssetDatabase.CreateAsset (database, relativePath);

            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh ();

            return true;
        }

        void Save () {
            DatabaseEditor.Save ();
            EditorUtility.SetDirty (DatabaseEditor.Database);
            AssetDatabase.SaveAssets ();
        }

    }
}
#endif