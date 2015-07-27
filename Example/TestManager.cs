//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================
using UnityEngine;
using System.Collections;
using Lockstep;

public class TestManager : MonoBehaviour
{
	public GameObject TestWall;

	void Start ()
	{

		LockstepManager.Initialize ();
		GridManager.Generate ();
		const int count = 32;

		/*
		for (int i = -count; i < count; i++)
		{
			for (int j = -count; j < count; j++)
			{
				if (i * i + j * j < 16) continue;
				if (LSUtility.GetRandom (2) == 0)
				{
					Vector2d pos = new Vector2d(i,j);
					GridManager.GetNode(pos.x,pos.y).Unwalkable = true;
					Instantiate(TestWall).GetComponent<LSBody>().Initialize(pos);
				}
			}
		}*/

		/*LSBody wall = Instantiate (TestWall).GetComponent<LSBody> ();
		wall.Initialize (new Vector2d (-32 + 14, 0));
		for (long i = wall.XMin; i <= wall.XMax; i+= FixedMath.One) {
			for (long j = wall.YMin; j <= wall.YMax; j+= FixedMath.One) {
				GridManager.GetNode (i, j).Unwalkable = true;
			}
		}*/

		GridManager.Initialize ();
		controller = AgentController.Create ();
		for (int i = 0; i < 1; i++) {
			agent = controller.CreateAgent (AgentCode.Minion);
		}
		PlayerManager.AddAgentController (controller);
	}
	AgentController controller;
	LSAgent agent;

	void FixedUpdate ()
	{
		LockstepManager.Simulate ();
	}
	
	void Update ()
	{
		LockstepManager.Visualize ();
		if (Input.GetKeyDown (KeyCode.Space))
		{
			LSAgent temp = controller.CreateAgent (AgentCode.Minion);
			temp.Body.Parent = agent.Body;
		}
	}

	void OnGUI ()
	{
		if (ReplayManager.IsPlayingBack) {
			if (GUILayout.Button ("Play")) {
				NetworkManager.sendState = SendState.Autosend;
				ReplayManager.Stop ();
				Application.LoadLevel ("TestScene");
			}
		} else {
			if (GUILayout.Button ("Replay")) {
				ReplayManager.Save ("Test");
				ReplayManager.Play ("Test");
				Application.LoadLevel ("TestScene");
			}
		}
	}
}