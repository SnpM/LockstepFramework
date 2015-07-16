using UnityEngine;
using System.Collections;
namespace Lockstep {
public class PlayerController  : MonoBehaviour{
		public static FastList<AgentController> agentControllers = new FastList<AgentController>();
		public static Camera mainCamera;

		private static bool FirstInitialize = true;
		public static void Initialize ()
		{
			if (FirstInitialize)
			{
				mainCamera = Camera.main;
				FirstInitialize = false;
			}
		}

		public static void Simulate ()
		{
			SelectionManager.Simulate ();
		}

		public static void Visualize ()
		{
			SelectionManager.Visualize ();
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