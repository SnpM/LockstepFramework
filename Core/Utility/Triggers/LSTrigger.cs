using UnityEngine;
using System.Collections;
using TypeReferences;

namespace Lockstep
{
    public abstract class LSTrigger : MonoBehaviour
    {
        [SerializeField,ClassImplements(typeof(ITriggerMessage))]
        private ClassTypeReference _triggerMessageType;

        ClassTypeReference TriggerMessageType { get { return _triggerMessageType; } }

        [SerializeField]
        private string _channelID;

        string ChannelID { get { return _channelID; } }

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
        protected abstract void OnCheckInput();

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

        public void End () {
            TriggerManager.Dessimilate(this);
            OnEnd ();
        }

        protected virtual void OnEnd () {

        }

    }
}