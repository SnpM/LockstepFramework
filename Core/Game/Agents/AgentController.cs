using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lockstep;

namespace Lockstep
{
	public class AgentController : MonoBehaviour
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
					if (AgentCodes[i] != null)
					{
						AgentObjects[(int)AgentCodes[i]] = agentObjects[i];
					}
				}
				FirstInitializeStatic = false;
			}
			OpenGlobalIDs.FastClear ();
			PeakGlobalID = 0;
			GlobalActiveAgents = new Dictionary<ushort, LSAgent> (1024);
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
		public const int MaxAgents = 256;
		public Dictionary<ushort,LSAgent> ActiveAgents;
		public byte ControllerID;

		public void InitializeLocal ()
		{
			ActiveAgents = new Dictionary<ushort, LSAgent> (256);
			OpenLocalIDs.FastClear ();
			PeakLocalID = 0;
			ControllerID = (byte)InstanceManagers.Count;
			InstanceManagers.Add (this);
		}

		public void SimulateLocal ()
		{
			foreach (LSAgent agent in ActiveAgents.Values)
			{
				agent.Simulate ();
			}
		}
		public void VisualizeLocal ()
		{
			foreach (LSAgent agent in ActiveAgents.Values)
			{
				agent.Visualize ();
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
				curAgent = Instantiate (AgentObjects[(int)agentCode]).GetComponent<LSAgent> ();
			}

			localID = GenerateLocalID ();
			curAgent.LocalID = localID;
			ActiveAgents.Add (localID, curAgent);

			globalID = GenerateGlobalID ();
			curAgent.GlobalID = globalID;

			curAgent.Initialize ();

			RingController ringController = Instantiate (LockstepManager.Instance.SelectionRing).GetComponent<RingController> ();
			ringController.Initialize (curAgent);
			curAgent.ringController = ringController;

			return curAgent;
		}

		public void DestroyAgent (LSAgent agent)
		{
			agent.Deactivate ();
			CachedAgents[(int)agent.MyAgentCode].Add (agent);
			OpenLocalIDs.Add (agent.LocalID);
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
		#endregion
	}
}