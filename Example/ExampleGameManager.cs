using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Lockstep.Data;
using TypeReferences;
using System;

namespace Lockstep.Example
{
    public class ExampleGameManager : GameManager
    {
        static Replay LastSave = new Replay();

        void OnGUI()
        {
            GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(2.5f, 2.5f, 1)); 



            if (GUILayout.Button("Restart"))
            {
                ReplayManager.Stop();
                LSUtility.LoadLevel(SceneManager.GetActiveScene().name);
            }

            if (GUILayout.Button("Playback"))
            {
                LastSave = ReplayManager.SerializeCurrent();
                LSUtility.LoadLevel(SceneManager.GetActiveScene().name);
                ReplayManager.Play(LastSave);

            }
			
        }


    }
}