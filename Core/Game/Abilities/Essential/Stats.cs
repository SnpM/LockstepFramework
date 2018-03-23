using UnityEngine;
using System;

namespace Lockstep {
    public class Stats : Ability {

        protected Move cachedMove;
        protected Scan cachedScan;
        protected Health cachedHealth;
        protected override void OnLateSetup()
        {
            cachedMove = Agent.GetAbility<Move> ();
            cachedScan = Agent.GetAbility<Scan> ();
            cachedHealth = Agent.GetAbility<Health> ();
        }

        public long Speed {
            get {
                return cachedMove.Speed;
            }
        }

        public long Range {
            get {
                return cachedScan.Range;
            }
        }

        public long Damage {
            get {
                return cachedScan.Damage;
            }
        }

        public long Health {
            get {
                return cachedHealth.MaxHealth;
            }
        }

        public long DPS {
            get {
				return cachedScan.Damage.Div(cachedScan.AttackInterval);
            }
        }

    }
}