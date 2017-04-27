using UnityEngine;
using System.Collections;
using System;

namespace FastCollections
{
	/// <summary>
	/// List that sorts its elements, adds from the optimal end, and has O(1) popping.
	/// </summary>
	public class FastSorter<T>
	{
		public delegate int SortCompare<CompareT> (CompareT source, CompareT other);
		private const int defaultCapacity = 4;

		public int Count { get; private set; }

		private int Capacity;
		private T[] innerArray;
		private int _offset;

		private int Offset {
			get {
				return _offset;
			}
			set {
				_offset = value;
			}
		}

		public SortCompare<T> Comparer;
		public FastSorter () {
			_Init (CompareWithIComparable, defaultCapacity);
		}
		public FastSorter (SortCompare<T> comparer)
		{
			_Init (comparer, defaultCapacity);
		}

		private void _Init (SortCompare<T> comparer, int startCapacity)
		{
			Comparer = comparer;
			Capacity = startCapacity;
			innerArray = new T[startCapacity];
			Offset = 0;
		}

		public void Add (T item)
		{
			if (Count > 0) {
				int min = 0;
				int max = Count;
				int split = GetSplit (min, max);

				while (min < max) {
					int compare = Comparer (item, innerArray [GetIndex (split)]);
					if (compare == 0) {
						min = split + 1;
						break;
					} else if (compare > 0) {
						min = split + 1;
					} else {
						max = split;
					}
					split = GetSplit (min, max);
				}
				Insert (item, min);
			} else {
				Insert (item, 0);
			}
			Count++;
		}

		public T PopMin ()
		{
			int index = GetIndex (0);
			T ret = innerArray [index];
			innerArray[index] = default(T);
			Offset++;
			Count--;
			if (Count == 0)
				Offset = 0;
			return ret;
		}

		public T PeekMin () {
			return innerArray[GetIndex(0)];
		}

		public T PopMax ()
		{
			int index = GetIndex (--Count);
			T ret = innerArray [index];
			innerArray[index] = default(T);
			if (Count == 0)
				Offset = 0;
			return ret;
		}

		public T PeekMax () {
			return innerArray[GetIndex (Count - 1)];
		}

		public void Clear (bool fast = true)
		{
			if (fast == false) {
				Array.Clear (innerArray, GetIndex(0), Count);
			}
			Count = 0;
			Offset = 0;

		}

		private int GetSplit (int min, int max)
		{
			return min + ((max - min) / 2);
		}

		private int GetIndex (int place)
		{
			return place + Offset;
		}

		private void Insert (T item, int place)
		{
			int index = GetIndex (place);
			bool capped = Count + Offset >= Capacity;
			bool forceLeftShift = false;
			if (capped) {
				if (forceLeftShift = Offset != 0) {

				} else {
					CheckCapacity (Count + Offset + 1);
				}
			}

			if (Count > 0) {
				int distanceToHead = Count - place;
				int distanceToTail = place + 1;

				if (forceLeftShift || Offset != 0 && (distanceToHead >= distanceToTail)) {
					Shift (innerArray, Offset--, index, -1);
				} else {
					Shift (innerArray, index, Count + Offset, 1);
				}
				if (Offset == 0 || (!capped && distanceToHead < distanceToTail)) {
					if (distanceToHead > 0) {

					}
				} else {

				}
			}

			innerArray [GetIndex (place)] = item;
		}

		private void CheckCapacity (int min)
		{
			if (Capacity < min) {
				Capacity *= 2;
				if (Capacity < min)
					Capacity = min;
				Array.Resize (ref innerArray, Capacity);
			}
		}


		private static void Shift (Array array, int min, int max, int shiftAmount) {
			if (shiftAmount == 0) return;
			Array.Copy (array, min, array, min + shiftAmount, max - min);
		}

		#region Default SortCompares
		public static SortCompare<T> CompareWithIComparable = 
		(obj, other) => {return ((IComparable)obj).CompareTo (other);};

		#endregion
	}
}