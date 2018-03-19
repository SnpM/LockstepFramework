using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;

public class ExampleReplayHelper : BehaviourHelper
{
	protected override void OnLateSimulate ()
	{
		if (ReplayManager.IsPlayingBack)
		{
            if (!FrameManager.CanAdvanceFrame)
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
