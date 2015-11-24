#if true
using UnityEngine;
using System.Collections;
using UnityEditor;
using Lockstep;
using System;
using System.IO;

namespace Lockstep.Data
{
    public class EditorLSDatabaseWindow : EditorWindow
    {
       
        private static EditorLSDatabase _databaseEditor;
        private static EditorLSDatabase databaseEditor {
            get {
                return _databaseEditor;
            }
        }

        [MenuItem ("Lockstep/Database %#l")]
        private static void Menu()
        {
            EditorLSDatabaseWindow window = EditorWindow.GetWindow<EditorLSDatabaseWindow>();
            window.titleContent = new GUIContent("Lockstep Database");
            window.minSize = new Vector2(400, 100);
            window.Show();
        }

        void OnEnable()
        {
            LoadDatabase();
        }

        private DataEditType editorState = DataEditType.Agent;
        Vector2 scrollPos;

        static readonly DataEditType[] DataEditTypes = (DataEditType[])Enum.GetValues(typeof(DataEditType));

        Rect windowRect  = new Rect (0,0,500,500);
        void OnGUI()
        {
            if (Application.isPlaying) {
                EditorGUILayout.LabelField("Cannot modify database during runtime", EditorStyles.boldLabel);
                return;
            }
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < DataEditTypes.Length; i++) {
                DataEditType dataEditType = DataEditTypes[i];
                if (GUILayout.Button (dataEditType.ToString()))
                {
                    editorState = dataEditType;
                    break;
                }
            }
            EditorGUILayout.EndHorizontal();
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField(editorState.ToString (), style);

            EditorGUI.BeginChangeCheck();

            switch (editorState)
            {
                case DataEditType.Agent:
                    databaseEditor.DrawAgentDatabase();
                    break;
                case DataEditType.Projectile:
                    databaseEditor.DrawProjectileDatabase();
                    break;
                case DataEditType.Effect:
                    databaseEditor.DrawEffectDatabase ();
                    break;
                case DataEditType.Ability:
                    databaseEditor.DrawAbilityDatabase ();
                    break;
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                Save();
            }

            if (GUILayout.Button ("Apply")) {
                databaseEditor.Apply();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        static void LoadDatabase()
        {
            LSDatabase database = AssetDatabase.LoadAssetAtPath <LSDatabase>(LSDatabaseManager.DATABASE_RESOURCES_PATH) as LSDatabase;
            if (database == null) {
                CreateDatabase ();
            }

            else 
            {
                _databaseEditor = new EditorLSDatabase(database);
            }
        }

        static void CreateDatabase()
        {
            if (!AssetDatabase.IsValidFolder(LSDatabaseManager.DATABASE_RESOURCES_FOLDER))
            {
                string[] folderParts = LSDatabaseManager.DATABASE_RESOURCES_FOLDER.Split(new char[]{'/'}, StringSplitOptions.RemoveEmptyEntries);
                string curFolderName = "Assets";
                string newFolderName = "";
                for (int i = 0; i < folderParts.Length; i++) {
                    newFolderName = curFolderName + "/" + folderParts[i];
                    if (AssetDatabase.IsValidFolder(newFolderName) == false) {
                        AssetDatabase.CreateFolder(curFolderName, folderParts[i]);
                    }
                    curFolderName += newFolderName;
                    //Debug.Log(curFolderName + ", " +AssetDatabase.IsValidFolder (curFolderName));
                        
                }
            }
            else {

            }
            _databaseEditor = new EditorLSDatabase(ScriptableObject.CreateInstance<LSDatabase>());
            AssetDatabase.CreateAsset(databaseEditor.Database, LSDatabaseManager.DATABASE_RESOURCES_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void Save()
        {
            databaseEditor.Save();
            EditorUtility.SetDirty (databaseEditor.Database);
            AssetDatabase.SaveAssets();
        }



        private enum DataEditType
        {
            Agent,
            Projectile,
            Effect,
            Ability
        }
    }
}
#endif