using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lockstep
{
	public class LockstepManager : MonoBehaviour
	{
		public static LockstepManager Instance;
		public const long Timestep = FixedMath.One / 32;
		public const int NetworkingIterationSpread = 2;
		public static int FrameCount;

		public static void Initialize ()
		{

			Time.fixedDeltaTime = FixedMath.ToFloat (Timestep);
			FrameCount = 0;
			NetworkManager.Initialize ();
			FrameManager.Initialize ();
			AgentController.Initialize (Instance.AgentObjects);
			PhysicsManager.Initialize ();
			InputManager.Initialize ();
			PlayerManager.Initialize ();
		}

		public static void Simulate ()
		{
			//FrameManager.EarlySimulate ();
			PlayerManager.Simulate ();
			NetworkManager.Simulate ();
			if (!FrameManager.CanAdvanceFrame) return;
			FrameManager.Simulate ();
			AgentController.Simulate ();

			//Custom code goes here

			PhysicsManager.Simulate ();
			CoroutineManager.Simulate ();
			InputManager.Simulate ();
			SelectionManager.Simulate ();
			FrameCount++;
		}

		public static void Visualize ()
		{
			PhysicsManager.Visualize ();
			InputManager.Visualize ();
			PlayerManager.Visualize ();
			AgentController.Visualize ();
		}


		#region Instance Settings

		public GameObject[] AgentObjects;
		public GameObject SelectionRing;

		void Awake ()
		{
			Instance = this;
		}
		#endregion
	}
}