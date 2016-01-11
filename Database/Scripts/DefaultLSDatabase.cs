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

        //  InputData item types. Make sure that the registered data has protected access
        //  to ensure that subclasses of DefaultLSDatabase still function.
        #region Input
        [SerializeField]
        [RegisterData ("Input")]
        protected InputDataItem[] _inputData = new InputDataItem[] {
            new InputDataItem("Special1", KeyCode.Q),
            new InputDataItem("Special2", KeyCode.W),
            new InputDataItem("Special3", KeyCode.E),
            new InputDataItem("Special4", KeyCode.R),
            new InputDataItem("Core1", KeyCode.A),
            new InputDataItem("Core2", KeyCode.S),
            new InputDataItem("Core3", KeyCode.D),
            new InputDataItem("Core4",KeyCode.F),
            new InputDataItem("Item1", KeyCode.Z),
            new InputDataItem("Item2",KeyCode.X),
            new InputDataItem("Item3",KeyCode.C),
            new InputDataItem("Item4",KeyCode.V)
        };
        public InputDataItem[] InputData {get {return _inputData;}}
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