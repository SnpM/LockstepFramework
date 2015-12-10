using UnityEngine;
using System.Collections;

namespace Lockstep.Data
{
    [System.Serializable]
    public class DefaultLSDatabase : LSDatabase
    {
        #region Agents

        [SerializeField]
        //This data is registered for editing in the database editor
        [RegisterData("Agents")]
            
        [RegisterSort("Order Units First",
            typeof (AgentDataSorter),
            "OrderUnitsFirst"
        )]
            
        [RegisterSort("Order Buildings First",
            typeof (AgentDataSorter),
            "OrderBuildingsFirst"
        )]
            
         
        protected AgentInterfacer[]
            _agentData;

        //Using IDE reference finding, you can see how the stored _agentData gets passed on to the simulation
        public AgentInterfacer[] AgentData { get { return _agentData; } }

        #endregion

        #region Projectiles

        [SerializeField,RegisterData("Projectiles")]
        protected ProjectileDataItem[] _projectileData;

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
            _abilityData;

        public AbilityInterfacer[] AbilityData { get { return _abilityData; } }

        #endregion
    }
}