using UnityEngine;
using System.Collections.Generic;
using System;
namespace Lockstep {
public class PlayerManager  : MonoBehaviour{
		public static FastList<AgentController> agentControllers = new FastList<AgentController>();
		public static bool[] HasAgentController = new bool[4];
		public static Camera mainCamera;

		private static bool FirstInitialize = true;
		public static void Initialize ()
		{

			if (FirstInitialize)
			{
				FirstInitialize = false;
			}
			mainCamera = Camera.main;
			agentControllers.FastClear ();
			Array.Clear (HasAgentController,0,HasAgentController.Length);

		}

		public static void Simulate ()
		{
			SelectionManager.Simulate ();
		}

		public static void Visualize ()
		{
			SelectionManager.Update ();
			if (Input.GetMouseButtonDown(1))
			{

				Command com = new Command(agentControllers[0].ControllerID,InputCode.M);
				Selection select = new Selection();
				select.SerializeFromSelectionManager ();
				com.Select = select;
				com.Position = new Vector2d(SelectionManager.MouseWorldPosition);
				NetworkManager.SendCommand (com);
			}
		}

		public static void AddAgentController(AgentController agentController)
		{
			agentControllers.Add (agentController);
			if (agentController.ControllerID >= HasAgentController.Length)
			{
				Array.Resize (ref HasAgentController, HasAgentController.Length * 2);
			}
			HasAgentController[agentController.ControllerID] = true;
		}




		#region Instance Behaviors
		public GUIStyle SelectionBoxStyle = new GUIStyle();

		void OnGUI ()
		{
			SelectionManager.DrawBox (SelectionBoxStyle);
		}

		void OnDrawGizmos ()
		{
			SelectionManager.DrawRealWorldBox ();
		}
		#endregion
	}
}