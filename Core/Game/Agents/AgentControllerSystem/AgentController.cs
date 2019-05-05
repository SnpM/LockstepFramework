using System;
using UnityEngine;
using Lockstep.Data;
using System.Collections.Generic;
using FastCollections;

namespace Lockstep
{
	public sealed class AgentController
	{

		public static string[] AgentCodes;
		private static readonly Dictionary<string, ushort> CodeIndexMap = new Dictionary<string, ushort>();

		public static Dictionary<string, FastStack<LSAgent>> CachedAgents;

		public static readonly bool[] GlobalAgentActive = new bool[MaxAgents * 4];
		public static readonly LSAgent[] GlobalAgents = new LSAgent[MaxAgents * 4];

		private static readonly FastStack<ushort> OpenGlobalIDs = new FastStack<ushort>();

		public static ushort PeakGlobalID { get; private set; }

		public const int MaxAgents = 16384;
		public static readonly Dictionary<string, IAgentData> CodeInterfacerMap = new Dictionary<string, IAgentData>();
		public static readonly Dictionary<string, LSAgent> CodeTemplateMap = new Dictionary<string, LSAgent>();
		public static IAgentData[] AgentData;

		public static Dictionary<ushort, FastList<bool>> TypeAgentsActive = new Dictionary<ushort, FastList<bool>>();
		public static Dictionary<ushort, FastList<LSAgent>> TypeAgents = new Dictionary<ushort, FastList<LSAgent>>();
		static Transform OrganizerObject;

		public static void Setup()
		{
			IAgentDataProvider database;
			if (LSDatabaseManager.TryGetDatabase<IAgentDataProvider>(out database))
			{
				AgentData = database.AgentData;

				//AgentInterfacer[] agentInters = (LSDatabaseManager.CurrentDatabase as DefaultLSDatabase).AgentData;
				AgentCodes = new string[AgentData.Length];

				CachedAgents = new Dictionary<string, FastStack<LSAgent>>(AgentData.Length);

				OrganizerObject = LSUtility.CreateEmpty().transform;
				OrganizerObject.gameObject.name = "OrganizerObject";
				OrganizerObject.gameObject.SetActive(false);

				GameObject.DontDestroyOnLoad(OrganizerObject);
				for (int i = 0; i < AgentData.Length; i++)
				{
					IAgentData interfacer = AgentData[i];
					string agentCode = interfacer.Name;
					AgentCodes[i] = agentCode;

					CachedAgents.Add(agentCode, new FastStack<LSAgent>(2));
					CodeInterfacerMap.Add(agentCode, interfacer);
					CodeIndexMap.Add(agentCode, (ushort)i);
				}
			}
			else
			{
				Debug.Log("Database does not provide AgentData. Make sure it implements IAgentDataProvider.");
			}
		}


		public static IAgentData GetAgentInterfacer(string agentCode)
		{
			return AgentController.CodeInterfacerMap[agentCode];
		}

		public static LSAgent GetAgentTemplate(string agentCode)
		{
			LSAgent template;
			if (!CodeTemplateMap.TryGetValue(agentCode, out template))
			{
				template = GameObject.Instantiate(GetAgentSource(agentCode));
				CodeTemplateMap.Add(agentCode, template);
				template.transform.parent = OrganizerObject.transform;
			}

			return template;
		}

		public static ushort GetAgentCodeIndex(string agentCode)
		{
			return CodeIndexMap[agentCode];
		}

		public static string GetAgentCode(ushort id)
		{
			return AgentCodes[id];
		}

		public static bool IsValidAgentCode(string code)
		{
			return CodeInterfacerMap.ContainsKey(code);
		}

		public static void Initialize()
		{
			InstanceManagers.Clear();
			GlobalAgentActive.Clear();
			OpenGlobalIDs.FastClear();
			PeakGlobalID = 0;
			foreach (FastStack<LSAgent> cache in CachedAgents.Values)
			{
				for (int j = 0; j < cache.Count; j++)
				{
					cache.innerArray[j].SessionReset();
				}
			}


		}

