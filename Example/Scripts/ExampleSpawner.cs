using UnityEngine;
using System.Collections; using FastCollections;

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

        protected ushort cacheTarget;
        protected ushort cacheCount;
        protected string cacheAgentCode;
		protected string cacheUID;
		protected override void OnExecute (Command com)
		{
			byte conID = com.ControllerID;
            Vector2d pos = com.GetData<Vector2d>();

			ProcessSpawn(
				conID,
				pos,
				(ushort)com.GetData<DefaultData>(0).Value,
				(ushort)com.GetData<DefaultData>(1).Value,
				(string)com.GetData<DefaultData>(2).Value);
		}
		public virtual void ProcessSpawn (byte conID, Vector2d pos, ushort agentCodeID, ushort count, string UID)
		{
			cacheTarget = agentCodeID;
			cacheCount = count;
			cacheUID = UID; 
			AgentController ac = AgentController.InstanceManagers[conID];
			cacheAgentCode = AgentController.GetAgentCode(cacheTarget);
			bufferSpawnedAgents.FastClear();
			for (int i = 0; i < cacheCount; i++)
			{
				LSAgent agent = ac.CreateAgent(cacheAgentCode, pos);
				bufferSpawnedAgents.Add(agent);
			}
		}

        public static Command GenerateSpawnCommand(AgentController cont, string agentCode, int count, Vector2d position, string uid)
        {

            Command com = new Command (InputCodeManager.GetCodeID ("Spawn"));
            com.ControllerID = cont.ControllerID;
            com.Add<Vector2d>(position);

            com.Add<DefaultData>(new DefaultData(DataType.UShort,(ushort)AgentController.GetAgentCodeIndex(agentCode)));
            com.Add<DefaultData>(new DefaultData(DataType.UShort,(ushort)count));
			com.Add<DefaultData>(new DefaultData(DataType.String, (string)uid));
            return com;

        }
	}
}
