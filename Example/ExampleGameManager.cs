using UnityEngine;
using System.Collections;
using Lockstep.Data;
using TypeReferences;
using System;
namespace Lockstep.Example {
    public class ExampleGameManager : GameManager {
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