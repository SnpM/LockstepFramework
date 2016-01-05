using UnityEngine;
using System.Collections;
using Lockstep;
public class SetHeight : Ability {
	[SerializeField]
	private float _offset;
	float Offset {get {return _offset;}}

	private bool changed;

	protected override void OnInitialize ()
	{
		changed = false;
		Agent.Body.visualPosition.y = Offset;
	}
	protected override void OnVisualize ()
	{
		if (Agent.Body.SetPositionBuffer) {
			//TODO: Terrain mapping
		}

		if (changed) {
			Agent.Body.visualPosition.y = Offset;
		}
	}
}
