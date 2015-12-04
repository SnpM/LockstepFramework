using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Lockstep;
using Lockstep.Data;

namespace Lockstep.Data {
    [Serializable]
    public class LSDatabase : ScriptableObject, IDatabase {

        #region Agents
        [SerializeField]
        [RegisterData (
            "Agents"
            /*new SortInfo[] {
            new SortInfo(
                "Order Units First",
                (a) => (a as AgentInterfacer).SortDegreeFromAgentType(AgentType.Unit)
                ),
            new SortInfo(
                "Order Buildings First",
                (a) => (a as AgentInterfacer).SortDegreeFromAgentType(AgentType.Building)
                )
        }*/
        )
        ]
        protected AgentInterfacer[]
            _agentData;

        public AgentInterfacer[] AgentData { get { return _agentData; } }
        #endregion

        #region Projectiles
        [SerializeField,RegisterData ("Projectiles")]
        protected ProjectileDataItem[]
            _projectileData ;

        public ProjectileDataItem[] ProjectileData { get { return _projectileData; } }

        #endregion

        #region Effects
        [SerializeField,RegisterData("Effects")]
        protected EffectDataItem[]
            _effectData;

        public EffectDataItem[] EffectData { get { return _effectData; } }
        #endregion

        #region Ability
        [SerializeField,RegisterData("Abilities")]
        protected AbilityInterfacer[]
            _abilityData ;

        public AbilityInterfacer[] AbilityData { get { return _abilityData; } }
       #endregion

    }

}
