using UnityEngine;
using Lockstep.Data;
namespace Lockstep {
    public abstract class ActiveAbility : Ability {
		const bool UseCooldown = true;
        [SerializeField,FrameCount]
        protected int _cooldown;
        public int Cooldown {get {return _cooldown;}}

		public ushort ListenInput {get; private set;}

        private int _heat;
        public int Heat {get {return _heat;}private set {_heat = value;}}

		protected sealed override void TemplateSetup ()
		{
            ListenInput = Data.ListenInputID;
		}

		public void Execute(Command com)
		{
			if (Heat <= 0 || UseCooldown == false) {
                OnExecute(com);
                Heat = Cooldown;
            }
        }

        protected sealed override void TemplateSimulate()
        {
            Heat--;
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