using UnityEngine;
using Lockstep.Data;
namespace Lockstep {
    public abstract class ActiveAbility : Ability {

		public InputCode ListenInput {get; private set;}

		protected sealed override void TemplateSetup ()
		{
			ListenInput = Interfacer.ListenInput;
		}

        public void Execute(Command com) {
            OnExecute(com);
        }

        protected abstract void OnExecute(Command com);
    }
}