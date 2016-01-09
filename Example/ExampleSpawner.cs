using UnityEngine;
using System.Collections;

namespace Lockstep.Example
{
	public class ExampleSpawner : BehaviourHelper
	{
		public override ushort ListenInput {
			get {
				return InputCodeManager.GetCodeID("Spawn");
			}
		}
		/*
		 *			Command com = new Command (InputCodeManager.GetCodeID ("Spawn"));
            com.ControllerID = cont.ControllerID;
            com.Position = position;
            com.Target = (ushort)AgentController.GetAgentCodeIndex(agentCode);
            com.Count = count;
            return com; 
		 */
		protected override void OnExecute (Command com)
		{
			Debug.Log (com.HasPosition + " After");
			byte conID = com.ControllerID;
			Vector2d pos = com.Position;
			ushort target = com.Target;
			int count = com.Count;

			AgentController ac = AgentController.InstanceManagers [conID];
			string agentCode = AgentController.GetAgentCode (target);
			for (int i = 0; i < count; i++) {
				ac.CreateAgent (agentCode, pos);
			}
		}
	}
}
