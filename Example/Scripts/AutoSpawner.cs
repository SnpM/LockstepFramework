using UnityEngine;

namespace Lockstep
{
	public class AutoSpawner : BehaviourHelper
	{
		[SerializeField]
		private SpawnInfo[] Spawns;
		public bool AutoCommand = true;

		protected override void OnInitialize()
		{
		}

		public void LaunchSpawns()
		{

			for (int i = 0; i < Spawns.Length; i++)
			{
				SpawnInfo info = Spawns[i];

				var controller = AgentControllerHelper.Instance.GetInstanceManager(info.ControllerCode);

				for (int j = 0; j < info.Count; j++)
				{
					LSAgent agent = controller.CreateAgent(info.AgentCode, info.Position);
					if (AutoCommand)
						Selector.Add(agent);
				}
			}

			if (AutoCommand)
			{

				//Find average of spawn positions
				Vector2d battlePos = Vector2d.zero;
				for (int i = 0; i < Spawns.Length; i++)
				{
					battlePos += Spawns[i].Position;
				}
				battlePos /= Spawns.Length;
				Command com = new Command(Lockstep.Data.AbilityDataItem.FindInterfacer<Scan>().ListenInputID);
				com.Add<Vector2d>(battlePos);

				PlayerManager.SendCommand(com);
				Selector.Clear();

			}
		}
		protected override void OnVisualize()
		{
			if (Input.GetKeyDown(KeyCode.M))
			{
				LaunchSpawns();
			}
		}
		protected override void OnGameStart()
		{
			LaunchSpawns();

		}
	}

	[System.Serializable]
	public struct SpawnInfo
	{

		[DataCode("Agents")]
		public string AgentCode;
		public int Count;
		[DataCode("AgentControllers")]
		public string ControllerCode;
		public Vector2d Position;
	}
}