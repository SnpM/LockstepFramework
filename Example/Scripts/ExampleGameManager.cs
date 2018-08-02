using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lockstep.Example
{
	public class ExampleGameManager : GameManager
	{
		static Replay LastSave = new Replay();

		//        public static void LoadLevel (string levelName) {
		//            LockstepManager.Deactivate ();
		//            SceneManager.LoadScene(levelName);
		//        }
		//		public static void LoadLevel(int levelName)
		//		{
		//			LockstepManager.Deactivate();
		//            SceneManager.LoadScene(levelName);
		//		}

		void OnGUI()
		{
			GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(2.5f, 2.5f, 1));



			if (GUILayout.Button("Restart"))
			{
				ReplayManager.Stop();
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}

			if (GUILayout.Button("Playback"))
			{
				LastSave = ReplayManager.SerializeCurrent();
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
				ReplayManager.Play(LastSave);

			}

		}


	}
}