		internal static FastBucket<LSAgent> DeathingAgents = new FastBucket<LSAgent>();

		public static void Deactivate()
		{

			for (int i = 0; i < PeakGlobalID; i++)
			{
				if (GlobalAgentActive[i])
				{
					DestroyAgent(GlobalAgents[i], true);
				}
			}
			CheckDestroyAgent();

			for (int i = 0; i < DeathingAgents.PeakCount; i++)
			{
				if (DeathingAgents.arrayAllocation[i])
				{
					LSAgent agent = DeathingAgents[i];
					AgentController.CompleteLife(agent);
				}
			}
			DeathingAgents.FastClear();

		}

		private static ushort GenerateGlobalID()
		{
			if (OpenGlobalIDs.Count > 0)
			{
				return OpenGlobalIDs.Pop();
			}
			return PeakGlobalID++;
		}

		public static bool TryGetAgentInstance(int globalID, out LSAgent returnAgent)
		{
			if (GlobalAgentActive[globalID])
			{
				returnAgent = GlobalAgents[globalID];
				return true;
			}
			returnAgent = null;
			return false;
		}



		public static void Simulate()
		{
			for (int iterator = 0; iterator < PeakGlobalID; iterator++)
			{
				if (GlobalAgentActive[iterator])
				{
					GlobalAgents[iterator].Simulate();
				}
			}

		}

		public static void LateSimulate()
		{
			for (int i = 0; i < PeakGlobalID; i++)
			{
				if (GlobalAgentActive[i])
					GlobalAgents[i].LateSimulate();
			}
			CheckDestroyAgent();


		}

		static void CheckDestroyAgent()
		{
			for (int i = 0; i < DeactivationBuffer.Count; i++)
			{
				DestroyAgentBuffer(DeactivationBuffer[i]);
			}
			DeactivationBuffer.FastClear();

		}

		public static void Visualize()
		{
			for (int iterator = 0; iterator < PeakGlobalID; iterator++)
			{
				if (GlobalAgentActive[iterator])
				{
					GlobalAgents[iterator].Visualize();
				}
			}
		}

		public static void LateVisualize()
		{
			for (int iterator = 0; iterator < PeakGlobalID; iterator++)
			{
				if (GlobalAgentActive[iterator])
				{
					GlobalAgents[iterator].LateVisualize();
				}
			}
		}

		public static void ClearAgents()
		{
			for (int i = GlobalAgents.Length - 1; i >= 0; i--)
			{
				if (GlobalAgentActive[i])
				{
					LSAgent agent = GlobalAgents[i];
					AgentController.DestroyAgent(agent);
				}
			}
		}

		public static void ChangeController(LSAgent agent, AgentController newCont)
		{

			AgentController leController = agent.Controller;
			if (leController != null)
			{
				leController.LocalAgentActive[agent.LocalID] = false;
				GlobalAgentActive[agent.GlobalID] = false;
				leController.OpenLocalIDs.Add(agent.LocalID);
				OpenGlobalIDs.Add(agent.GlobalID);

				if (newCont == null)
				{
					agent.InitializeController(null, 0, 0);
				}
				else
				{
					agent.Influencer.Deactivate();

					newCont.AddAgent(agent);
					agent.Influencer.Initialize();

				}
			}
		}

		public struct DeactivationData
		{
			public LSAgent Agent;
			public bool Immediate;

			public DeactivationData(LSAgent agent, bool immediate)
			{
				Agent = agent;
				Immediate = immediate;
			}
		}

		static FastList<DeactivationData> DeactivationBuffer = new FastList<DeactivationData>();

