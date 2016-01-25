using Lockstep;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Lockstep.NetworkHelpers;

namespace Lockstep
{
    public class GameManager : MonoBehaviour
    {

        BehaviourHelper[] _helpers;

        BehaviourHelper[] Helpers { get { return _helpers; } }


        public static GameManager Instance { get; private set; }

        /// <summary>
        /// If true, LS will run in headless mode and only frames will be processed. Disables all physics, abilities, etc..
        /// </summary>
        /// <value><c>true</c> if headless; otherwise, <c>false</c>.</value>
        public virtual bool Headless
        {
            get
            {
                return false;
            }
        }


        string replayLoadScene;
        static int hashFrame;
        static long prevHash;
        static long stateHash;
        static bool hashChecked;

        private NetworkHelper _mainNetworkHelper;

        public virtual NetworkHelper MainNetworkHelper
        {
            get
            {
                if (_mainNetworkHelper == null)
                {
                    _mainNetworkHelper = GetComponent<NetworkHelper>();
                    if (_mainNetworkHelper == null)
                    {
                        Debug.Log("NetworkHelper not found on this GameManager's GameObject. Defaulting to ExampleNetworkHelper...");
                        _mainNetworkHelper = base.gameObject.AddComponent<ExampleNetworkHelper>();
                    }
                }
                return _mainNetworkHelper;
            }
        }


        private static InterfacingHelper _defaultHelper;

        public InterfacingHelper MainInterfacingHelper
        {
            get
            {
                if (_defaultHelper.IsNull()) {
                    _defaultHelper = this.GetComponent<InterfacingHelper> ();
                    if (_defaultHelper == null) {
                        Debug.Log("InterfacingHelper not found. Defaulting to RTSInterfacingHelper.");
                        _defaultHelper = new RTSInterfacingHelper();
                    }
                }
                return _defaultHelper;
            }
        }

        public void ScanForHelpers()
        {
            //Currently deterministic but not guaranteed by Unity
            _helpers = this.gameObject.GetComponents<BehaviourHelper>();
        }

        public virtual void GetBehaviourHelpers(FastList<BehaviourHelper> output)
        {
            //if (Helpers == null)
            ScanForHelpers();
            if (Helpers != null)
            {
                for (int i = 0; i < Helpers.Length; i++)
                {
                    output.Add(Helpers [i]);
                }
            }
        }

        protected void Start()
        {
            Instance = this;
            LockstepManager.Initialize(this);
            this.Startup();
        }

        protected virtual void Startup()
        {

        }


        protected void FixedUpdate()
        {
            LockstepManager.Simulate();
            /*
            if (ReplayManager.IsPlayingBack) {
                if (hashChecked == false) {
                    if (LockstepManager.FrameCount == hashFrame) {
                        hashChecked = true;
                        long newHash = AgentController.GetStateHash ();
                        if (newHash != prevHash) {
                            Debug.Log ("Desynced!");
                        } else {
                            Debug.Log ("Synced!");
                        }
                    }
                }
            } else 

            {
                hashFrame = LockstepManager.FrameCount - 1;
                prevHash = stateHash;
                stateHash = AgentController.GetStateHash ();
                hashChecked = false;
            }
        */
        }

        private float timeToNextSimulate;

        protected void Update()
        {
            timeToNextSimulate -= Time.smoothDeltaTime * Time.timeScale;
            if (timeToNextSimulate <= float.Epsilon)
            {
                timeToNextSimulate = LockstepManager.BaseDeltaTime;
            }
            LockstepManager.Visualize();
            CheckInput();
        }

        protected virtual void CheckInput()
        {
        
        }

        void LateUpdate()
        {
            LockstepManager.LateVisualize();
        }

        public static void GameStart()
        {
            Instance.OnGameStart();
        }

        protected virtual void OnGameStart()
        {
            //When the game starts (first simulation frame)
        }
        bool Quited = false;
        void OnDisable ()
        {
            if (Quited) return;
            LockstepManager.Deactivate();
        }

        void OnApplicationQuit()
        {
            Quited = true;
            LockstepManager.Quit();
        }

    }
}