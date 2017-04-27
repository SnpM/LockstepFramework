using UnityEngine;
using System.Collections;

namespace FastCollections
{
	public interface FastEnumerable<T>
	{
		void Enumerate (FastList<T> output);
	}
}