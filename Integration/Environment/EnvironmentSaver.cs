using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public abstract class EnvironmentSaver : MonoBehaviour
    {
        public void Save() {
            OnSave ();
        }
        protected virtual void OnSave () {

        }
        public void Apply () {
            OnApply ();
        }
        protected virtual void OnApply () {

        }
    }
}