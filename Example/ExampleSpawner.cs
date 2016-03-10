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


        protected FastList<LSAgent> bufferSpawnedAgents = new FastList<LSAgent>();
		protected override void OnExecute (Command com)
		{
			byte conID = com.ControllerID;
            Vector2d pos = com.GetData<Vector2d>();
            ushort target = (ushort)com.GetData<DefaultData>(0).Value;
            ushort count = (ushort)com.GetData<DefaultData>(1).Value;

			AgentController ac = AgentController.InstanceManagers [conID];
			string agentCode = AgentController.GetAgentCode (target);
            bufferSpawnedAgents.FastClear();
			for (int i = 0; i < count; i++) {
				LSAgent agent = ac.CreateAgent (agentCode, pos);
                bufferSpawnedAgents.Add(agent);
			}
		}

        public static Command GenerateSpawnCommand(AgentController cont, string agentCode, int count, Vector2d position)
        {
            Command com = new Command (InputCodeManager.GetCodeID ("Spawn"));
            com.ControllerID = cont.ControllerID;
            com.Add<Vector2d>(position);

            com.Add<DefaultData>(new DefaultData(DataType.UShort,(ushort)AgentController.GetAgentCodeIndex(agentCode)));
            com.Add<DefaultData>(new DefaultData(DataType.UShort,(ushort)count));
            return com;

        }
	}
}
