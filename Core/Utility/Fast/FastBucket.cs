using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Lockstep
{
	public class FastBucket<T> : FastEnumerable<T>
	{
		private T[] innerArray;
		public BitArray arrayAllocation {get; private set;}
		private int capacity = 8;
		public int Count {get; private set;}
		public int PeakCount {get; private set;}
		private FastStack<int> OpenSlots = new FastStack<int>();

		public FastBucket ()
		{
			Initialize ();
		}

		private void Initialize ()
		{
			innerArray = new T[capacity];
			arrayAllocation = new BitArray (capacity);
			Count = 0;
			PeakCount = 0;
		}


		public int Add (T item)
		{
			int index = OpenSlots.Count > 0 ? OpenSlots.Pop () : PeakCount++;
			this.AddAt (item, index);
			return index;
		}
		public void AddAt (T item, int index) {
			if (PeakCount >= capacity) {
				capacity *= 2;
				if (capacity < PeakCount + 1) capacity = PeakCount + 1;
				Array.Resize (ref innerArray, capacity);
				arrayAllocation.Length = capacity;
			}
			arrayAllocation.Set (index, true);
            innerArray [index] = item;
            
            Count++;
            PeakCount++;
		}

		public bool Remove (T item)
		{
			int index = Array.IndexOf (innerArray, item);
			if (index >= 0 && arrayAllocation[index]) {
				RemoveAt(index);
				return true;
			}
			return false;
		}

		public void RemoveAt (int index)
		{
			OpenSlots.Add (index);
			arrayAllocation.Set (index, false);
            this.innerArray[index] = default(T);
			Count--;
            PeakCount--;
        }

        public bool ContainsAt (int index, T item) {
            return index >= 0 && index < PeakCount && this.arrayAllocation[index] && innerArray[index].Equals( item);
        }

		public T this [int index] {
			get {
#if DEBUG
				if (arrayAllocation[index] == false) throw new System.IndexOutOfRangeException ();
#endif
				return innerArray [index];
			}
			set {
				#if DEBUG
				if (arrayAllocation[index] == false) throw new System.IndexOutOfRangeException ();
				#endif
				innerArray [index] = value;
			}
		}
		public void Clear ()
		{
			for (int i = 0; i < capacity; i++) {
				innerArray [i] = default(T);
			}
			FastClear ();
		}

		public void FastClear ()
		{
			arrayAllocation.SetAll (false);
			OpenSlots.FastClear ();
			PeakCount = 0;
			Count = 0;
		}

		public void Enumerate (FastList<T> output) {
			output.FastClear();
			for (int i = 0; i < PeakCount; i++) {
				if (arrayAllocation[i]){
					output.Add (innerArray[i]);
				}
			}
		}
	}

}