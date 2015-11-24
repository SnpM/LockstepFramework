using UnityEngine;
using System.Collections;

namespace Lockstep
{
	public abstract class ActiveAbility : Ability
	{
		public abstract void Execute (Command com);
		public abstract InputCode ListenInput {get;}
	}
}