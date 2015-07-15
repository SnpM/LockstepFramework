using UnityEngine;
using System.Collections;
using System;
namespace Lockstep
{
/*
Collection with optimized Add and Remove time - used for structures that need to update frequently.
*/
	public class FastSet<T>
	{
		private FastStack<int> OpenSlots;
		public T[] innerArray;
		public int Count;
		public int LastCount;
		private int Capacity;

		public void Add (T item)
		{
			if (OpenSlots.Count > 0)
			{
				innerArray[OpenSlots.Pop ()] = item;
				Count++;
				return;
			}
			if (LastCount == Capacity)
			{
				Capacity *= 2;
				Array.Resize (ref innerArray, Capacity);
			}
			innerArray[LastCount++] = item;
			Count++;
		}

		public void Remove (T item)
		{
			int index = System.Array.IndexOf (innerArray, item, 0, Count);
			if (index >= 0)
			{
				OpenSlots.Add (index);
				Count--;
				if (index == LastCount)
				{
					LastCount--;
				}
			}
		}
	}
}