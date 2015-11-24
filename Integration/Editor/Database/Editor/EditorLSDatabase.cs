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

        public EditorLSDatabase(LSDatabase database)
        {
            Database = database;
            this.AgentDataModifier = CreateModifier<AgentInterfacer>(
                "AgentCode",
                "_agentData"
            );
            this.ProjectileDataModifier = CreateModifier<ProjectileDataItem>(
                "ProjectileCode",
                "_projectileData"
            );
            this.EffectDataModifier = CreateModifier<EffectDataItem>(
                "EffectCode",
                "_effectData"
            );
            this.AbilityDataModifier = CreateModifier<AbilityInterfacer>(
                "AbilityCode",
                "_abilityData"
            );

        }

        private DataHelper<TData> CreateModifier<TData>(string dataCodeName, string dataFieldName) where TData : DataItem,new()
        {
            DataHelper<TData> helper = new DataHelper<TData>(this, Database, dataCodeName, dataFieldName);
            return helper;
        }

        public void Apply()
        {
            this.AgentDataModifier.GenerateEnum();
            this.ProjectileDataModifier.GenerateEnum();
            this.EffectDataModifier.GenerateEnum();
            this.AbilityDataModifier.GenerateEnum();
            AssetDatabase.Refresh();
        }

        DataHelper<AgentInterfacer> AgentDataModifier { get; set; }

        DataHelper<ProjectileDataItem> ProjectileDataModifier { get; set; }

        DataHelper<EffectDataItem> EffectDataModifier { get; set; }

        DataHelper<AbilityInterfacer> AbilityDataModifier { get; set; }
    

        static bool isSearching;
        static string lastSearchString;
        static string searchString = "";
        
        public static bool foldAll { get { return foldAllBuffer == true || foldAllBufferBuffer == true; } }
        
        private static bool foldAllBuffer;
        private static bool foldAllBufferBuffer;

        private static void DrawDatabase<TData>(DataHelper<TData> dataHelper, params SortInfo<TData>[] sorts) where TData : DataItem, new()
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

            for (int i = 0; i < sorts.Length; i++)
            {
                SortInfo<TData> sort = sorts [i];
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
    
        private static void GenerateEnum<TData>(DataHelper<TData> helper) where TData : DataItem,new()
        {
            LSEditorUtility.GenerateEnum<TData>(helper.DataCodeName, helper);
        }

        public void Save()
        {
            serializedObject.ApplyModifiedProperties();
        }


        
        public static void FoldAll()
        {
            foldAllBuffer = true;
        }


        public void DrawAgentDatabase()
        {
            if (AgentDataModifier != null)
            {
                DrawDatabase<AgentInterfacer>(
                    AgentDataModifier,
                    new SortInfo<AgentInterfacer>(
                    "Order Units First",
                    (a) => a.SortDegreeFromAgentType(AgentType.Unit)
                ),
                    new SortInfo<AgentInterfacer>(
                    "Order Buildings First",
                    (a) => a.SortDegreeFromAgentType(AgentType.Building)
                )
                );
            }
        }

        public void DrawProjectileDatabase()
        {
            if (this.ProjectileDataModifier != null)
                DrawDatabase<ProjectileDataItem>(this.ProjectileDataModifier);
        }

        public void DrawEffectDatabase()
        {
            if (this.EffectDataModifier != null)
                DrawDatabase<EffectDataItem>(this.EffectDataModifier);
        }

        public void DrawAbilityDatabase()
        {
            if (this.AbilityDataModifier != null)
                DrawDatabase <AbilityInterfacer>(this.AbilityDataModifier);
        }
    }
}
#endif