		public static void DestroyAgent(LSAgent agent, bool immediate = false)
		{
			DeactivationBuffer.Add(new DeactivationData(agent, immediate));

		}
		public const ushort UNREGISTERED_TYPE_INDEX = ushort.MaxValue;
		private static void DestroyAgentBuffer(DeactivationData data)
		{

			LSAgent agent = data.Agent;
			if (agent.IsActive == false)
				return;
			bool immediate = data.Immediate;

			agent.Deactivate(immediate);
			ChangeController(agent, null);

			//Pool if the agent is registered
			ushort agentCodeID;
			if (agent.TypeIndex != UNREGISTERED_TYPE_INDEX)
			{
				if (CodeIndexMap.TryGetValue(agent.MyAgentCode, out agentCodeID))
				{
					TypeAgentsActive[agentCodeID][agent.TypeIndex] = false;
				}
			}


		}
		/// <summary>
		/// Completes the life of the agent and pools or destroys it.
		/// </summary>
		/// <param name="agent">Agent.</param>
		public static void CompleteLife(LSAgent agent)
		{
			if (agent.CachedGameObject != null)
				agent.CachedGameObject.SetActive(false);
			if (agent.TypeIndex != UNREGISTERED_TYPE_INDEX)
			{
				AgentController.CacheAgent(agent);

			}
			else
			{
				//This agent was not registered for pooling. Let's destroy it
				GameObject.Destroy(agent);
			}
		}

		public static void CacheAgent(LSAgent agent)
		{
			if (LockstepManager.PoolingEnabled)
				CachedAgents[agent.MyAgentCode].Add(agent);
			else
				GameObject.Destroy(agent.gameObject);
		}

		private static void UpdateDiplomacy(AgentController newCont)
		{
			for (int i = 0; i < InstanceManagers.Count; i++)
			{
				var other = InstanceManagers[i];
				other.SetAllegiance(newCont, other.DefaultAllegiance);
				newCont.SetAllegiance(other, newCont.DefaultAllegiance);
			}
		}


		public static int GetStateHash()
		{
			int operationToggle = 0;
			int hash = LSUtility.PeekRandom(int.MaxValue);
			for (int i = 0; i < PeakGlobalID; i++)
			{
				if (GlobalAgentActive[i])
				{
					LSAgent agent = GlobalAgents[i];
					int n1 = agent.Body._position.GetHashCode() + agent.Body._rotation.GetStateHash();
					switch (operationToggle)
					{
						case 0:
							hash ^= n1;
							break;
						case 1:
							hash += n1;
							break;
						default:
							hash ^= n1 * 3;
							break;
					}
					operationToggle++;
					if (operationToggle >= 2)
					{
						operationToggle = 0;
					}
				}
			}


			return hash;
		}

		public static AgentController DefaultController { get { return InstanceManagers[0]; } }
		//TODO: Hide this list and use methods to access it
		//Also, move static AC stuff into its own class
		public static FastList<AgentController> InstanceManagers = new FastList<AgentController>();
		public readonly FastBucket<LSAgent> SelectedAgents = new FastBucket<LSAgent>();

		public bool SelectionChanged { get; set; }

		public readonly LSAgent[] LocalAgents = new LSAgent[MaxAgents];
		public readonly bool[] LocalAgentActive = new bool[MaxAgents];

		public byte ControllerID { get; private set; }

		public ushort PeakLocalID { get; private set; }

		public int PlayerIndex { get; set; }

		public bool HasTeam { get; private set; }

		public Team MyTeam { get; private set; }

		public AllegianceType DefaultAllegiance { get; private set; }

		private readonly FastList<AllegianceType> DiplomacyFlags = new FastList<AllegianceType>();
		private readonly FastStack<ushort> OpenLocalIDs = new FastStack<ushort>();

		private AgentCommander _commander;
		public AgentCommander Commander
		{
			get
			{
				if (_commander.IsNotNull())
					return _commander;
				else
				{
					return null;
				}
			}
		}
		public static AgentController GetInstanceManager(int index)
		{
			if (index >= AgentController.InstanceManagers.Count)
			{
				Debug.LogError("Controller with index " + index + " not created. You can automatically create controllers by configuring AgentControllerCreator.");
				return null;
			}
			else if (index < 0)
			{
				Debug.LogError("Controller cannot have negative index.");
			}
			return AgentController.InstanceManagers[index];
		}
		public static AgentController Create(AllegianceType defaultAllegiance = AllegianceType.Neutral)
		{
			return new AgentController(defaultAllegiance);
		}

