using System;
using UnityEngine;
using FastCollections;

namespace Lockstep
{
    public static class PlayerManager
    {

        public static readonly FastBucket<AgentController> AgentControllers = new FastBucket<AgentController>();

        public static AgentController MainController { get; private set; }

        public static void Initialize()
        {
            MainController = null;
            AgentControllers.FastClear();

        }

        public static void Simulate()
        {
        }

        public static void Visualize()
        {


        }

        public static int AgentControllerCount
        {
            get { return AgentControllers.Count; }
        }

        public static AgentController GetAgentController(int index)
        {
            return AgentControllers [index];
        }


        public static void AddController(AgentController agentController)
        {
            if (PlayerManager.ContainsController(agentController))
            {
                Debug.Log("BOOM");
                return;
            }

            agentController.PlayerIndex = AgentControllers.Add(agentController);
			if (MainController == null)
			{
				MainController = agentController;
			}
        }

        public static void RemoveController(AgentController agentController)
        { 
            Selector.Clear();
            AgentControllers.RemoveAt(agentController.PlayerIndex);
            if (MainController == agentController)
            {
                if (AgentControllers.Count == 0)
                    MainController = null;
                else
                {
                    for (int i = 0; i < AgentControllers.PeakCount; i++)
                    {
                        if (AgentControllers.arrayAllocation [i])
                        {
                            MainController = AgentControllers [i];
                            break;
                        }
                    }
                }
            }
        }

        public static void ClearControllers()
        {
            Selector.Clear();
            while (MainController != null)
            {
                RemoveController(MainController);
            }
            //return;
            //MainController = null;
            //AgentControllers.FastClear();
        }

        public static bool ContainsController(AgentController controller)
        {
			if (AgentControllers == null) Debug.Log(controller);
            return controller.PlayerIndex < AgentControllers.PeakCount && AgentControllers.ContainsAt(controller.PlayerIndex, controller);
        }

        public static AllegianceType GetAllegiance(AgentController otherController)
        {
            if (Selector.MainSelectedAgent != null)
                return Selector.MainSelectedAgent.Controller.GetAllegiance(otherController);
            if (MainController == null)
                return AllegianceType.Neutral;
            return MainController.GetAllegiance(otherController);
        }

        public static AllegianceType GetAllegiance(LSAgent agent)
        {
            return PlayerManager.GetAllegiance(agent.Controller);
        }

        /// <summary>
        /// Sends the command for all AgentControllers under the control of this PlayerManager...
        /// Mainly for shared control capabilities
        /// </summary>
        /// <param name="com">COM.</param>
        public static void SendCommand(Command com)
        {
            com.Add<Selection>(new Selection());
            for (int i = 0; i < AgentControllers.PeakCount; i++)
            {
                if (AgentControllers.arrayAllocation [i])
                {
                    AgentController cont = AgentControllers [i];

					if (cont.SelectedAgents.Count > 0)
                    {
                        com.ControllerID = cont.ControllerID;

						#if false
                        if (cont.SelectionChanged)
                        {
                            com.SetData<Selection>(new Selection(cont.SelectedAgents));
                            cont.SelectionChanged = false;

						} else
                        {
                            com.ClearData<Selection>();
                        }
						#else
						//we always sending selection data
						com.SetData<Selection>(new Selection(cont.SelectedAgents));
						cont.SelectionChanged = false;
						#endif
                        CommandManager.SendCommand(com);
                    }
                }
            }
        }

    }
}