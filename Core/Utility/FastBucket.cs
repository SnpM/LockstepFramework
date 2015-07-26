using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Lockstep
{
	public class FastBucket<T>
	{
		static int leIndex, i, j;
		static ulong shiftedIndex;
		public T[] innerArray;
		public ulong arrayAllocation;
		public const int Capacity = 64;
		public int Count;
		public int PeakCount;
		public FastStack<int> OpenSlots;

		public FastBucket ()
		{
			Initialize ();
		}

		private void Initialize ()
		{
			innerArray = new T[Capacity];
			OpenSlots = new FastStack<int> (Capacity);
			arrayAllocation = 0;
			Count = 0;
			PeakCount = 0;
		}

		public void Add (T item)
		{
			if (OpenSlots.Count > 0) {
				leIndex = OpenSlots.Pop ();
				shiftedIndex = (ulong)1 << leIndex;
				innerArray [leIndex] = item;
				arrayAllocation |= shiftedIndex;
				return;
			}
			if (PeakCount == Capacity) {
				return;
			}
			shiftedIndex = (ulong)1 << PeakCount;
			arrayAllocation |= shiftedIndex;
			innerArray [PeakCount++] = item;

			Count++;
		}

		public void Remove (T item)
		{
			leIndex = Array.IndexOf (innerArray, item);
			if (leIndex >= 0) {
				RemoveAt(leIndex);
			}
		}
		public void RemoveAt (int index)
		{
			shiftedIndex = (ulong)1 << index;
			OpenSlots.Add (index);
			arrayAllocation ^= shiftedIndex;
			Count--;
		}

		public T this [int index] {
			get {
				return innerArray [index];
			}
			set {
				innerArray [index] = value;
			}
		}
		public void Clear ()
		{
			for (i = 0; i < Capacity; i++) {
				innerArray [i] = default(T);
			}
			OpenSlots.FastClear ();
			arrayAllocation = 0;
			PeakCount = 0;
			Count = 0;
		}

		public void FastClear ()
		{
			arrayAllocation = 0;
			OpenSlots.FastClear ();
			PeakCount = 0;
			Count = 0;
		}

	}

}