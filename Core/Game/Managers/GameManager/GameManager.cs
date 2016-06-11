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


        public void ScanForHelpers()
        {
            //Currently deterministic but not guaranteed by Unity
            _helpers = this.gameObject.GetComponents<BehaviourHelper>();
        }

        public void GetBehaviourHelpers(FastList<BehaviourHelper> output)
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
        }
            

        protected virtual void FixedUpdate()
        {
			LockstepManager.Simulate();
        }

        private float timeToNextSimulate;

		protected virtual void Update()
        {
            timeToNextSimulate -= Time.smoothDeltaTime * Time.timeScale;
            if (timeToNextSimulate <= float.Epsilon)
            {
                timeToNextSimulate = LockstepManager.BaseDeltaTime;
            }
            LockstepManager.Visualize();
        }



		protected void LateUpdate()
        {
            LockstepManager.LateVisualize();
        }
            
        bool Quited = false;
        void OnDisable ()
        {
			Instance = null;
            if (Quited) return;
            LockstepManager.Deactivate();
        }

        void OnApplicationQuit()
        {
			Instance = null;
            Quited = true;
            LockstepManager.Quit();
        }

    }
}