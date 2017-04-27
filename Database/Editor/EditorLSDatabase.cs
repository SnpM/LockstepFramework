#if true
using UnityEngine;
using System.Collections; using FastCollections;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Lockstep.Data
{
    public sealed class EditorLSDatabase : IEditorDatabase
    {
        private void InitializeData()
        {
            Type databaseType = Database.GetType();
            Type foundationType = typeof(LSDatabase);
            if (!databaseType.IsSubclassOf(foundationType))
            {
                throw new System.Exception("Database does not inherit from LSDatabase and cannot be edited.");
            }
            HashSet<string> nameCollisionChecker = new HashSet<string>();
            FastList<SortInfo> sortInfos = new FastList<SortInfo>();
            while (databaseType != foundationType)
            {
                FieldInfo[] fields = databaseType.GetFields((BindingFlags)~0);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields [i];
                    object[] attributes = field.GetCustomAttributes(typeof(RegisterDataAttribute), false);
                    for (int j = 0; j < attributes.Length; j++)
                    {
                        RegisterDataAttribute registerDataAttribute = attributes [j] as RegisterDataAttribute;
                        if (registerDataAttribute != null)
                        {
                            if (!field.FieldType.IsArray)
                            {
                                Debug.LogError("Serialized data field must be array");
                                continue;
                            }
                            if (!field.FieldType.GetElementType().IsSubclassOf(typeof(DataItem)))
                            {
                                Debug.LogError("Serialized data type must be derived from DataItem");
                                continue;
                            }


                            object[] sortAttributes = field.GetCustomAttributes(typeof(RegisterSortAttribute), false);
                            sortInfos.FastClear();
                            foreach (object obj in sortAttributes)
                            {
                                RegisterSortAttribute sortAtt = obj as RegisterSortAttribute;
                                if (sortAtt != null)
                                {
                                    sortInfos.Add(new SortInfo(sortAtt.Name, sortAtt.DegreeGetter));
                                }
                            }

                            DataItemInfo dataInfo = new DataItemInfo(
                                                        field.FieldType.GetElementType(),
                                                        registerDataAttribute.DataName,
                                                        field.Name,
                                                        sortInfos.ToArray()
                                                    );

                            if (nameCollisionChecker.Add(dataInfo.DataName) == false)
                            {
                                throw new System.Exception("Data Name collision detected for '" + dataInfo.DataName + "'.");
                            }
                            RegisterData(dataInfo);
                            break;
                        }
                    }
                }
                databaseType = databaseType.BaseType;
            }
        }


        public LSDatabase Database { get; private set; }

        private SerializedObject _serializedObject;

        public SerializedObject serializedObject { get { return _serializedObject ?? (_serializedObject = Database.cerealObject()); } }

        public EditorLSDatabaseWindow MainWindow { get; private set; }

        public EditorLSDatabase()
        {
        }

        public void Initialize(EditorLSDatabaseWindow window, LSDatabase database, out bool valid)
        {
            this.MainWindow = window;
            Database = database;

            InitializeData();
			bool isValid = true;
            for (int i = 0; i < DataItemInfos.Count; i++)
            {
                DataItemInfo info = DataItemInfos [i];
				bool bufferValid;
				CreateDataHelper(info, out bufferValid);
				if (!bufferValid)
                {
                    Debug.LogError("Database does not match database type described by the database editor. Make sure Lockstep_Database.asset is the correct database type.");
                    isValid = false;
                    break;
                }
            }
            valid = isValid;
        }

        public void RegisterData(Type targetType, string dataName, string dataFieldName, params SortInfo[] sorts)
        {
            DataItemInfos.Add(new DataItemInfo(targetType, dataName, dataFieldName, sorts));
        }

        public void RegisterData(DataItemInfo info)
        {
            DataItemInfos.Add(info);
        }

        private DataHelper CreateDataHelper(DataItemInfo info, out bool valid)
        {
            DataHelper helper = new DataHelper(info.TargetType, this, Database, info.DataName, info.FieldName, info.Sorts, out valid);
            this.HelperOrder.Add(info.DataName);
            this.DataHelpers.Add(info.DataName, helper);
            return helper;
        }

        private readonly FastList<DataItemInfo> DataItemInfos = new FastList<DataItemInfo>();
        public readonly FastList<string> HelperOrder = new FastList<string>();
        public readonly Dictionary<string,DataHelper> DataHelpers = new Dictionary<string,DataHelper>();
        static bool isSearching;
        static string lastSearchString;
        static string searchString = "";

        public static bool foldAll { get { return foldAllBuffer == true || foldAllBufferBuffer == true; } }

        private static bool foldAllBuffer;
        private static bool foldAllBufferBuffer;
        private int selectedHelperIndex;

        public void Draw()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (DataHelpers.Count == 0)
            {
                return;
            }
            for (int i = 0; i < DataHelpers.Count; i++)
            {
                if (GUILayout.Button(DataHelpers [HelperOrder [i]].DisplayName))
                {
                    selectedHelperIndex = i;
                }
            }
            EditorGUILayout.EndHorizontal();
            DrawDatabase(DataHelpers [HelperOrder [selectedHelperIndex]]);
            EditorGUILayout.EndVertical();
        }

        private static void DrawDatabase(DataHelper dataHelper)
        {
			if (dataHelper == null || dataHelper.DataProperty == null) return;
            dataHelper.serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            LSEditorUtility.ListField(dataHelper.DataProperty, dataHelper.ListFlags);
            if (EditorGUI.EndChangeCheck())
            {
                dataHelper.serializedObject.ApplyModifiedProperties();
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
                    dataHelper.Sort((a1, a2) => sort.degreeGetter(a1) - sort.degreeGetter(a2));
                }
            }

            EditorGUILayout.EndHorizontal();
            
            dataHelper.Manage();
            dataHelper.serializedObject.Update();
            
            EditorGUILayout.Space();

            /*if (GUILayout.Button ("Apply")) {
                dataHelper.SourceEditor.Apply ();
            }*/
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