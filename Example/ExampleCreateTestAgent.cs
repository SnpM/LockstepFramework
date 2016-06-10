using UnityEngine;
using System.Collections;
using Lockstep;

namespace Lockstep.Example
{
	public class ExampleCreateTestAgent : BehaviourHelper
	{
		protected FastList<LSAgent> spawnedAgents = new FastList<LSAgent> ();
		public string agentName = "TestAgent";
		public int agentsToSpawn = 2;

		// Use this for initialization
		void Start ()
		{
	
		}
	
		// Update is called once per frame
		void Update ()
		{
	
		}

		protected override void OnGameStart ()
		{
			AgentController ac = AgentController.Create ();
			PlayerManager.AddController (ac);

			for (int i = 0; i < agentsToSpawn; i++) {
				spawnedAgents.Add (ac.CreateAgent (agentName, Vector2d.zero));
			}
		}
	}
}