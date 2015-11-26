//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================

#if UNITY_EDITOR
#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.
#endif

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

			TeamManager.Simulate ();

			LateSimulate ();
            FrameCount++;

        }
		private static void StartGame () {
			GameManager.StartGame ();
		}
		private static void LateSimulate () {
            BehaviourHelper.GlobalLateSimulate ();
			AgentController.LateSimulate ();
			PhysicsManager.LateSimulate ();
		}
		public static void InfluenceSimulate () {
			PlayerManager.Simulate();
			CommandManager.Simulate();
			ClientManager.Simulate ();
		}

        public static void Visualize() {
			PlayerManager.Visualize();

			BehaviourHelper.GlobalVisualize();
			PhysicsManager.Visualize();
			AgentController.Visualize();
            ProjectileManager.Visualize();
            EffectManager.Visualize();

			TeamManager.Visualize ();

			//LateVisualize ();
        }

		public static void LateVisualize () {
			InputManager.Visualize();
		}

        public static void Deactivate() {
            if (Started == false) return;
            Selector.Clear();
            AgentController.Deactivate();
			BehaviourHelper.GlobalDeactivate ();
            ProjectileManager.Deactivate();
			ClientManager.Deactivate ();

			TeamManager.Deactivate ();
            ClientManager.NetworkHelper.Disconnect ();
			Started = false;
        }

		public static void Quit () {
			ClientManager.Quit ();
		}

        private static void LoadSceneObjects() {
            LSSceneObject[] sceneObjects = GameObject.FindObjectsOfType<LSSceneObject>();
            for (int i = 0; i < sceneObjects.Length; i++) {
                sceneObjects[i].Initialize();
            }
        }

        public static int GetStateHash () {
            int hash = LSUtility.PeekRandom (int.MaxValue);
            hash += 1;
            hash ^= AgentController.GetStateHash ();
            hash += 1;
            hash ^= ProjectileManager.GetStateHash ();
            hash += 1;
            return hash;
        }
    }

    public enum SelectionSetting {
        PC_RTS,
        Mobile
    }
}