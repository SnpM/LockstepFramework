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

        protected FastList<LSAgent> spawnedAgents = new FastList<LSAgent>();

        public override void GetBehaviourHelpers (FastList<BehaviourHelper> output) {
            base.GetBehaviourHelpers(output);
            var go = new GameObject("BehaviourHandlers");

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