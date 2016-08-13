using UnityEngine;
using System.Collections;
namespace Lockstep {
public class Stop : ActiveAbility {
		protected override void OnExecute (Command com)
		{
			Agent.StopCast ();
		}
	}
}