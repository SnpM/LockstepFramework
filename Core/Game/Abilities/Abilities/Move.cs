using UnityEngine;
using System.Collections;
using Lockstep;
public class Move : ActiveAbility {
	public override void Initialize (LSAgent agent)
	{

	}

	public override void Simulate ()
	{

	}

	public override void Deactivate ()
	{

	}

	public override void Execute (Command com)
	{
		Debug.Log (Speed);
	}

	public long Speed;

	public override InputCode ListenInput {
		get {
			return InputCode.M;
		}
	} 

}
