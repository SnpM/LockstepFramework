using System.Collections;
using System.Collections.Generic;
using Lockstep.Data;
namespace Lockstep
{
    public static class InputCodeManager
    {
        private static BiDictionary<string,ushort> InputMap = new BiDictionary<string, ushort>();
        private static bool Setted {get; set;}
        public static void Setup () {
            InputDataItem[] inputData = (LSDatabaseManager.CurrentDatabase as DefaultLSDatabase).InputData;
            for (int i = inputData.Length - 1; i >= 0; i--) {
                ushort id = (ushort)(i + 1);
                string code = inputData[i].Name;
                InputMap.Add(code,id);
            }
            Setted = true;
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

                throw new System.Exception(string.Format("Code '{0}' does not exist in the current database", code));
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