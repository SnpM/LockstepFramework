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
namespace Lockstep {
    //TODO: Set up default functions to implement LSManager
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
        public static bool Loaded {get; private set;}

        public static GameManager MainGameManager {get; private set;}

        internal static void Setup () {

            DefaultMessageRaiser.EarlySetup();

            LSDatabaseManager.Setup ();

			UnityInstance = GameObject.CreatePrimitive (PrimitiveType.Sphere).AddComponent<MonoBehaviour> ();
            UnityInstance.GetComponent<Renderer>().enabled = false;
			GameObject.DontDestroyOnLoad (UnityInstance.gameObject);

			AbilityInterfacer.Setup ();
         
            AgentController.Setup();
			TeamManager.Setup ();

            ProjectileManager.Setup();
            EffectManager.Setup();

			PhysicsManager.Setup ();
			ClientManager.Setup (MainGameManager.MainNetworkHelper);
            InterfaceManager.Setup();

			Application.targetFrameRate = 30;
			Time.fixedDeltaTime = BaseDeltaTime;
			Time.maximumDeltaTime = Time.fixedDeltaTime * 2;

			InputManager.Setup ();

            DefaultMessageRaiser.LateSetup();
        }

        internal static void Initialize(GameManager gameManager) {
            MainGameManager = gameManager;

            if (!Loaded) {
                Setup ();
                Loaded = true;
            }

            DefaultMessageRaiser.EarlyInitialize();

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

            TriggerManager.Initialize();

            GridManager.Generate();
            GridManager.Initialize();

			TeamManager.Initialize ();

            CoroutineManager.Initialize();
			FrameManager.Initialize();

            CommandManager.Initialize();

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

            InitializeHelpers ();

            DefaultMessageRaiser.LateInitialize();
        }

        static void InitializeHelpers () {
            FastList<BehaviourHelper> helpers = new FastList<BehaviourHelper>();
            MainGameManager.GetBehaviourHelpers (helpers);
            BehaviourHelperManager.Initialize(helpers.ToArray ());
        }

		static bool Stalled;
        internal static void Simulate() {
            DefaultMessageRaiser.EarlySimulate ();
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
			BehaviourHelperManager.Simulate();
			AgentController.Simulate();
            PhysicsManager.Simulate();
            CoroutineManager.Simulate();
            InfluenceManager.Simulate();
            ProjectileManager.Simulate();
			TeamManager.Simulate ();

            TriggerManager.Simulate();

			LateSimulate ();
            FrameCount++;

        }
		private static void StartGame () {
			GameManager.StartGame ();
		}
		private static void LateSimulate () {
            BehaviourHelperManager.LateSimulate ();
			AgentController.LateSimulate ();
			PhysicsManager.LateSimulate ();
            DefaultMessageRaiser.LateSimulate ();
		}
        internal static void InfluenceSimulate () {
			PlayerManager.Simulate();
			CommandManager.Simulate();
			ClientManager.Simulate ();
		}

        internal static void Execute (Command com) {
            
            switch (com.LeInput)
            {
                case InputCode.None:
                    break;
                case InputCode.Meta:
                    MetaActionCode actionCode = (MetaActionCode)com.Target;
                    int id = com.Count;
                    switch (actionCode)
                    {
                        case MetaActionCode.NewPlayer:
                            AgentController controller = new AgentController();
                            if (id == ClientManager.ID)
                            {
                                PlayerManager.AddController(controller);
                            }
                            TeamManager.JoinTeam(controller);
                            
                            break;
                    }
                    break;
                default:
                    AgentController cont = AgentController.InstanceManagers [com.ControllerID];
                    cont.Execute(com);
                    break;
            }

            DefaultMessageRaiser.Execute (com);

        }

        internal static void Visualize() {
            DefaultMessageRaiser.EarlyVisualize();
			PlayerManager.Visualize();

			BehaviourHelperManager.Visualize();
			PhysicsManager.Visualize();
			AgentController.Visualize();
            ProjectileManager.Visualize();
            EffectManager.Visualize();

			TeamManager.Visualize ();
        }

        internal static void LateVisualize () {
			InputManager.Visualize();
            DefaultMessageRaiser.LateVisualize();

		}

        internal static void Deactivate() {
            DefaultMessageRaiser.EarlyDeactivate();

            if (Started == false) return;
            Selector.Clear();
            AgentController.Deactivate();
			BehaviourHelperManager.Deactivate ();
            ProjectileManager.Deactivate();
			ClientManager.Deactivate ();
            LockstepManager.Deactivate();

			TeamManager.Deactivate ();
            ClientManager.NetworkHelper.Disconnect ();
			Started = false;

            DefaultMessageRaiser.LateDeactivate();
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