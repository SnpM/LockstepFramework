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
			byte conID = com.ControllerID;
            Vector2d pos = com.GetData<Vector2d>(0);
            Vector2d rot = com.GetData<Vector2d>(1);
            ushort target = (ushort)com.GetData<DefaultData>(0).Value;
            ushort count = (ushort)com.GetData<DefaultData>(1).Value;

			AgentController ac = AgentController.InstanceManagers [conID];
			string agentCode = AgentController.GetAgentCode (target);
			for (int i = 0; i < count; i++) {
				ac.CreateAgent (agentCode, pos, rot);

			}
		}
	}
}