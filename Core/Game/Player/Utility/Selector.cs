using UnityEngine;
using System;
namespace Lockstep {
    public static class Selector {
        public static event Action onChange;
        public static event Action onAdd;
        public static event Action onRemove;
		private static LSAgent _mainAgent;
        static Selector () {
            onAdd += Change;
            onRemove += Change;
        }
        private static void Change () {
            if (onChange != null)
                onChange();
        }
		public static LSAgent MainSelectedAgent {get {return _mainAgent;}
			private set{
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
            onAdd ();
        }

        public static void Remove(LSAgent agent) {
			agent.Controller.RemoveFromSelection (agent);
			agent.IsSelected = false;
			if (agent == MainSelectedAgent) {
				agent = SelectedAgents.Count > 0 ? SelectedAgents.PopMax () : null;
			}
            onRemove ();
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
            onRemove ();
        }
    }

}