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

        private NetworkHelper _mainNetworkHelper = new ExampleNetworkHelper ();

        protected FastList<LSAgent> spawnedAgents = new FastList<LSAgent>();

        public override void GetBehaviourHelpers (FastList<BehaviourHelper> output) {
            base.GetBehaviourHelpers(output);
            var go = new GameObject("BehaviourHandlers");

        }

        public override NetworkHelper MainNetworkHelper {
            get {
                return _mainNetworkHelper;
            }
        }


        protected override void OnStartGame () {
            for (int i = 0; i < _spawnAmount; i++) {
                AgentController ac = new AgentController();
                PlayerManager.AddController (ac);
                spawnedAgents.Add (ac.CreateAgent (_spawnCode,Vector2d.zero));
            }
        }

    }
}