using UnityEngine;
using System.Collections;
using Lockstep;
public class Print : ActiveAbility {
	public override void Initialize (LSAgent agent)
	{

	}

	public override void Simulate ()
	{
		Debug.Log (LockstepManager.FrameCount);
	}

	public override void Deactivate ()
	{

	}

	public override void Execute (Command com)
	{

	}

	public long ASDF;

	public override InputCode ListenInput {
		get {
			return InputCode.Q;
		}
	} 

	public override AbilCode Code {
		get {
			return AbilCode.Print;
		}
	}
}
