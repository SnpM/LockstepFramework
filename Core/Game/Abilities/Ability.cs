using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
	[Serializable]
/// <summary>
/// Template for abilities.
/// </summary>
	public abstract class Ability : MonoBehaviour
	{
		public abstract void Initialize (LSAgent agent);

		public abstract void Simulate ();
	
		public abstract void Deactivate ();


	}
}