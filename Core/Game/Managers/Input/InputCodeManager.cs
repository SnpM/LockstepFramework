using System.Collections; using FastCollections;
using System.Collections.Generic;
using Lockstep.Data;
using UnityEngine;
namespace Lockstep
{
    public static class InputCodeManager
    {
        private static Dictionary<string,InputDataItem> InputDataMap = new Dictionary<string, InputDataItem>();
        private static BiDictionary<string,ushort> InputMap = new BiDictionary<string, ushort>();
        private static bool Setted {get; set;}
        public static void Setup () {
            IInputDataProvider provider;
            if (LSDatabaseManager.TryGetDatabase<IInputDataProvider> (out provider)) {
                InputDataItem[] inputData = provider.InputData;
                for (int i = inputData.Length - 1; i >= 0; i--) {
                    InputDataItem item = inputData[i];
                    ushort id = (ushort)(i + 1);
                    string code = inputData[i].Name;
                    InputMap.Add(code,id);
                    InputDataMap.Add (code,item);
                }
                Setted = true;
            }
        }
        public static InputDataItem GetInputData (string code) {
            InputDataItem item;
            if (InputDataMap.TryGetValue(code, out item)) {
                return item;
            }
            else {
                Debug.Log ("No InputData of " + code + " found. Ignoring.");
            }
            return null;
        }
        public static ushort GetCodeID (string code) {
            if (!Setted)
                throw NotSetupException;

            if (string.IsNullOrEmpty(code))
                return 0;
            try {
                
                return InputMap[code];
            }
            catch {

                throw new System.Exception(string.Format("Input Code '{0}' does not exist in the current database", code));
            }
        }
        public static string GetIDCode (ushort id) {
            if (!Setted)
                throw NotSetupException;
            if (id == 0)
                return "";
            return InputMap.GetReversed(id);
        }
        static System.Exception NotSetupException {
            get {
                return new System.Exception("InputCodeManager has not yet been setup.");
            }
        }
    }
}