//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================
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
			LSUtility.Initialize (1);
			CoroutineManager.Initialize ();

			NetworkManager.Initialize ();
			FrameManager.Initialize ();
			AgentController.Initialize (Instance.AgentObjects);
			PhysicsManager.Initialize ();
			InputManager.Initialize ();
			PlayerManager.Initialize ();

			MovementGroup.Initialize ();

			Initialized = true;
		}

		public static void Simulate ()
		{
			if (!Initialized)
				return;

			ReplayManager.Simulate ();
			PlayerManager.Simulate ();
			NetworkManager.Simulate ();

			if (!FrameManager.CanAdvanceFrame) {
				return;
			}
			else {

			}
			FrameManager.Simulate ();

			#region Custom Behaviors
			MovementGroup.Simulate ();
			#endregion

			AgentController.Simulate ();



			PhysicsManager.Simulate ();
			CoroutineManager.Simulate ();
			InputManager.Simulate ();
			SelectionManager.Simulate ();
			FrameCount++;
		}

		public static void Visualize ()
		{
			if (!Initialized)
				return;
			PhysicsManager.Visualize ();
			InputManager.Visualize ();
			PlayerManager.Visualize ();
			AgentController.Visualize ();
		}

		public static bool Initialized = false;

		public static void End ()
		{
			Initialized = false;
		}


		#region Instance Settings

		[SerializeField]
		public GameObject[]
			AgentObjects;
		[SerializeField]
		public GameObject
			SelectionRing;

		void Awake ()
		{
			Instance = this;
		}


		#endregion
	}
}