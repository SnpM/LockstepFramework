using UnityEngine;
using System.Collections; using FastCollections;
using System.Collections.Generic;
namespace Lockstep
{
    /// <summary>
    /// Essentially a collection of LSVariables belonging to a single object (i.e. Ability)
    /// </summary>
    public sealed class LSVariableContainer
    {
        static readonly FastList<int> bufferCompareHashes = new FastList<int>();
        static readonly FastList<LSVariable> bufferVariables = new FastList<LSVariable>();

        private LSVariable[] _variables;
        public LSVariable[] Variables {get {return _variables;}}
        public LSVariableContainer (IEnumerable<LSVariable> trackedVariables) {
            bufferVariables.FastClear(); //Recycle the buffer
            foreach (LSVariable variable in trackedVariables) {
                bufferVariables.Add(variable);
            }
            _variables = bufferVariables.ToArray();


        }

        public int Hash () {
            int hash = int.MaxValue / 2;
            //Iterate through and generate hash from variables
            foreach (LSVariable variable in this.Variables) {
                hash ^= variable.Hash();
            }
            return hash;
        }

        public void Reset () {
            //Iterate through and reset every varaible
            foreach (LSVariable vari in this.Variables) {
                vari.Reset();
            }
        }

        public IEnumerable<LSVariable> GetDesyncs (int[] compareHashes) {
            if (compareHashes.Length != Variables.Length) {
                throw new System.Exception("There are not the same amount of hashes to compare to!");
            }
            for (int i = 0; i < this.Variables.Length; i++) {
                LSVariable variable = this.Variables[i];
                if (compareHashes[i] != variable.Hash()) {
                    yield return variable;
                }
            }
        }

        public int[] GetCompareHashes () {
            bufferCompareHashes.FastClear();
            for (int i = 0; i < this.Variables.Length; i++) {
                bufferCompareHashes.Add (Variables[i].Hash());
            }
            return bufferCompareHashes.ToArray();
        }
    }
}