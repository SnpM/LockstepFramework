using System;
using UnityEngine;

namespace Lockstep {
	public class PlayerManager : MonoBehaviour {
		[SerializeField]
		private GUIStyle selectionBoxStyle = new GUIStyle();
		[SerializeField]
		private GameObject _orderMarker;
		public static Marker OrderMarker {get; private set;}
		
		private static PlayerManager instance;
		
		public static SelectionSetting selectionSetting {get; private set;}
		public static readonly FastBucket<AgentController> AgentControllers = new FastBucket<AgentController>();
		public static AgentController MainController {get; private set;}
		public static bool IsInterfacing {
			get {
				return InterfaceManager.IsGathering;
			}
		}

		void Awake () {
            instance = this;
        }

		
		public static void Initialize(SelectionSetting selSetting = SelectionSetting.PC_RTS) {
			AgentControllers.FastClear();
			selectionSetting = selSetting;

			OrderMarker =
                instance._orderMarker != null ? GameObject.Instantiate (instance._orderMarker).GetComponent<Marker> ():
                GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<Marker> ();

			InterfaceManager.Initialize ();
		}
		
		public static void Simulate() {}
		
		public static void Visualize() {
			switch (selectionSetting) {
			case SelectionSetting.PC_RTS:
				SelectionManager.Update();
				CommandCard.Visualize ();
				InterfaceManager.Visualize ();
				break;
			case SelectionSetting.Mobile:

				break;
			}
		}
		
		public static int AgentControllerCount {
			get { return AgentControllers.Count; }
		}
		
		public static AgentController GetAgentController(int index) {
			return AgentControllers[index];
		}
		
        public static AgentController CreateController () {
            AgentController ac = new AgentController();
            return ac;
        }

		public static void AddController(AgentController agentController) {
			agentController.PlayerIndex = AgentControllers.Add(agentController);
			if (MainController == null) MainController = agentController;
		}

        public static void RemoveController (AgentController agentController) { 
            Selector.Clear();
            AgentControllers.RemoveAt(agentController.PlayerIndex);
            if (MainController == agentController) {
                if (AgentControllers.Count == 0)
                    MainController = null;
                else
                {
                    for (int i = 0; i < AgentControllers.PeakCount; i++) {
                        if (AgentControllers.arrayAllocation[i]) {
                            MainController = AgentControllers[i];
                            break;
                        }
                    }
                }
            }
        }

		public static bool ContainsController (AgentController controller) {
            return controller.PlayerIndex < AgentControllers.PeakCount && AgentControllers.ContainsAt(controller.PlayerIndex,controller);
		}
		
		public static AllegianceType GetAllegiance (AgentController otherController)
		{
            if (Selector.MainSelectedAgent != null) return Selector.MainSelectedAgent.Controller.GetAllegiance(otherController);
			if (MainController == null) return AllegianceType.Neutral;
			return MainController.GetAllegiance (otherController);
		}
		public static AllegianceType GetAllegiance (LSAgent agent)
		{
			return PlayerManager.GetAllegiance (agent.Controller);
		}

		public static void SendCommand (Command com)
		{
			com.Select = new Selection ();
			for (int i = 0; i < AgentControllers.Count; i++)
			{
				AgentController cont = AgentControllers[i];
				if (cont.SelectedAgents.Count > 0)
				{
					if (cont.SelectionChanged)
					{
						com.Select = new Selection(cont.SelectedAgents);
						cont.SelectionChanged = false;
					}
					else {
						com.HasSelect = false;
					}
					com.ControllerID = cont.ControllerID;
					com.Select.Serialize (AgentControllers[i].SelectedAgents);
					CommandManager.SendCommand (com);
				}
			}
		}
		
		private void OnGUI() {
			switch (selectionSetting) {
			case SelectionSetting.PC_RTS:
				SelectionManager.DrawBox(selectionBoxStyle);
				break;
			case SelectionSetting.Mobile:
				break;
			}
		}
	}
}