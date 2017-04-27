using UnityEngine;
using System.Collections; using FastCollections;
using System;
using System.Collections.Generic;
using TypeReferences;

namespace Lockstep.Data
{
    #if UNITY_EDITOR
    [DataItemAttribute(
        false,
        Rotorz.ReorderableList.ReorderableListFlagsUtility.DefinedItems,
        true,
        typeof(ActiveAbility))]
#endif
    [Serializable]

    public sealed class AbilityDataItem : ScriptDataItem
    {

        private static Dictionary<string,AbilityDataItem> CodeInterfacerMap = new Dictionary<string, AbilityDataItem>();
        private static Dictionary<Type,AbilityDataItem> TypeInterfacerMap = new Dictionary<Type, AbilityDataItem>();

        public static void Setup()
        {
            IAbilityDataProvider database;
            if (LSDatabaseManager.TryGetDatabase<IAbilityDataProvider>(out database))
            {
                AbilityDataItem[] interfacers = database.AbilityData;
                for (int i = 0; i < interfacers.Length; i++)
                {
                    AbilityDataItem interfacer = interfacers [i];
                    if (interfacer.Script.Type == null)
                    {

                        //exception or ignore?
                        continue;
                    }
                    CodeInterfacerMap.Add(interfacer.Name, interfacer);
                    TypeInterfacerMap.Add(interfacer.Script.Type, interfacer);
                }
            }
        }

        public static AbilityDataItem FindInterfacer(string code)
        {
            AbilityDataItem output;
            if (!CodeInterfacerMap.TryGetValue(code, out output))
            {
                throw new System.Exception(string.Format("AbilityInterfacer for code '{0}' not found.", code));
            }
            return output;
        }

        public static AbilityDataItem FindInterfacer(Type type)
        {
            AbilityDataItem interfacer;
            if (TypeInterfacerMap.TryGetValue(type, out interfacer))
                return interfacer;
            return null;
        }

        public static AbilityDataItem FindInterfacer<TAbility>()  where TAbility : ActiveAbility
        {
            return FindInterfacer(typeof(TAbility));
        }

        public string GetAbilityCode()
        {
            return this.Name;
        }


        [SerializeField,DataCode("Input")]
        private string _listenInputCode;

        bool ListenInputInitialized{ get; set; }

        private ushort _listenInputID;

        public string ListenInputCode { get { return _listenInputCode; } }

        public ushort ListenInputID
        {
            get
            {
                if (ListenInputInitialized)
                {
                    return _listenInputID;
                } else
                {
                    ListenInputInitialized = true;
                    return _listenInputID = InputCodeManager.GetCodeID(_listenInputCode);
                }
            }
        }

        [SerializeField]
        private InformationGatherType _informationGather;

        public InformationGatherType InformationGather { get { return _informationGather; } }


    }

}