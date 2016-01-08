using UnityEngine;
using System.Collections;
using Lockstep;

public class ExampleReplayHelper : BehaviourHelper
{
	protected override void OnLateSimulate ()
	{
		if (ReplayManager.IsPlayingBack)
		{
			if (LockstepManager.FrameCount == ReplayManager.CurrentReplay.FrameCount-1)
			{
				long newHash = LockstepManager.GetStateHash ();
				if (newHash != ReplayManager.CurrentReplay.hash)
				{
					Debug.Log ("Desynced!");
				} else
				{
					Debug.Log ("Synced!");
				}
			}
		}
	}
}
