using UnityEngine;
using System.Linq;

namespace Lockstep.Data
{
	//PROTECTED NOT PRIVATE... private dun serialize in derived class
	[System.Serializable]

	public class DefaultLSDatabase : LSDatabase, IEssentialDataProvider, IUnitConfigDataProvider
	, IAgentDataProvider, IAgentControllerDataProvider
	{
		#region Agents

		[SerializeField]
		//This data is registered for editing in the database editor
		[RegisterData("Agents")]

		[RegisterSort("Order Units First",
			typeof(AgentDataSorter),
			"OrderUnitsFirst"
		)]

		[RegisterSort("Order Buildings First",
			typeof(AgentDataSorter),
			"OrderBuildingsFirst"
		)]


		protected AgentDataItem[]
			_agentData;

		//Using IDE reference finding, you can see how the stored _agentData gets passed on to the simulation
		public IAgentData[] AgentData { get { return _agentData.Cast<IAgentData>().ToArray(); } }

		#endregion

		#region Input
		[SerializeField]
		[RegisterData("Input")]
		protected InputDataItem[] _inputData = new InputDataItem[] {
			new InputDataItem("Move",KeyCode.M),
			new InputDataItem("Attack",KeyCode.A),
			new InputDataItem("Stop",KeyCode.S)
		};
		public InputDataItem[] InputData { get { return _inputData; } }
		#endregion

		#region Projectiles

		[SerializeField, RegisterData("Projectiles")]
		protected ProjectileDataItem[] _projectileData;

		public IProjectileData[] ProjectileData { get { return _projectileData.Cast<IProjectileData>().ToArray(); } }

		#endregion

		#region Effects

		[SerializeField, RegisterData("Effects")]
		protected EffectDataItem[]
			_effectData;

		public IEffectData[] EffectData { get { return _effectData.Cast<IEffectData>().ToArray(); } }

		#endregion

		#region Ability

		[SerializeField, RegisterData("Abilities")]
		protected AbilityDataItem[]
			_abilityData;

		public AbilityDataItem[] AbilityData { get { return _abilityData; } }

		#endregion

		#region Unit Configs
		[SerializeField, RegisterData("UnitConfigElements")]
		protected UnitConfigElementDataItem[] _unitConfigElementData;
		public UnitConfigElementDataItem[] UnitConfigElementData
		{
			get
			{
				return _unitConfigElementData;
			}
		}

		[SerializeField, RegisterData("UnitConfigs")]
		protected UnitConfigDataItem[] _unitConfigData;
		public IUnitConfigDataItem[] UnitConfigData { get { return _unitConfigData; } }
		#endregion

		[SerializeField, RegisterDataAttribute("AgentControllers")]
		protected AgentControllerDataItem[] _agentControllerData;
		public AgentControllerDataItem[] AgentControllerData { get { return _agentControllerData; } }
	}
}
