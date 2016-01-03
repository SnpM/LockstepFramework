using UnityEngine;
using System.Collections;
using Lockstep.Data;
using TypeReferences;
using System;
namespace Lockstep.Example {
    public class ExampleGameManager : GameManager {

        [SerializeField,DataCode ("Agents")]
        private string _spawnCode;
        [SerializeField]
        private int _spawnAmount;

        private NetworkHelper _mainNetworkHelper;

        protected FastList<LSAgent> spawnedAgents = new FastList<LSAgent>();

        public override void GetBehaviourHelpers (FastList<BehaviourHelper> output) {
            base.GetBehaviourHelpers(output);
            var go = new GameObject("BehaviourHandlers");

        }

        public override NetworkHelper MainNetworkHelper {
            get {
                if (_mainNetworkHelper == null) {
                    _mainNetworkHelper = GetComponent<NetworkHelper> ();
                    if (_mainNetworkHelper == null) {
                        throw new System.NotImplementedException ("A NetworkHelper needs to be attached to the same object as the GameManager.");
                    }
                }
                return _mainNetworkHelper;
            }
        }


        protected override void OnGameStart () {
            AgentController ac = AgentController.Create();
            PlayerManager.AddController (ac);

            for (int i = 0; i < _spawnAmount; i++) {
                spawnedAgents.Add (ac.CreateAgent (_spawnCode,Vector2d.zero));
            }
        }

    }
}