using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lockstep
{
	public class LockstepManager : MonoBehaviour
	{
		private static LockstepManager Instance;
		public const long Timestep = FixedMath.One / 32;
		public const int NetworkingIterationSpread = 2;
		public static int FrameCount;

		public static void Initialize ()
		{

			Time.fixedDeltaTime = FixedMath.ToFloat (Timestep);
			FrameCount = 0;
			NetworkManager.Initialize ();
			FrameManager.Initialize ();
			AgentController.Initialize (Instance.AllAgentCodes, Instance.AgentObjects);
			PhysicsManager.Initialize ();
			InputManager.Initialize ();
			PlayerController.Initialize ();
		}

		public static void Simulate ()
		{
			FrameManager.EarlySimulate ();
			PlayerController.Simulate ();
			NetworkManager.Simulate ();
			FrameManager.Simulate ();
			AgentController.Simulate ();


			PhysicsManager.Simulate ();
			CoroutineManager.Simulate ();
			InputManager.Simulate ();
			FrameCount++;
		}

		public static void Visualize ()
		{
			PhysicsManager.Visualize ();
			InputManager.Visualize ();
			PlayerController.Visualize ();
		}


		#region Instance Settings

		public GameObject[] AgentObjects;
		public AgentCode[] AllAgentCodes;

		void Awake ()
		{
			Instance = this;
		}
		#endregion
	}
}