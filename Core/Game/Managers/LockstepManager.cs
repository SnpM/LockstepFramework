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

namespace Lockstep
{
    //TODO: Set up default functions to implement LSManager
    public static class LockstepManager
    {
        public static readonly System.Diagnostics.Stopwatch SimulationTimer = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// Seconds since start if the last session.
        /// </summary>
        /// <value>The seconds.</value>
        public static double Seconds { get { return SimulationTimer.ElapsedTicks / (double)System.TimeSpan.TicksPerSecond; } }

        public static MonoBehaviour UnityInstance { get; private set; }

        public const int FrameRate = 32;
        public const int InfluenceResolution = 2;
        public const float BaseDeltaTime = (float)(1d / FrameRate);

        private static int InfluenceCount;

        public static int InfluenceFrameCount { get; private set; }

        public static int FrameCount { get; private set; }

        public static bool GameStarted { get; private set; }

        public static bool Loaded { get; private set; }

        private static GameManager _mainGameManager;

        public static GameManager MainGameManager
        {
            get
            {
                if (_mainGameManager == null)
                    throw new System.Exception("MainGameManager has exploded!");
                return _mainGameManager;
            }
            private set
            {
                _mainGameManager = value;
            }
        }

        internal static void Setup()
        {
            DefaultMessageRaiser.EarlySetup();

            LSDatabaseManager.Setup();

            UnityInstance = GameObject.CreatePrimitive(PrimitiveType.Quad).AddComponent<MonoBehaviour>();
            GameObject.Destroy(UnityInstance.GetComponent<Collider>());
            UnityInstance.GetComponent<Renderer>().enabled = false;
            GameObject.DontDestroyOnLoad(UnityInstance.gameObject);

            GridManager.Setup();
            AbilityInterfacer.Setup();
         
            AgentController.Setup();
            TeamManager.Setup();

            ProjectileManager.Setup();
            EffectManager.Setup();

            PhysicsManager.Setup();
            ClientManager.Setup();

            Application.targetFrameRate = 60;
            Time.fixedDeltaTime = BaseDeltaTime;
            Time.maximumDeltaTime = Time.fixedDeltaTime * 2;
            InputCodeManager.Setup();


            DefaultMessageRaiser.LateSetup();
        }

        internal static void Initialize(GameManager gameManager)
        {
            Application.targetFrameRate = 30;
            MainGameManager = gameManager;

            if (!Loaded)
            {
                Setup();
                Loaded = true;
            }

            DefaultMessageRaiser.EarlyInitialize();
            SimulationTimer.Stop();
            SimulationTimer.Reset();
            SimulationTimer.Start();
            LSDatabaseManager.Initialize();
            LSUtility.Initialize(1);
            InfluenceCount = 0;
            Time.timeScale = 1f;
            Stalled = true;

            FrameCount = 0;
            InfluenceFrameCount = 0;
            MainGameManager.MainInterfacingHelper.Initialize();

			ClientManager.Initialize(MainGameManager.MainNetworkHelper);

            TriggerManager.Initialize();

            GridManager.Initialize();

            TeamManager.Initialize();

            CoroutineManager.Initialize();
            FrameManager.Initialize();

            CommandManager.Initialize();

            AgentController.Initialize();
            TeamManager.LateInitialize();

            PhysicsManager.Initialize();
            PlayerManager.Initialize();
            SelectionManager.Initialize();
            InfluenceManager.Initialize();
            ProjectileManager.Initialize();

			InitializeHelpers();

            DefaultMessageRaiser.LateInitialize();
            BehaviourHelperManager.LateInitialize();
            MainGameManager.MainInterfacingHelper.LateInitialize();
        }

        static void InitializeHelpers()
        {
            FastList<BehaviourHelper> helpers = new FastList<BehaviourHelper>();
            MainGameManager.GetBehaviourHelpers(helpers);
            BehaviourHelperManager.Initialize(helpers.ToArray());
        }

        static bool Stalled;

        internal static void Simulate()
        {
            MainGameManager.MainNetworkHelper.Simulate();
            DefaultMessageRaiser.EarlySimulate();
            if (InfluenceCount == 0)
            {
                InfluenceSimulate();
                InfluenceCount = InfluenceResolution - 1;
                if (FrameManager.CanAdvanceFrame == false)
                {
                    Stalled = true;
                    return;
                }
                Stalled = false;
                if (InfluenceFrameCount == 0)
                {
                    GameStart ();
                }
                FrameManager.Simulate();
                InfluenceFrameCount++;
            } else
            {
                InfluenceCount--;
            }
            if (Stalled)
            {
                return;
            }

            MainGameManager.MainInterfacingHelper.Simulate();

            BehaviourHelperManager.Simulate();
            AgentController.Simulate();
            PhysicsManager.Simulate();
            CoroutineManager.Simulate();
            InfluenceManager.Simulate();
            ProjectileManager.Simulate();
            TeamManager.Simulate();

            TriggerManager.Simulate();

            LateSimulate();
            FrameCount++;

        }

        private static void GameStart()
        {
            GameManager.GameStart();
            BehaviourHelperManager.GameStart();
            GameStarted = true;

        }

        private static void LateSimulate()
        {
            BehaviourHelperManager.LateSimulate();
            AgentController.LateSimulate();
            PhysicsManager.LateSimulate();
            DefaultMessageRaiser.LateSimulate();
        }

        internal static void InfluenceSimulate()
        {
            PlayerManager.Simulate();
            CommandManager.Simulate();
            ClientManager.Simulate();
        }

        internal static void Execute(Command com)
        {
            if (!GameStarted) 
            {
                Debug.LogError("BOOM");
                return;
            }
            AgentController cont = AgentController.InstanceManagers [com.ControllerID];
            cont.Execute(com);


            DefaultMessageRaiser.Execute(com);

        }

        internal static void Visualize()
        {
            if (!GameStarted) return;
            DefaultMessageRaiser.EarlyVisualize();
            PlayerManager.Visualize();
            MainGameManager.MainInterfacingHelper.Visualize();
            BehaviourHelperManager.Visualize();
            PhysicsManager.Visualize();
            AgentController.Visualize();
            ProjectileManager.Visualize();
            EffectManager.Visualize();

            TeamManager.Visualize();
        }

        internal static void LateVisualize()
        {
            DefaultMessageRaiser.LateVisualize();

        }

        internal static void DrawGUI()
        {

        }

        internal static void Deactivate()
        {
            DefaultMessageRaiser.EarlyDeactivate();

            if (GameStarted == false)
                return;
            Selector.Clear();
            AgentController.Deactivate();
            MainGameManager.MainInterfacingHelper.Deactivate();
            BehaviourHelperManager.Deactivate();
            ProjectileManager.Deactivate();
            ClientManager.Deactivate();

            TeamManager.Deactivate();
            ClientManager.Quit();
			PhysicsManager.Deactivate();
            GameStarted = false;
			LSServer.Deactivate();
            DefaultMessageRaiser.LateDeactivate();
        }

        public static void Quit()
        {
            ClientManager.Quit();
        }

        public static int GetStateHash()
        {
            int hash = LSUtility.PeekRandom(int.MaxValue);
            hash += 1;
            hash ^= AgentController.GetStateHash();
            hash += 1;
            hash ^= ProjectileManager.GetStateHash();
            hash += 1;
            return hash;
        }
    }
}