using Lockstep;
using System;
using System.IO;
using TypeReferences;
using UnityEditor;
using UnityEngine;
namespace Lockstep.Data {
    [System.Serializable]
    public sealed class EditorLSDatabaseWindow : EditorWindow {

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
            this.LoadDatabase(LSFSettingsManager.GetSettings().Database);
            if (this.Database != null) {
                _databaseType = LSFSettingsManager.GetSettings().Database.GetType();
                if (_databaseType.Type == null) _databaseType = typeof (DefaultLSDatabase);
            }
        }
        
        Vector2 scrollPos;
        //Rect windowRect = new Rect (0, 0, 500, 500);
        
        void OnGUI () {
            if (Application.isPlaying) {
                EditorGUILayout.LabelField ("Do not modify database during runtime", EditorStyles.boldLabel);
                //return;
            }
            DrawSettings ();
            if (DatabaseEditor != null) {
                DrawDatabase ();
            } else {
                EditorGUILayout.LabelField ("No database loaded");
            }
            
            
        }
		public static bool CanSave {
			get {
				return Application.isPlaying == false;
			}}
        public string DatabasePath {get; private set;}
        bool settingsFoldout = false;

		TextAsset jsonFile;
        void DrawSettings () {
            settingsFoldout = EditorGUILayout.Foldout (settingsFoldout, "Data Settings");
            if (settingsFoldout) {
                GUILayout.BeginHorizontal ();
                
                /*
                const int maxDirLength = 28;
                if (GUILayout.Button (DatabasePath.Length > maxDirLength ? "..." + DatabasePath.Substring (DatabasePath.Length - maxDirLength) : DatabasePath, GUILayout.ExpandWidth (true))) {
                }*/

                SerializedObject obj = new SerializedObject (this);
                
                SerializedProperty databaseTypeProp = obj.FindProperty ("_databaseType");
                EditorGUILayout.PropertyField (databaseTypeProp, new GUIContent ("Database Type"));

				float settingsButtonWidth = 70f;
                if (GUILayout.Button ("Load", GUILayout.MaxWidth (settingsButtonWidth))) {
                    DatabasePath = EditorUtility.OpenFilePanel ("Database File", Application.dataPath, "asset");
                    if (!string.IsNullOrEmpty (DatabasePath)) {
                        
                        LSFSettingsModifier.Save ();
                        if (LoadDatabaseFromPath (DatabasePath) == false) {
                            Debug.LogFormat ("Database was not found at path of '{0}'.", DatabasePath);
                        }
                    }
                }
                if (GUILayout.Button ("Create", GUILayout.MaxWidth (settingsButtonWidth))) {
                    DatabasePath = EditorUtility.SaveFilePanel ("Database File", Application.dataPath, "NewDatabase", "asset");
                    if (!string.IsNullOrEmpty (DatabasePath)) {
                        if (CreateDatabase (DatabasePath)) {
                            Debug.Log ("Database creation succesful!");
                        } else {
                            Debug.Log ("Database creation unsuccesful");
                        }
                    }
                }             
                GUILayout.EndHorizontal ();

				//json stuff
				GUILayout.BeginHorizontal ();
				jsonFile = (TextAsset)EditorGUILayout.ObjectField ("Json", jsonFile, typeof (TextAsset), false);
				if (GUILayout.Button ("Load", GUILayout.MaxWidth (settingsButtonWidth))) {
					LSDatabaseManager.ApplyJson (jsonFile.text, Database);
				}
				if (GUILayout.Button ("Save", GUILayout.MaxWidth (settingsButtonWidth))) {
					System.IO.File.WriteAllText (
						AssetDatabase.GetAssetPath (jsonFile),
						LSDatabaseManager.ToJson (Database)
					);
				}
				GUILayout.EndHorizontal ();
				if (CanSave)
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
            _databaseEditor = new EditorLSDatabase();
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
            if (database == null) return false;

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
