using UnityEngine;
using Lockstep.Data;
using System.Collections.Generic;
namespace Lockstep {
    public class AbilityManager {


		static FastList<ActiveAbility> setupActives = new FastList<ActiveAbility>();
		public Health Healther {get; private set;}
		public Scan Scanner {get; private set;}
		public Move Mover {get; private set;}
		public Turn Turner {get; private set;}
		public EnergyStore EnergyStorer {get; private set;}

        private Ability[] Abilitys;
        private ActiveAbility[] ActiveAbilitys;
		public readonly FastList<AbilityInterfacer> Interfacers = new FastList<AbilityInterfacer>();

        public void Setup(LSAgent agent) {
			setupActives.FastClear ();
			Abilitys = agent.AttachedAbilities;
			for (int i = 0; i < Abilitys.Length; i++) {
				Ability abil = Abilitys[i];
				if (Healther == null) Healther = abil as Health;
				if (Scanner == null) Scanner = abil as Scan;
				if (Mover == null) Mover = abil as Move;
				if (Turner == null) Turner = abil as Turn;
				if (EnergyStorer == null) EnergyStorer = abil as EnergyStore;

				ActiveAbility activeAbil = abil as ActiveAbility;
				if (activeAbil.IsNotNull ())
					setupActives.Add (activeAbil);
			}
			ActiveAbilitys = setupActives.ToArray ();

            for (int i = 0; i < Abilitys.Length; i++) {
                Abilitys[i].Setup(agent, i);
            }
			for (int i = 0; i < ActiveAbilitys.Length; i++)
			{
				if (ActiveAbilitys[i].Interfacer.IsNotNull ())
				Interfacers.Add (ActiveAbilitys[i].Interfacer);
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

        public void Execute(Command com) {
            for (int k = 0; k < ActiveAbilitys.Length; k++) {
                ActiveAbility abil = ActiveAbilitys[k];
                if (abil.ListenInput == com.LeInput) {
                    abil.Execute(com);
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

        public T GetAbility<T>() where T : Ability {
            for (var k = 0; k < Abilitys.Length; k++) {
                var ability = Abilitys[k] as T;
                if (ReferenceEquals(ability, null) == false) {
                    return ability;
                }
            }
            return null;
        }
    }
}