		private AgentController(AllegianceType defaultAllegiance)
		{
			if (InstanceManagers.Count > byte.MaxValue)
			{
				throw new System.Exception("Cannot have more than 256 AgentControllers");
			}
			OpenLocalIDs.FastClear();
			PeakLocalID = 0;
			ControllerID = (byte)InstanceManagers.Count;
			DefaultAllegiance = defaultAllegiance;

			for (int i = 0; i < InstanceManagers.Count; i++)
			{
				this.SetAllegiance(InstanceManagers[i], AllegianceType.Neutral);
			}

			InstanceManagers.Add(this);
			UpdateDiplomacy(this);
			this.SetAllegiance(this, AllegianceType.Friendly);
		}

		public AgentCommander CreateCommander(string commanderAgentCode)
		{
			if (Commander != null)
				Debug.LogError("A commander called '" + Commander.Agent.CachedGameObject + "' already exists for '" + this.ToString() + "'.");
			LSAgent commanderAgent = CreateBareAgent(commanderAgentCode);

			_commander = commanderAgent.GetAbility<AgentCommander>();
			if (_commander == null)
				Debug.LogError("Agent '" + commanderAgentCode + "' does not have AgentCommander component and cannot be a commander.");

			return Commander;
		}

		public void AddToSelection(LSAgent agent)
		{
			if (agent.IsSelected == false)
			{
				SelectedAgents.Add(agent);
				SelectionChanged = true;
			}
		}

		public void RemoveFromSelection(LSAgent agent)
		{
			SelectedAgents.Remove(agent);
			SelectionChanged = true;
		}

		private Selection previousSelection = new Selection();

		public Selection GetSelection(Command com)
		{
			if (com.ContainsData<Selection>() == false)
			{
				return previousSelection;
			}
			return com.GetData<Selection>();
		}

		public void Execute(Command com)
		{
			if (com.ContainsData<Selection>())
			{
				previousSelection = com.GetData<Selection>();
			}

			BehaviourHelperManager.Execute(com);
			Selection selection = GetSelection(com);
			for (int i = 0; i < selection.selectedAgentLocalIDs.Count; i++)
			{
				ushort selectedAgentID = selection.selectedAgentLocalIDs[i];
				if (LocalAgentActive[selectedAgentID])
				{
					var agent = LocalAgents[selectedAgentID];
					//Prevent executing twice on commander
					if (Commander.IsNull() || agent != Commander.Agent)
					{
						agent.Execute(com);
					}
				}
			}
			if (Commander.IsNotNull())
				Commander.Agent.Execute(com);
		}

		//Backward compat.
		public static Command GenerateSpawnCommand(AgentController cont, string agentCode, int count, Vector2d position)
		{
			return Lockstep.Example.ExampleSpawner.GenerateSpawnCommand(cont, agentCode, count, position, "");
		}

		public void AddAgent(LSAgent agent)
		{
			ushort localID = GenerateLocalID();
			LocalAgents[localID] = agent;
			LocalAgentActive[localID] = true;

			ushort globalID = GenerateGlobalID();
			GlobalAgentActive[globalID] = true;
			GlobalAgents[globalID] = agent;

			agent.InitializeController(this, localID, globalID);
		}

		public event Action<LSAgent> onCreateAgent;

		public LSAgent CreateAgent(string agentCode, Vector2d position)
		{

			LSAgent agent = CreateAgent(agentCode, position, Vector2d.right);
			if (onCreateAgent != null)
				onCreateAgent(agent);
			return agent;
		}

		public static LSAgent GetAgentSource(string agentCode)
		{
			IAgentData interfacer = AgentController.CodeInterfacerMap[agentCode];
			return interfacer.GetAgent();
		}


