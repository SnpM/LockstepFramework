using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
/// <summary>
/// Template for abilities.
/// </summary>
	public abstract class Ability
	{
		public abstract void Initialize (LSAgent agent);

		public abstract void Simulate ();
	
		public abstract AbilCode Code {get;}

		public abstract void Deactivate ();
	}
}