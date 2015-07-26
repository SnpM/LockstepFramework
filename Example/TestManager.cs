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
		const int mid = 32;


		LSBody wall = Instantiate(TestWall).GetComponent<LSBody>();
		wall.Initialize(new Vector2d(-32 + 14,0));
		for (long i = wall.XMin; i <= wall.XMax ; i+= FixedMath.One)
		{
			for (long j = wall.YMin; j <= wall.YMax ; j+= FixedMath.One)
			{
				GridManager.GetNode(i,j).Unwalkable = true;
			}
		}
		GridManager.Initialize ();
		AgentController controller = AgentController.Create ();
		LSAgent agent = null;
		for (int i = 0; i < 256; i++)
		{
			agent = controller.CreateAgent (AgentCode.Minion);
		}
		PlayerManager.AddAgentController (controller);

	}

	void FixedUpdate ()
	{
		LockstepManager.Simulate ();
	}
	
	void Update ()
	{
		LockstepManager.Visualize ();
	}
}