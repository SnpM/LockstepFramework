using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;

namespace Lockstep
{
	public class AutoSpawner : BehaviourHelper
	{
		[SerializeField]
		private SpawnInfo[] Spawns;
		public bool AutoCommand = true;

		protected override void OnInitialize ()
		{
		}

        protected override void OnVisualize()
        {
            if (Input.GetKeyDown (KeyCode.M))
            {
                for (int i = 0; i < Spawns.Length; i++)
                {
                    SpawnInfo info = Spawns[i];
                    
					/*while (AgentController.InstanceManagers.Count <= info.ControllerIndex)
                    {
						
                        AgentController cont = AgentController.Create();
                        PlayerManager.AddController(cont);
                        for (int j = 0; j < AgentController.InstanceManagers.Count; j++)
                        {
                            AgentController ac = AgentController.InstanceManagers[j];
                            if (ac != cont)
                            {
                                cont.SetAllegiance(ac, AllegianceType.Enemy);
                                ac.SetAllegiance(cont, AllegianceType.Enemy);
                            }
                        }

                    }*/
					if (info.ControllerIndex >= AgentController.InstanceManagers.Count)
						Debug.LogError ("Controller with index " + info.ControllerIndex + " not created. You can automatically create controllers by configuring AgentControllerCreator.");

                    AgentController controller = AgentController.InstanceManagers[info.ControllerIndex];

                    for (int j = 0; j < info.Count; j++)
                    {
                        LSAgent agent = controller.CreateAgent(info.AgentCode, info.Position);
                        if (AutoCommand)
                            Selector.Add(agent);
                    }
                }
            }
        }
        protected override void OnGameStart ()
		{


			for (int i = 0; i < Spawns.Length; i++) {
				SpawnInfo info = Spawns [i];
				/*while (AgentController.InstanceManagers.Count <= info.ControllerIndex) {

					AgentController cont = AgentController.Create ();
					AgentControllerTool.SetFullHostile (cont);
					PlayerManager.AddController (cont);
	

				}
				*/
				if (info.ControllerIndex >= AgentController.InstanceManagers.Count)
					Debug.LogError ("Controller with index " + info.ControllerIndex + " not created. You can automatically create controllers by configuring AgentControllerCreator.");


				AgentController controller = AgentController.InstanceManagers [info.ControllerIndex];

				//add default controller if necessary
				if (info.ControllerIndex == 0 && PlayerManager.AgentControllers.Count == 0)
					PlayerManager.AddController(controller);
				for (int j = 0; j < info.Count; j++) {
					LSAgent agent = controller.CreateAgent (info.AgentCode, info.Position);
					if (AutoCommand)
					Selector.Add (agent);
				}
			}

			if (AutoCommand) {

				//Find average of spawn positions
				Vector2d battlePos = Vector2d.zero;
				for (int i = 0; i < Spawns.Length; i++) {
					battlePos += Spawns [i].Position;
				}
				battlePos /= Spawns.Length;
				Command com = new Command (Lockstep.Data.AbilityDataItem.FindInterfacer<Scan> ().ListenInputID);
				com.Add<Vector2d> (battlePos);

				PlayerManager.SendCommand (com);
				Selector.Clear ();

			}
		}
	}

	[System.Serializable]
	public struct SpawnInfo
	{

		[DataCode ("Agents")]
		public string AgentCode;
		public int Count;
		public int ControllerIndex;
		public Vector2d Position;
	}
}