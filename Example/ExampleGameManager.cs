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

        Replay LastSave = new Replay();
		void OnGUI ()
		{
			GUI.matrix = Matrix4x4.TRS (new Vector3(0, 0, 0), Quaternion.identity, new Vector3 (2.5f, 2.5f, 1)); 

			if (ReplayManager.CurrentReplay != null)
			{
				if (GUILayout.Button ("Stop")) {
					ReplayManager.CurrentReplay = null;
					ReplayManager.Stop ();
					Application.LoadLevel (Application.loadedLevel);
				}

				if (GUILayout.Button ("Rewind")) {
					ReplayManager.Play (LastSave);
					Application.LoadLevel (Application.loadedLevel);
				}
			}

			if (ReplayManager.IsPlayingBack) {
			} else {
				if (GUILayout.Button ("Restart")) {
					Application.LoadLevel (Application.loadedLevel);
				}

				if (GUILayout.Button ("Save")) {
                    LastSave = ReplayManager.SerializeCurrent();
					Application.LoadLevel (Application.loadedLevel);
                    ReplayManager.Play (LastSave);

				}
			}
		}


    }
}