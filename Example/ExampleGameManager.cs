using UnityEngine;
using UnityEngine.SceneManagement;
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


			if (ReplayManager.IsPlayingBack) {
				if (ReplayManager.CurrentReplay != null)
				{
					if (GUILayout.Button("Stop"))
					{
						ReplayManager.CurrentReplay = null;
						ReplayManager.Stop();
						LSUtility.LoadLevel(SceneManager.GetActiveScene().name);
					}

					if (GUILayout.Button("Rewind"))
					{
						ReplayManager.Play(LastSave);
						LSUtility.LoadLevel(SceneManager.GetActiveScene().name);
					}
				}

			} else {
				if (GUILayout.Button ("Restart")) {
					LSUtility.LoadLevel (SceneManager.GetActiveScene().name);
				}

				if (GUILayout.Button ("Save")) {
                    LastSave = ReplayManager.SerializeCurrent();
					LSUtility.LoadLevel (SceneManager.GetActiveScene().name);
                    ReplayManager.Play (LastSave);

				}
			}
		}


    }
}