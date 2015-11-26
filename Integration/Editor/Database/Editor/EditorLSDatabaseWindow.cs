#if true
using UnityEngine;
using System.Collections;
using UnityEditor;
using Lockstep;
using System;
using System.IO;

namespace Lockstep.Data {
    public class EditorLSDatabaseWindow : EditorWindow {
       
        private EditorLSDatabase _databaseEditor;

        private EditorLSDatabase databaseEditor {
            get {
                return _databaseEditor;
            }
        }

        private LSDatabase _database;

        private LSDatabase Database {
            get { return _database;}
        }

        [MenuItem ("Lockstep/Database %#l")]
        private static void Menu () {
            EditorLSDatabaseWindow window = EditorWindow.GetWindow<EditorLSDatabaseWindow> ();
            window.titleContent = new GUIContent ("Lockstep Database");
            window.minSize = new Vector2 (400, 100);
            window.Show ();
        }

        void OnEnable () {
            DatabaseDirectory = EditorPrefs.GetString ("*DataDir", Application.dataPath);
            LoadDatabaseFromPath (DatabaseDirectory + "/" + LSDatabaseManager.DatabaseFileName);
        }

        private DataEditType editorState = DataEditType.Agent;
        Vector2 scrollPos;
        static readonly DataEditType[] DataEditTypes = (DataEditType[])Enum.GetValues (typeof(DataEditType));
        Rect windowRect = new Rect (0, 0, 500, 500);

        void OnGUI () {

            if (Application.isPlaying) {
                EditorGUILayout.LabelField ("Cannot modify database during runtime", EditorStyles.boldLabel);
                return;
            }
            DrawLoader ();
            if (databaseEditor != null) {
                DrawDatabase ();
            } else {
                EditorGUILayout.LabelField ("No database loaded");
            }


        }

        public string DatabaseDirectory {get; private set;}
        bool loadFoldout = false;

        void DrawLoader () {
            loadFoldout = EditorGUILayout.Foldout (loadFoldout, "Database Directory");
            if (loadFoldout) {
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
            }
        }

        void DrawDatabase () {

            scrollPos = EditorGUILayout.BeginScrollView (scrollPos);

            EditorGUILayout.BeginVertical ();
            EditorGUILayout.BeginHorizontal ();
            for (int i = 0; i < DataEditTypes.Length; i++) {
                DataEditType dataEditType = DataEditTypes [i];
                if (GUILayout.Button (dataEditType.ToString ())) {
                    editorState = dataEditType;
                    break;
                }
            }
            EditorGUILayout.EndHorizontal ();
            GUIStyle style = new GUIStyle (EditorStyles.boldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField (editorState.ToString (), style);

            EditorGUI.BeginChangeCheck ();

            switch (editorState) {
                case DataEditType.Agent:
                    databaseEditor.DrawAgentDatabase ();
                    break;
                case DataEditType.Projectile:
                    databaseEditor.DrawProjectileDatabase ();
                    break;
                case DataEditType.Effect:
                    databaseEditor.DrawEffectDatabase ();
                    break;
                case DataEditType.Ability:
                    databaseEditor.DrawAbilityDatabase ();
                    break;
            }
            
            if (EditorGUI.EndChangeCheck ()) {
                Save ();
            }

            if (GUILayout.Button ("Apply")) {
                databaseEditor.Apply ();
            }
            EditorGUILayout.EndVertical ();
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
            _databaseEditor = new EditorLSDatabase (this,Database);
            LSFSettingsManager.GetSettings ().Database = database;
            LSFSettingsModifier.Save ();
        }

        bool CreateDatabase (string absolutePath) {
            LSDatabase database = ScriptableObject.CreateInstance<LSDatabase> ();
            LoadDatabase (database);

            string relativePath = absolutePath.GetRelativeUnityAssetPath ();
            AssetDatabase.CreateAsset (database, relativePath);

            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh ();

            return true;
        }

        void Save () {
            databaseEditor.Save ();
            EditorUtility.SetDirty (databaseEditor.Database);
            AssetDatabase.SaveAssets ();
        }



        private enum DataEditType {
            Agent,
            Projectile,
            Effect,
            Ability
        }
    }
}
#endif