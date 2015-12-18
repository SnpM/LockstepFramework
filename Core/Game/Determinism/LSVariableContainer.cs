using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Lockstep
{
    /// <summary>
    /// Essentially a collection of LSVariables belonging to a single object (i.e. Ability)
    /// </summary>
    internal sealed class LSVariableContainer
    {
        static readonly FastList<LSVariable> bufferVariables = new FastList<LSVariable>();

        private LSVariable[] _variables;
        LSVariable[] Variables {get {return _variables;}}
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
    }
}