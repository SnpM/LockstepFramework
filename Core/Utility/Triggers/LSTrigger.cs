using UnityEngine;
using System.Collections;
using TypeReferences;

namespace Lockstep
{
    /// <summary>
    /// Triggers check for input and send a message when Trigger () is called.
    /// </summary>
    public abstract class LSTrigger : MonoBehaviour
    {


        internal int ID {
            get; set;
        }


        public void Initialize () {
            TriggerManager.Assimilate(this);
            OnInitialize ();
        }
        protected virtual void OnInitialize () {

        }

        public void CheckInput()
        {
            OnCheckInput();
        }

        /// <summary>
        /// Called every frame for checking for stimuli.
        /// </summary>
        protected virtual void OnCheckInput() {

        }

        /// <summary>
        /// Call to trigger.
        /// </summary>
        protected void Trigger()
        {
            OnTrigger();
        }

        /// <summary>
        /// Perform optional functions with triggered.
        /// </summary>
        protected virtual void OnTrigger()
        {

        }

        public void Deactivate () {
            TriggerManager.Dessimilate(this);
            this.OnDeactivate ();
        }

        protected virtual void OnDeactivate () {

        }

    }
}