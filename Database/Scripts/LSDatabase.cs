using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Lockstep;
using Lockstep.Data;

namespace Lockstep.Data
{
    [Serializable]
    public class LSDatabase : ScriptableObject, IDatabase
    {

        #region Agents
        [SerializeField]
        protected AgentInterfacer[]
        _agentData;
        public AgentInterfacer[] AgentData { get { return _agentData; } }
        #endregion

        #region Projectiles
        [SerializeField]
        protected ProjectileDataItem[]
        _projectileData ;

        public ProjectileDataItem[] ProjectileData { get { return _projectileData; } }

        #endregion

        #region Effects
        [SerializeField]
        protected EffectDataItem[]
        _effectData;

        public EffectDataItem[] EffectData { get { return _effectData; } }
        #endregion

        #region Ability
        [SerializeField]
        protected AbilityInterfacer[]
        _abilityData ;
        public AbilityInterfacer[] AbilityData { get { return _abilityData; } }
       #endregion

    }

}
