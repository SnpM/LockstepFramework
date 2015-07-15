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
		public static Dictionary<AgentCode,GameObject> AgentObjects;
		public static Dictionary<AgentCode,FastStack<LSAgent>> CachedAgents;

		private static bool FirstInitializeStatic = true;
		public static void Initialize (AgentCode[] agentTypes, GameObject[] gameObjects)
		{
			if (FirstInitializeStatic)
			{
				CachedAgents = new Dictionary<AgentCode, FastStack<LSAgent>>(agentTypes.Length);
				AgentObjects = new Dictionary<AgentCode, GameObject> (agentTypes.Length);
				for (i = 0; i < agentTypes.Length; i++)
				{
					if (gameObjects[i] != null)
					{
						AgentObjects.Add (agentTypes[i],gameObjects[i]);
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
			InstanceManagers.Add (this);
		}

		public void SimulateLocal ()
		{
			foreach (LSAgent agent in ActiveAgents.Values)
			{
				agent.Simulate ();
			}
		}

		public LSAgent CreateAgent (AgentCode agentType)
		{
			FastStack<LSAgent> cache;

			CachedAgents.TryGetValue (agentType, out cache);
			if (cache != null && cache.Count > 0)
			{
				curAgent = cache.Pop ();
			}
			else {
				curAgent = Instantiate (AgentObjects[agentType]).GetComponent<LSAgent> ();
			}

			localID = GenerateLocalID ();
			curAgent.LocalID = localID;
			ActiveAgents.Add (localID, curAgent);

			globalID = GenerateGlobalID ();
			curAgent.GlobalID = globalID;

			curAgent.Initialize ();

			return curAgent;
		}

		public void DestroyAgent (LSAgent agent)
		{
			agent.Deactivate ();
			CachedAgents[agent.MyAgentCode].Add (agent);
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