//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================
using UnityEngine;
using Lockstep.Data;
namespace Lockstep {
    public abstract class Ability : CerealBehaviour
    {
        private bool isCasting;
        
		private LSAgent _agent;
		public LSAgent Agent {
			get {
#if UNITY_EDITOR
				if (_agent == null) return this.GetComponent<LSAgent> ();
#endif
				return _agent;
			}
		}
        public string MyAbilityCode {get; private set;}
        public AbilityInterfacer Interfacer {get; private set;}

		public int ID {get; private set;}
		public Transform CachedTransform {get {return Agent.CachedTransform;}}
		public GameObject CachedGameObject {get {return Agent.CachedGameObject;}}

        public bool IsCasting {
            get {
				return isCasting;
			}
            protected set {
                if (value != isCasting) {
                    if (value == true) {
                        Agent.CheckCasting = false;
                    } else {
                        Agent.CheckCasting = true;
                    }
                    isCasting = value;
                }
            }
        }


        public void Setup(LSAgent agent, int id) {
            Interfacer = AbilityInterfacer.FindInterfacer(this.GetType());
            if (Interfacer == null) {
                throw new System.ArgumentException("This Ability of type " + this.GetType() + " has not been registered in database");
            }
            this.MyAbilityCode = Interfacer.Name;
            _agent = agent;
			ID = id;
			TemplateSetup ();
            OnSetup();
        }

		protected virtual void TemplateSetup () {

		}

        protected virtual void OnSetup() {}

        public void Initialize() {
            IsCasting = false;
            OnInitialize();
        }

        protected virtual void OnInitialize() {}

        public void Simulate() {
            OnSimulate();
            if (isCasting) {
                OnCast();
            }
        }
        protected virtual void OnSimulate() {}

		public void LateSimulate () {
			OnLateSimulate ();
		}
		protected virtual void OnLateSimulate () {

		}

        protected virtual void OnCast() {}

        public void Visualize() {
            OnVisualize();
        }

        protected virtual void OnVisualize() {}

        public void BeginCast() {
            OnBeginCast();
        }

        protected virtual void OnBeginCast() {}

        public void StopCast() {
            OnStopCast();
        }

        protected virtual void OnStopCast() {}

        public void Deactivate() {
            IsCasting = false;
            OnDeactivate();
        }

        protected virtual void OnDeactivate() {}
    }
}