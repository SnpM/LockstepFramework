using UnityEngine;
using Lockstep.Data;
namespace Lockstep {
    public abstract class ActiveAbility : Ability {

		public ushort ListenInput {get; private set;}

		protected sealed override void TemplateSetup ()
		{
            ListenInput = Interfacer.ListenInputID;
		}

        public void Execute(Command com) {
            OnExecute(com);
        }
            
        protected virtual void OnExecute(Command com) {}
        public bool DoRawExecute {get; protected set;}

        public void RawExecute (Command com) {
            OnRawExecute (com);
        }
        protected virtual void OnRawExecute (Command com) {

        }
    }
}