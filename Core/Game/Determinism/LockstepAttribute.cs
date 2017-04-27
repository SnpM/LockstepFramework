using UnityEngine;
using System.Collections; using FastCollections;
using System;

namespace Lockstep
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class LockstepAttribute : Attribute
	{
		public bool DoReset { get; private set; }
		public LockstepAttribute()
		{
			this.DoReset = false;
		}

		public LockstepAttribute(bool doReset)
		{
			DoReset = doReset;
		}
	}
}