#if true
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Lockstep.Data
{
    public class EditorLSDatabase
    {

        public LSDatabase Database { get; private set; }
        private SerializedObject _serializedObject;
        public SerializedObject serializedObject {get {return _serializedObject ?? (_serializedObject = Database.cerealObject());}}
        public EditorLSDatabaseWindow MainWindow {get; private set;}
        public EditorLSDatabase(EditorLSDatabaseWindow window, LSDatabase database)
        {
            this.MainWindow = window;
            Database = database;
            CreateModifier<AgentInterfacer>(
                "Agents",
                "AgentCode",
                "_agentData",
                new SortInfo(
                "Order Units First",
                (a) => (a as AgentInterfacer).SortDegreeFromAgentType(AgentType.Unit)
                ),
                new SortInfo(
                "Order Buildings First",
                (a) => (a as AgentInterfacer).SortDegreeFromAgentType(AgentType.Building)
                )
            );
             CreateModifier<ProjectileDataItem>(
                "Projectiles",
                "ProjectileCode",
                "_projectileData"
            );
           CreateModifier<EffectDataItem>(
                "Effects",
                "EffectCode",
                "_effectData"
            );
            CreateModifier<AbilityInterfacer>(
                "Abilities",
                "AbilityCode",
                "_abilityData"
            );

        }

        private DataHelper CreateModifier<TData>(string displayName, string dataCodeName, string dataFieldName, params SortInfo[] sorts) where TData : DataItem
        {
            DataHelper helper = new DataHelper(typeof(TData), this, Database, displayName, dataCodeName, dataFieldName, sorts);
            this.DataHelpers.Add (helper);
            return helper;
        }

        public void Apply()
        {
            for (int i = 0; i < DataHelpers.Count; i++) {
                DataHelpers[i].GenerateEnum ();
            }
            AssetDatabase.Refresh();
        }

        private FastList<DataHelper> DataHelpers = new FastList<DataHelper>();

        static bool isSearching;
        static string lastSearchString;
        static string searchString = "";
        
        public static bool foldAll { get { return foldAllBuffer == true || foldAllBufferBuffer == true; } }
        
        private static bool foldAllBuffer;
        private static bool foldAllBufferBuffer;
        private int selectedHelperIndex;

        public void Draw () {
            EditorGUILayout.BeginVertical ();
            EditorGUILayout.BeginHorizontal ();
            if (DataHelpers.Count == 0) return;
            for (int i = 0; i < DataHelpers.Count; i++) {
                if (GUILayout.Button (DataHelpers[i].DisplayName)) {
                    selectedHelperIndex = i;
                }
            }
            EditorGUILayout.EndHorizontal ();
            DrawDatabase (DataHelpers[selectedHelperIndex]);
            EditorGUILayout.EndVertical ();
        }

        private static void DrawDatabase(DataHelper dataHelper)
        {
            dataHelper.serializedObject.Update();
            EditorGUI.BeginChangeCheck ();
            LSEditorUtility.ListField(dataHelper.DataProperty, dataHelper.ListFlags);
            if (EditorGUI.EndChangeCheck ())
            {
                dataHelper.serializedObject.ApplyModifiedProperties ();
            }
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            //folding all
            foldAllBufferBuffer = foldAllBuffer;
            foldAllBuffer = false;
            if (GUILayout.Button("Fold All", GUILayout.MaxWidth(50)))
            {
                FoldAll();        
            }
            //Search
            EditorGUILayout.LabelField("Filter: ", GUILayout.MaxWidth(35));
            searchString = EditorGUILayout.TextField(searchString, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
            {
                searchString = "";
            }
            if (lastSearchString != searchString)
            {
                if (string.IsNullOrEmpty(searchString) == false)
                {
                    dataHelper.FilterWithString(searchString);
                }
                lastSearchString = searchString;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Order Alphabetically"))
            {
                dataHelper.Sort((a1, a2) => a1.Name.CompareTo(a2.Name));
            }

            SortInfo[] sorts = dataHelper.Sorts;
            for (int i = 0; i < sorts.Length; i++)
            {
                SortInfo sort = sorts [i];
                if (GUILayout.Button(sort.sortName))
                {
                    dataHelper.Sort((a1,a2) => sort.degreeGetter(a1) - sort.degreeGetter(a2));
                }
            }

            EditorGUILayout.EndHorizontal();
            
            dataHelper.Manage();
            dataHelper.serializedObject.Update ();
            
            EditorGUILayout.Space();
        }
    

        public void Save()
        {
            serializedObject.ApplyModifiedProperties();
        }


        
        public static void FoldAll()
        {
            foldAllBuffer = true;
        }



   
    }
}
#endif