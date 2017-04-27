using UnityEngine;
using System.Collections; using FastCollections;
using System.Linq;
namespace Lockstep.Data
{
	//PROTECTED NOT PRIVATE... private dun serialize in derived class
    [System.Serializable]

    public class DefaultLSDatabase : LSDatabase, IEssentialDataProvider, IUnitConfigDataProvider
    ,IAgentDataProvider
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
            
        
        protected AgentDataItem[]
            _agentData;

        //Using IDE reference finding, you can see how the stored _agentData gets passed on to the simulation
        public IAgentData[] AgentData { get { return _agentData.Cast<IAgentData>().ToArray(); } }

        #endregion

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
            new InputDataItem("Item4",KeyCode.V),
			new InputDataItem("Spawn",KeyCode.None)
        };
        public InputDataItem[] InputData {get {return _inputData;}}
        #endregion

        #region Projectiles

        [SerializeField,RegisterData("Projectiles")]
        protected ProjectileDataItem[] _projectileData;

        public IProjectileData[] ProjectileData { get { return _projectileData.Cast<IProjectileData>().ToArray(); } }

        #endregion

        #region Effects

        [SerializeField,RegisterData("Effects")]
        protected EffectDataItem[]
            _effectData;

        public IEffectData[] EffectData { get { return _effectData.Cast<IEffectData>().ToArray(); } }

        #endregion

        #region Ability

        [SerializeField,RegisterData("Abilities")]
        protected AbilityDataItem[]
            _abilityData;

        public AbilityDataItem[] AbilityData { get { return _abilityData; } }

		#endregion

		
		#region Unit Configs
		[SerializeField, RegisterData ("UnitConfigElements")]
		protected UnitConfigElementDataItem [] _unitConfigElementData;
		public UnitConfigElementDataItem [] UnitConfigElementData {
			get {
				return _unitConfigElementData;
			}
		}

		[SerializeField, RegisterData ("UnitConfigs")]
		protected UnitConfigDataItem [] _unitConfigData;
		public IUnitConfigDataItem [] UnitConfigData { get { return _unitConfigData; } }
		#endregion
		
    }
}
