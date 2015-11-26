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

<<<<<<< HEAD
			ReplayManager.Simulate ();
			PlayerManager.Simulate ();
			NetworkManager.Simulate ();

			if (!FrameManager.CanAdvanceFrame) {
				return;
			}
			else {

			}
			FrameManager.Simulate ();

			OnSimulate ();

			AgentController.Simulate ();
=======
using Lockstep.UI;
using UnityEngine;
//using Lockstep.Integration;
using Lockstep.Data;
namespace Lockstep {
    public static class LockstepManager {
		public static readonly System.Diagnostics.Stopwatch SimulationTimer = new System.Diagnostics.Stopwatch();
		public static long Ticks {get {return SimulationTimer.ElapsedTicks;}}
		public static MonoBehaviour UnityInstance {get; private set;}
        public const int FrameRate = 32;
        public const int InfluenceResolution = 4;
		public const float BaseDeltaTime = (float)(1d  / FrameRate);

		private static int InfluenceCount;

		public static int InfluenceFrameCount {get; private set;}
        public static int FrameCount { get; private set; }
		public static bool Started {get; private set;}

        public static GameManager MainGameManager {get; private set;}

        public static void Setup () {


			UnityInstance = GameObject.CreatePrimitive (PrimitiveType.Sphere).AddComponent<MonoBehaviour> ();
            UnityInstance.GetComponent<Renderer>().enabled = false;
			GameObject.DontDestroyOnLoad (UnityInstance.gameObject);

			AbilityInterfacer.Setup ();
         
            AgentController.Setup();
			TeamManager.Setup ();

            ProjectileManager.Setup();
            EffectManager.Setup();
            BehaviourHelper.GlobalSetup();
			PhysicsManager.Setup ();
			ClientManager.Setup (MainGameManager.MainNetworkHelper);
            InterfaceManager.Setup();

			Application.targetFrameRate = 30;
			Time.fixedDeltaTime = BaseDeltaTime;
			Time.maximumDeltaTime = Time.fixedDeltaTime * 2;

			InputManager.Setup ();
        }

        public static void Initialize(GameManager gameManager) {

            MainGameManager = gameManager;


			SimulationTimer.Reset ();
			SimulationTimer.Start ();
			LSDatabaseManager.Initialize();
            LSUtility.Initialize(1);
			Interfacing.Initialize ();
			InfluenceCount = 0;
			Time.timeScale = 1f;
			Stalled = true;

            FrameCount = 0;
			InfluenceFrameCount = 0;

            GridManager.Generate();
            GridManager.Initialize();

			TeamManager.Initialize ();

            CoroutineManager.Initialize();
			FrameManager.Initialize();

            CommandManager.Initialize();
			BehaviourHelper.GlobalInitialize();

            AgentController.Initialize();
			TeamManager.LateInitialize ();

            PhysicsManager.Initialize();
            PlayerManager.Initialize();
            SelectionManager.Initialize();
            InfluenceManager.Initialize();
            ProjectileManager.Initialize();

            LoadSceneObjects();

			Started = true;
            ClientManager.Initialize ();
        }

		static bool Stalled;
        public static void Simulate() {
			if (InfluenceCount == 0)
			{
				InfluenceSimulate ();
				InfluenceCount = InfluenceResolution - 1;
            	if (FrameManager.CanAdvanceFrame == false) {
					Stalled = true;
               		return;
           		}
				Stalled = false;
				FrameManager.Simulate();
				InfluenceFrameCount++;
			}
			else {
				InfluenceCount--;
			}
			if (Stalled){
				return;
			}
			if (FrameCount == 0) StartGame ();
			BehaviourHelper.GlobalSimulate();
			AgentController.Simulate();
            PhysicsManager.Simulate();
            CoroutineManager.Simulate();
            InfluenceManager.Simulate();
            ProjectileManager.Simulate();
            TestManager.Simulate ();
>>>>>>> origin/develop



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


		public static event SimulationEvent OnSimulate;
		public delegate void SimulationEvent ();

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