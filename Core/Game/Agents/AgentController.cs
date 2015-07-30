using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lockstep;

namespace Lockstep
{
	public class AgentController
	{
		#region Static Stuff
		static int i,j;
		static ushort localID, globalID;
		static LSAgent curAgent;
		public static AgentCode[] AgentCodes;
		public static GameObject[] AgentObjects;
		public static FastStack<LSAgent>[] CachedAgents;

		private static bool FirstInitializeStatic = true;
		public static void Initialize (GameObject[] agentObjects)
		{
			if (FirstInitializeStatic)
			{
				AgentCodes = (AgentCode[])System.Enum.GetValues (typeof(AgentCode));
				CachedAgents = new FastStack<LSAgent>[AgentCodes.Length];
				AgentObjects = new GameObject[AgentCodes.Length];
				for (i = 0; i < AgentCodes.Length; i++)
				{
					AgentObjects[(int)AgentCodes[i]] = agentObjects[i];
				}
				FirstInitializeStatic = false;
			}
			GlobalActiveAgents = new Dictionary<ushort, LSAgent> (1024);

			OpenGlobalIDs.FastClear ();
			PeakGlobalID = 0;
			InstanceManagers.Clear ();

		}
		public static Dictionary<ushort,LSAgent> GlobalActiveAgents;
		private static FastStack<ushort> OpenGlobalIDs = new FastStack<ushort> ();
		private static ushort PeakGlobalID;
		public static ushort GenerateGlobalID ()
		{
			if (OpenGlobalIDs.Count > 0)
			{
				return OpenGlobalIDs.Pop ();
			}
			return PeakGlobalID++;
		}

		public static FastList<AgentController> InstanceManagers = new FastList<AgentController> ();
		public static void Simulate ()
		{
			for (i = 0; i < InstanceManagers.Count; i++)
			{
				InstanceManagers[i].SimulateLocal ();
			}
		}
		public static void Visualize ()
		{
			for (i = 0; i < InstanceManagers.Count; i++)
			{
				InstanceManagers[i].VisualizeLocal ();
			}
		}
		public static AgentController Create ()
		{
			AgentController controller = new AgentController();
			controller.InitializeLocal ();
			return controller;
		}
		#endregion


		#region Instance
		public const int MaxAgents = 512;
		public LSAgent[] Agents = new LSAgent[MaxAgents];
		public bool[] AgentActive = new bool[MaxAgents];
		public byte ControllerID;
		public FastList<AllegianceType> DiplomacyFlags = new FastList<AllegianceType>();

		public void InitializeLocal ()
		{
			OpenLocalIDs.FastClear ();
			PeakLocalID = 0;
			ControllerID = (byte)InstanceManagers.Count;
			InstanceManagers.Add (this);

			while (DiplomacyFlags.Count < InstanceManagers.Count)
			{
				DiplomacyFlags.Add (AllegianceType.Neutral);
			}
		}

		public void SimulateLocal ()
		{
			for (i = 0; i < MaxAgents; i++)
			{
				if (AgentActive[i])
					Agents[i].Simulate ();
			}
		}

		public void VisualizeLocal ()
		{
			for (i = 0; i < MaxAgents;i++)
			{
				if (AgentActive[i])
					Agents[i].Visualize();
			}
		}


		public void Execute (Command com)
		{
			Selection selection = com.Select;
			for (i = 0; i < selection.selectedAgentLocalIDs.Count; i++)
			{
				Agents[selection.selectedAgentLocalIDs[i]].Execute (com);
			}
		}

		public LSAgent CreateAgent (AgentCode agentCode)
		{
			FastStack<LSAgent> cache = CachedAgents[(int)agentCode];
			if (cache != null && cache.Count > 0)
			{
				curAgent = cache.Pop ();
			}
			else {
				curAgent = GameObject.Instantiate (AgentObjects[(int)agentCode]).GetComponent<LSAgent> ();
			}

			localID = GenerateLocalID ();
			curAgent.LocalID = localID;
			Agents[localID] = curAgent;
			AgentActive[localID] = true;

			globalID = GenerateGlobalID ();
			curAgent.GlobalID = globalID;

			curAgent.Initialize (this);

			RingController ringController = GameObject.Instantiate (LockstepManager.Instance.SelectionRing).GetComponent<RingController> ();
			ringController.Initialize (curAgent);
			curAgent.ringController = ringController;

			return curAgent;
		}

		public void DestroyAgent (LSAgent agent)
		{
			agent.Deactivate ();
			CachedAgents[(int)agent.MyAgentCode].Add (agent);
			OpenLocalIDs.Add (agent.LocalID);
			AgentActive[agent.LocalID] = false;
		}

		private FastStack<ushort> OpenLocalIDs = new FastStack<ushort> ();
		private ushort PeakLocalID;
		public ushort GenerateLocalID ()
		{
			if (OpenLocalIDs.Count > 0)
			{
				return OpenLocalIDs.Pop ();
			}
			else {
				return PeakLocalID++;
			}
		}

		public void SetAllegiance (AgentController otherController, AllegianceType allegianceType)
		{
			DiplomacyFlags[otherController.ControllerID] = allegianceType;
		}
		#endregion
	}

	public enum AllegianceType : byte {
		Neutral,
		Friendly,
		Enemy
	}
}