		public static void RegisterRawAgent(LSAgent agent)
		{
			var agentCodeID = AgentController.GetAgentCodeIndex(agent.MyAgentCode);
			FastList<bool> typeActive;
			if (!AgentController.TypeAgentsActive.TryGetValue(agentCodeID, out typeActive))
			{
				typeActive = new FastList<bool>();
				TypeAgentsActive.Add(agentCodeID, typeActive);
			}
			FastList<LSAgent> typeAgents;
			if (!TypeAgents.TryGetValue(agentCodeID, out typeAgents))
			{
				typeAgents = new FastList<LSAgent>();
				TypeAgents.Add(agentCodeID, typeAgents);
			}

			//TypeIndex of ushort.MaxValue means that this agent isn't registered for the pool
			agent.TypeIndex = (ushort)(typeAgents.Count);
			typeAgents.Add(agent);
			typeActive.Add(true);
		}

		/// <summary>
		/// Create an uninitialized LSAgent
		/// </summary>
		/// <returns>The raw agent.</returns>
		/// <param name="agentCode">Agent code.</param>
		/// <param name="isBare">If set to <c>true</c> is bare.</param>
		public static LSAgent CreateRawAgent(string agentCode)
		{
			if (!IsValidAgentCode(agentCode))
			{
				throw new System.ArgumentException(string.Format("Agent code '{0}' not found.", agentCode));
			}
			FastStack<LSAgent> cache = CachedAgents[agentCode];
			LSAgent curAgent = null;

			if (cache.IsNotNull() && cache.Count > 0)
			{
				curAgent = cache.Pop();
				ushort agentCodeID = AgentController.GetAgentCodeIndex(agentCode);
				Debug.Log(curAgent.TypeIndex);
				TypeAgentsActive[agentCodeID][curAgent.TypeIndex] = true;
			}
			else
			{
				IAgentData interfacer = AgentController.CodeInterfacerMap[agentCode];

				curAgent = GameObject.Instantiate(AgentController.GetAgentTemplate(agentCode).gameObject).GetComponent<LSAgent>();
				curAgent.Setup(interfacer);

				RegisterRawAgent(curAgent);

			}
			return curAgent;
		}
		public LSAgent CreateAgent(string agentCode, Vector2d position, Vector2d rotation)
		{
			var agent = CreateRawAgent(agentCode);
			InitializeAgent(agent, position, rotation);

			return agent;
		}

		/// <summary>
		/// Creates and initializes an agent without activating LSBody, LSInfluencer, etc..
		/// </summary>
		/// <returns>The bare agent.</returns>
		/// <param name="agentCode">Agent code.</param>
		public LSAgent CreateBareAgent(string agentCode)
		{
			var agent = CreateRawAgent(agentCode);
			AddAgent(agent);
			agent.InitializeBare();
			return agent;
		}

		public void InitializeAgent(LSAgent agent,
									  Vector2d position,
									  Vector2d rotation)
		{
			AddAgent(agent);
			agent.Initialize(position, rotation);
		}

		private ushort GenerateLocalID()
		{
			if (OpenLocalIDs.Count > 0)
			{
				return OpenLocalIDs.Pop();
			}
			else
			{
				return PeakLocalID++;
			}
		}

		public void SetAllegiance(AgentController otherController, AllegianceType allegianceType)
		{
			while (DiplomacyFlags.Count <= otherController.ControllerID)
			{
				DiplomacyFlags.Add(AllegianceType.Neutral);
			}
			DiplomacyFlags[otherController.ControllerID] = allegianceType;
		}

		public AllegianceType GetAllegiance(AgentController otherController)
		{
			return HasTeam && otherController.HasTeam ? MyTeam.GetAllegiance(otherController) : DiplomacyFlags[otherController.ControllerID];
		}

		public AllegianceType GetAllegiance(byte controllerID)
		{
			if (HasTeam)
			{
				//TODO: Team allegiance
			}

			return DiplomacyFlags[controllerID];
		}

		public void JoinTeam(Team team)
		{
			MyTeam = team;
			HasTeam = true;
		}

		public void LeaveTeam()
		{
			HasTeam = false;
		}
	}

	//Implemented as flags for selecting multiple types.
	[System.Flags]
	public enum AllegianceType : byte
	{
		Neutral = 1 << 0,
		Friendly = 1 << 1,
		Enemy = 1 << 2,
		All = 0xff
	}

}