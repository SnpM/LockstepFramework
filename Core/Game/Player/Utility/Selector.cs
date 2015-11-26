using UnityEngine;
namespace Lockstep {
    public static class Selector {
		private static LSAgent _mainAgent;
		public static LSAgent MainSelectedAgent {get {return _mainAgent;}
			private set{
				if (value .IsNotNull ())
				{
					CommandCard.Inject (value.Interfacers);
				}
				else {
					CommandCard.Reset ();
				}
				_mainAgent = value;
			}
		}
		private static FastSorter<LSAgent> SelectedAgents;

		public static void Initialize (){

		}

        public static void Add(LSAgent agent) {
			agent.Controller.AddToSelection (agent);
            agent.IsSelected = true;
			if (MainSelectedAgent == null) MainSelectedAgent = agent;
        }

        public static void Remove(LSAgent agent) {
			agent.Controller.RemoveFromSelection (agent);
			agent.IsSelected = false;
			if (agent == MainSelectedAgent) {
				agent = SelectedAgents.Count > 0 ? SelectedAgents.PopMax () : null;
			}
        }

        public static void Clear() {
			for (int i = 0; i < PlayerManager.AgentControllerCount; i++)
			{
				FastBucket<LSAgent> selectedAgents = PlayerManager.AgentControllers[i].SelectedAgents;
				for (int j = 0; j < selectedAgents.PeakCount; j++) {
					if (selectedAgents.arrayAllocation[j]) {
						selectedAgents[j].IsSelected = false;
					}
				}
				selectedAgents.FastClear ();
			}
			MainSelectedAgent = null;
        }
    }

}