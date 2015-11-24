using UnityEngine;
using System.Collections;

namespace Lockstep
{
	public interface FastEnumerable<T>
	{
		void Enumerate (FastList<T> output);
	}
}