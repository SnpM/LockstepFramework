//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================
using Lockstep.NetworkHelpers;

#if UNITY_EDITOR
#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.
#endif

/*
 * Call Pattern
 * ------------
 * Setup: Called once per run for setting up any values
 * Initialize: Called once per instance. On managers, called in new game. On agents, called when unpooled.
 * Simulate: Called once every simulation frame. 
 * Visualize: Called once every rendering/player interfacing frame
 * Deactivate: Called upon deactivation. On managers, called when game is ended. On agents, called when pooled.
 */

using Lockstep.UI;
using UnityEngine;

//using Lockstep.Integration;
using Lockstep.Data;
using System;

using FastCollections;
namespace Lockstep
{
	//TODO: Set up default functions to implement LSManager
	public static class  LockstepManager
	{
		public const int FrameRate = 32;
		public const int InfluenceResolution = 2;
		public const float BaseDeltaTime = (float)(1d / FrameRate);

		private static int InfluenceCount;

		public static int InfluenceFrameCount { get; private set; }

		public static int FrameCount { get; private set; }

		public static bool GameStarted { get; private set; }

		public static bool Loaded { get; private set; }

		//for testing purposes
		public static bool PoolingEnabled = true;

		public static event Action onSetup;
		public static event Action onInitialize;

		public static int PauseCount { get; private set; }

		public static bool IsPaused { get { return PauseCount > 0; } }

		public static NetworkHelper MainNetworkHelper;

		public static void Pause ()
		{
			PauseCount++;
		}

		public static void Unpause ()
		{
			PauseCount--;
		}

//		public static void Reset ()
//		{
//			LockstepManager.Deactivate ();
//			GameObject.Instantiate (MainGameManager.gameObject);
//		}

		internal static void Setup ()
		{
			DefaultMessageRaiser.EarlySetup ();

			LSDatabaseManager.Setup ();
			Command.Setup ();

			GridManager.Setup ();
			AbilityDataItem.Setup ();
         
			AgentController.Setup ();

			ProjectileManager.Setup ();
			EffectManager.Setup ();

			PhysicsManager.Setup ();
			ClientManager.Setup ();

			Time.fixedDeltaTime = BaseDeltaTime;
			Time.maximumDeltaTime = Time.fixedDeltaTime * 2;
			InputCodeManager.Setup ();


			DefaultMessageRaiser.LateSetup ();
			if (onSetup != null)
				onSetup ();


		}

		private static long _playRate = FixedMath.One;
		public static long PlayRate
		{
			get
			{
				return _playRate;
			}
			set
			{
				if (value != _playRate)
				{
					_playRate = value;
					Time.timeScale = PlayRate.ToFloat();
					//Time.fixedDeltaTime = BaseDeltaTime / _playRate.ToFloat();
				}
			}
		}

		public static float FloatPlayRate
		{
			get { return _playRate.ToFloat(); }
			set
			{
				PlayRate = FixedMath.Create(value);
			}
		}

		internal static void Initialize (BehaviourHelper[] helpers, NetworkHelper networkHelper)
		{
			PlayRate = FixedMath.One;
			//PauseCount = 0;

			if (!Loaded) {
				Setup ();
				Loaded = true;
			}



			DefaultMessageRaiser.EarlyInitialize ();

			LSDatabaseManager.Initialize ();
			LSUtility.Initialize (1);
			InfluenceCount = 0;
			Time.timeScale = 1f;

			Stalled = true;

			FrameCount = 0;
			InfluenceFrameCount = 0;
			MainNetworkHelper = networkHelper;

			BehaviourHelperManager.Initialize (helpers);
			ClientManager.Initialize (MainNetworkHelper);

			GridManager.Initialize ();


			CoroutineManager.Initialize ();
			FrameManager.Initialize ();

			CommandManager.Initialize ();

			AgentController.Initialize ();

			PhysicsManager.Initialize ();
			PlayerManager.Initialize ();
			SelectionManager.Initialize ();
			InfluenceManager.Initialize ();
			ProjectileManager.Initialize ();

			DefaultMessageRaiser.LateInitialize ();
			BehaviourHelperManager.LateInitialize ();
			if (onInitialize != null)
				onInitialize ();
		}

		static bool Stalled;

		internal static void Simulate ()
		{
			MainNetworkHelper.Simulate ();
			DefaultMessageRaiser.EarlySimulate ();
			if (InfluenceCount == 0) {
				InfluenceSimulate ();
				InfluenceCount = InfluenceResolution - 1;
				if (FrameManager.CanAdvanceFrame == false) {
					Stalled = true;
					return;
				}
				Stalled = false;
				if (InfluenceFrameCount == 0) {
					GameStart ();
				}
				FrameManager.Simulate ();
				InfluenceFrameCount++;
			} else {
				InfluenceCount--;
			}
			if (Stalled || IsPaused) {
				return;
			}


			BehaviourHelperManager.Simulate ();
			AgentController.Simulate ();
			PhysicsManager.Simulate ();
			CoroutineManager.Simulate ();
			InfluenceManager.Simulate ();
			ProjectileManager.Simulate ();

			LateSimulate ();
			FrameCount++;

		}

		private static void GameStart ()
		{
			BehaviourHelperManager.GameStart ();
			GameStarted = true;
		}

		private static void LateSimulate ()
		{
			BehaviourHelperManager.LateSimulate ();
			AgentController.LateSimulate ();
			PhysicsManager.LateSimulate ();
			DefaultMessageRaiser.LateSimulate ();
		}

		internal static void InfluenceSimulate ()
		{
			PlayerManager.Simulate ();
			CommandManager.Simulate ();
			ClientManager.Simulate ();
		}

		internal static void Execute (Command com)
		{
			if (!GameStarted) {
				Debug.LogError ("BOOM");
				return;
			}
			if (com.ControllerID != byte.MaxValue) {
				AgentController cont = AgentController.InstanceManagers [com.ControllerID];
				cont.Execute (com);
			} else {
				BehaviourHelperManager.Execute (com);
			}

			DefaultMessageRaiser.Execute (com);

		}

		internal static void Visualize ()
		{
			if (!GameStarted)
				return;
			DefaultMessageRaiser.EarlyVisualize ();
			PlayerManager.Visualize ();
			BehaviourHelperManager.Visualize ();
			AgentController.Visualize ();
			ProjectileManager.Visualize ();
			EffectManager.Visualize ();
			CommandManager.Visualize();

		}

		internal static void LateVisualize ()
		{
			DefaultMessageRaiser.LateVisualize ();
			AgentController.LateVisualize();
			PhysicsManager.LateVisualize();
			BehaviourHelperManager.LateVisualize();
		}

		internal static void Deactivate ()
		{
			DefaultMessageRaiser.EarlyDeactivate ();

			if (GameStarted == false)
				return;
			Selector.Clear ();
			AgentController.Deactivate ();
			BehaviourHelperManager.Deactivate ();
			ProjectileManager.Deactivate ();
			EffectManager.Deactivate ();
			ClientManager.Deactivate ();

			ClientManager.Quit ();
			PhysicsManager.Deactivate ();
			GameStarted = false;
			LSServer.Deactivate ();
			DefaultMessageRaiser.LateDeactivate ();
			CoroutineManager.Deactivate();
		}

		public static void Quit ()
		{
			ClientManager.Quit ();
		}

		public static int GetStateHash ()
		{
			int hash = LSUtility.PeekRandom (int.MaxValue);
			hash += 1;
			hash ^= AgentController.GetStateHash ();
			hash += 1;
			hash ^= ProjectileManager.GetStateHash ();
			hash += 1;
			return hash;
		}
	}
}