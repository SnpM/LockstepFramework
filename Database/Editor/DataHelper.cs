#if UNITY_EDITOR
using UnityEngine;
using System.Collections; using FastCollections;
using System;
using System.Collections.Generic;
using UnityEditor;
using Lockstep;
using System.Reflection;
using Lockstep.Rotorz.ReorderableList;
namespace Lockstep.Data
{

    public sealed class DataHelper
    {
        public DataHelper (
            Type targetType,
            EditorLSDatabase sourceEditor,
            LSDatabase sourceDatabase,
            string displayName,
            string dataFieldName,
            SortInfo[] sorts,
            out bool valid)
        {
            Sorts = sorts;
            this.TargetType = targetType;
            SourceEditor = sourceEditor;
            this.DisplayName = displayName;
            SourceDatabase = sourceDatabase;
            _dataFieldName = dataFieldName; 

            FieldInfo info = sourceDatabase.GetType ().GetField (_dataFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (info == null) {
                Debug.Log (string.Format ("Field with fieldName of {0} not found.", dataFieldName));
                valid = false;
                return;
            }

            _getData = () => (DataItem[])info.GetValue(sourceDatabase);
            _setData = (value) => info.SetValue(sourceDatabase, value);
            if (Data == null)
                Data = (DataItem[])Array.CreateInstance (TargetType,0);
            DataItemAttribute dataAttribute = (DataItemAttribute)Attribute.GetCustomAttribute(targetType, typeof (DataItemAttribute));
            _dataAttribute = dataAttribute ?? new DataItemAttribute ();
            valid = true;
        }
       
        public string DisplayName {get; private set;}
        public string DataCodeName {
            get; private set;
        }

        string _dataFieldName;
        public SerializedProperty DataProperty {
            get { return serializedObject.FindProperty (_dataFieldName);}
        }

        private SerializedObject _serializedObject;
        public SerializedObject serializedObject {
            get {return SourceEditor.serializedObject;}
        }
        public SortInfo[] Sorts {get; private set;}
        public Type TargetType {get; private set;}
        public EditorLSDatabase SourceEditor {get; private set;}
        public LSDatabase SourceDatabase {get; private set;}
        readonly Func<DataItem[]> _getData;
        readonly Action<DataItem[]> _setData;

        public DataItem[] Data { 
            get {return _getData ();}
            set {
                _setData(value);
            }
        }
        private DataItemAttribute _dataAttribute;
        public DataItemAttribute DataAttribute {get {return _dataAttribute;}}
        public ReorderableListFlags ListFlags {
            get {return _dataAttribute.ListFlags;}
        }

        public void Sort (Comparison<DataItem> comparison)
        {
            //EditorLSDatabase.FoldAll();
            Array.Sort (Data as DataItem[], comparison);
        }


        ArrayList _bufferData;
        ArrayList bufferData {
            get {
                return _bufferData != null ? _bufferData : (_bufferData = new ArrayList (Array.CreateInstance (TargetType,0)));
            }
        }
        static HashSet<int> duplicateChecker = new HashSet<int> ();


        private void CullDuplicates () {
            DataItem[] data = Data as DataItem[];
            bufferData.Clear ();
            duplicateChecker.Clear ();
            for (int i = 0; i < data.Length; i++) {
                DataItem item = data[i];
                int hash = item.Name.GetHashCode();
                if (duplicateChecker.Contains (hash)) {
                    Debug.LogError ("Item [" + i + "]: Multiple items cannot have same hash code.");
                }
                else {
                    bufferData.Add (item);
                    duplicateChecker.Add (hash);
                }
            }
            if (bufferData.Count != data.Length) {
                Debug.Log (bufferData.ToArray () as DataItem[]);
                Data = bufferData.ToArray () as DataItem[];
            }
        }
        private void Apply () {
            EditorUtility.SetDirty (this.SourceDatabase);
            SourceDatabase.cerealObject().Update();
            SourceDatabase.cerealObject().ApplyModifiedProperties();
        }
        private void CheckDuplicates () {
            DataItem[] data = Data as DataItem[];
            for (int i = 0; i < data.Length; i++) {
                DataItem item = data[i];
                int hash = item.Name.GetHashCode();
                if (duplicateChecker.Contains (hash)) {
                    Debug.LogWarning ("Item [" + i + "]: Multiple items cannot have same hash code.");
                }
                else {
                    duplicateChecker.Add (hash);
                }
            }
        }

        private bool firstManage = true;
        public void Manage ()
        {
            SourceDatabase.cerealObject().ApplyModifiedProperties();
            if (firstManage) {
                if (DataAttribute.AutoGenerate)
                    GenerateFromTypes (DataAttribute.ScriptBaseType);
                firstManage = false;
            }
            DataItem[] data = Data;
            for (int i = 0; i < data.Length; i++) {
                data[i].Manage();
            }
            Apply ();
        }

        private void OnManage () {

        }

        public void FilterWithString (string filter) {
            /*
            TData[] data = Data;
            FastList<int> bufferInts = LSUtility.bufferInts;
            bufferInts.FastClear();
            for (int i = 0; i < data.Length; i++) {
                TData item = data[i];
                if (item.Name.ToLower().Contains(filter.ToLower())) {
                    bufferInts.Add(i);
                }
                else {

                }
            }*/
            this.Sort(
                (item,item2) =>
                (item.Name.InsensitiveContains(filter) ? 0 : 1)
                - (item2.Name.InsensitiveContains(filter) ? 0 : 1)
                );
        }

        private void GenerateFromTypes (Type baseType) {
            bufferData.Clear ();
            Type scriptBaseType = _dataAttribute.ScriptBaseType;
            List<Type> filteredTypes = LSEditorUtility.GetFilteredTypes(scriptBaseType);
            DataItem[] data = Data as DataItem[];
            HashSet<Type> lackingTypes = new HashSet<Type> ();
            Type abilityType = typeof (Ability);
            Type activeAbilityType = typeof (ActiveAbility);
            for (int i = 0; i < filteredTypes.Count; i++) {
                if (filteredTypes[i].BaseType != abilityType && filteredTypes[i].BaseType != activeAbilityType)
                {
                    if (filteredTypes[i].GetCustomAttributes(typeof (CustomActiveAbilityAttribute),false).Length == 0)
                    continue;
                }
                lackingTypes.Add(filteredTypes[i]);
            }
            for (int i = 0; i < data.Length; i++) {
                DataItem item = data[i];
                ScriptDataItem scriptItem = item as ScriptDataItem;
                if (lackingTypes.Remove(scriptItem.Script)) {
                    bufferData.Add(item);
                }
            }
            foreach (Type type in lackingTypes) {
                DataItem item = (DataItem)Activator.CreateInstance ( (TargetType));
                item.Inject (type);
                bufferData.Add(item);
            }
            DataItem[] tempData = (DataItem[])Array.CreateInstance (TargetType,bufferData.Count);
            bufferData.CopyTo (tempData);
            Data = tempData;
        }
        


        public override bool Equals(object obj)
        {
            if (System.Object.ReferenceEquals (obj, null))
                return System.Object.ReferenceEquals (this, null) || DataProperty == null || DataProperty.isArray == false;
            return System.Object.ReferenceEquals (this, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator == (DataHelper source, DataHelper other) {
            if (System.Object.ReferenceEquals(source, null))
                return System.Object.ReferenceEquals (other, null);

            else
                return source.Equals (other);

        }
        public static bool operator != (DataHelper source, DataHelper other) {
            return !(source == other);
        }
    }
    /*public interface IDataHelper <out TData> where TData : DataItem{
        void GenerateEnum ();
    }*/
}
#endif