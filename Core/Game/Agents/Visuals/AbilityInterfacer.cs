using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using TypeReferences;

namespace Lockstep.Data {
#if UNITY_EDITOR
    [DataItemAttribute (
        false,
        Rotorz.ReorderableList.ReorderableListFlagsUtility.DefinedItems,
        true,
        typeof (Ability))]
#endif
    [Serializable]

    public sealed class AbilityInterfacer : ScriptDataItem {

        private static Dictionary<string,AbilityInterfacer> CodeInterfacerMap = new Dictionary<string, AbilityInterfacer>();
        private static Dictionary<Type,AbilityInterfacer>TypeInterfacerMap = new Dictionary<Type, AbilityInterfacer>();
        public static void Setup ()
	    {
            AbilityInterfacer[] interfacers = LSDatabaseManager.CurrentDatabase.AbilityData;
            for (int i = 0; i < interfacers.Length; i++) {
                AbilityInterfacer interfacer = interfacers[i];
                CodeInterfacerMap.Add(interfacer.Name, interfacer);
                TypeInterfacerMap.Add(interfacer.Script.Type, interfacer);
            }
		}

		public static AbilityInterfacer FindInterfacer (string code) {
			return CodeInterfacerMap [code];
		}

        public static AbilityInterfacer FindInterfacer (Type type) {
            AbilityInterfacer interfacer;
            if (TypeInterfacerMap.TryGetValue (type, out interfacer))
                return interfacer;
            return null;
        }

        public string GetAbilityCode () {
            return this.Name;
        }

        public void SetName (string name)
        {
            Name = name;
        }


		[SerializeField]
        private InputCode _listenInput = InputCode.None;
		public InputCode ListenInput {get {return _listenInput;}}
		[SerializeField]
		private InformationGatherType _informationGather;
		public InformationGatherType InformationGather {get {return _informationGather;}}
		[SerializeField]
		private MarkerType _markType;
		public MarkerType MarkType {get {return _markType;}}

		public int TileIndex {get {return (int) ListenInput;}}
	}

}