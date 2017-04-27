using UnityEngine;
using Lockstep.Data;
using System.Collections.Generic;
using FastCollections;
using System;
namespace Lockstep {
    public class AbilityManager {

		static FastList<ActiveAbility> setupActives = new FastList<ActiveAbility>();

        public Ability[] Abilitys {get; private set;}
        public ActiveAbility[] ActiveAbilitys {get; private set;}
		public readonly FastList<AbilityDataItem> Interfacers = new FastList<AbilityDataItem>();

        public void Setup(LSAgent agent) {
			setupActives.FastClear ();
			Abilitys = agent.AttachedAbilities;
			for (int i = 0; i < Abilitys.Length; i++) {
				Ability abil = Abilitys[i];

				ActiveAbility activeAbil = abil as ActiveAbility;
				if (activeAbil.IsNotNull ())
					setupActives.Add (activeAbil);
			}

            ActiveAbilitys = setupActives.ToArray ();

            for (int i = 0; i < Abilitys.Length; i++) {
                Abilitys[i].Setup(agent, i);
            }
            for (int i = 0; i < Abilitys.Length; i++) {
                Abilitys[i].LateSetup();
            }
			for (int i = 0; i < ActiveAbilitys.Length; i++)
			{
				if (ActiveAbilitys[i].Data.IsNotNull ())

				Interfacers.Add (ActiveAbilitys[i].Data);
			}
        }

        public void Initialize() {
            for (int i = 0; i < Abilitys.Length; i++) {
                Abilitys[i].Initialize();
            }
        }

        public void Simulate() {
			for (int i = 0; i < Abilitys.Length; i++) {
                Abilitys[i].Simulate();
            }
        }
		public void LateSimulate () {
			for (int i = 0; i < Abilitys.Length; i++) {
				Abilitys[i].LateSimulate ();
			}
		}

        public void Visualize() {
			for (int i = 0; i < Abilitys.Length; i++) {
                Abilitys[i].Visualize();
            }
        }
		public void LateVisualize()
		{
			for (int i = 0; i < Abilitys.Length; i++)
			{
				Abilitys[i].LateVisualize();
			}
		}
        public void Execute(Command com) {
            for (int k = 0; k < ActiveAbilitys.Length; k++) {
                ActiveAbility abil = ActiveAbilitys[k];
                if (abil.ListenInput == com.InputCode) {
                    abil.Execute(com);
                }
                if (abil.DoRawExecute) {
                    abil.RawExecute(com);
                }
            }
        }

        public bool CheckCasting() {
            for (var k = 0; k < Abilitys.Length; k++) {
                if (Abilitys[k].IsCasting) {
                    return true;
                }
            }
            return false;
        }

        public void StopCast(int exception) {
            for (var k = 0; k < Abilitys.Length; k++) {
                if (k != exception) {
                    Abilitys[k].StopCast();
                }
            }
        }

        public void Deactivate() {
            for (var k = 0; k < Abilitys.Length; k++) {
                Abilitys[k].Deactivate();
            }
        }
		public Ability GetAbilityAny (Type type)
		{
			for (var k = 0; k < Abilitys.Length; k++) {
				var ability = Abilitys [k];
				Type abilType = ability.GetType ();
				if (abilType == type || abilType.IsSubclassOf (type)) {
					return ability;
				}
			}
			return null;
		}
        public Ability GetAbility (string name) {
            for (var k = 0; k < Abilitys.Length; k++) {
                var ability = Abilitys[k];
                if (ability.Data != null)
                if (ability.Data.Name == name) {
                    return ability;
                }
            }
            return null;
        }
        public T GetAbility<T>() where T : Ability {
            for (var k = 0; k < Abilitys.Length; k++) {
                var ability = Abilitys[k] as T;
                if (ReferenceEquals(ability, null) == false) {
                    return ability;
                }
            }
            return null;
        }
        public Ability GetAbilityAny<T>()
        {
            for (var k = 0; k < Abilitys.Length; k++) {
                Ability abil = Abilitys[k];
                if (abil is T)
                {
                    return abil;
                }
            }
            return null;
        }

        public IEnumerable<Ability> GetAbilitiesAny<T> () {
            for (var k = 0; k < Abilitys.Length; k++) {
                var abil = this.Abilitys[k];
                if (abil is T)
                    yield return abil;
            }
        }

    }
}