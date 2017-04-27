using UnityEngine;
using System.Collections; using FastCollections;
namespace Lockstep
{
	public class AttackerInfo
	{
		public AttackerInfo (LSAgent attacker, AgentController controller)
		{
			Attacker = attacker;
			Controller = controller;
		}
		public LSAgent Attacker;
		public AgentController Controller;
	}
}