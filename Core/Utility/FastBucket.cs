using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Lockstep
{
	public class FastBucket<T> where T : IBucketItem
	{
		static int leIndex, i, j;
		public T[] innerArray;
		public BitArray arrayAllocation;
		public int Capacity = 8;
		public int Count;
		public int PeakCount;
		public FastStack<int> OpenSlots;

		private void Initialize ()
		{
			innerArray = new T[Capacity];
			arrayAllocation = new BitArray(Capacity);
			Count = 0;
			PeakCount = 0;
		}

		public void Add (T item)
		{
			if (OpenSlots.Count > 0)
			{
				leIndex = OpenSlots.Pop ();
				innerArray[leIndex] = item;
				item.BucketIndex = leIndex;
				arrayAllocation[leIndex] = true;
				return;
			}
			if (PeakCount == Capacity)
			{
				Capacity *= 2;
				Array.Resize (ref innerArray, Capacity);
				arrayAllocation.Length = Capacity;
			}
			item.BucketIndex = PeakCount;
			arrayAllocation[PeakCount] = true;
			innerArray[PeakCount++] = item;

			Count++;
		}

		public void Remove (T item)
		{
			leIndex = item.BucketIndex;
			OpenSlots.Add (leIndex);
			arrayAllocation[leIndex] = false;
			Count--;
		}

		public void Clear ()
		{
			for (i = 0; i < Capacity; i++)
			{
				innerArray[i] = default(T);
			}
			OpenSlots.FastClear ();
			arrayAllocation.SetAll(false);
			PeakCount = 0;
			Count = 0;
		}

		public void FastClear ()
		{
			arrayAllocation.SetAll (false);
			OpenSlots.FastClear ();
			PeakCount = 0;
			Count = 0;
		}

		public IEnumerator<T> GetEnumerator() {
			return new BucketIterator<T>(this);
		}

	}

	class BucketIterator<T> : IEnumerator<T> where T : IBucketItem
	{
		static int i,j;
		private FastBucket<T> bucket;
		private T _current;
		public T Current { get { return _current; } }
		 System.Object IEnumerator.Current { get { return _current; } }

		private int curPosition;
		private int Count;
		public BucketIterator (FastBucket<T> Bucket)
		{
			bucket = Bucket;
			Count = bucket.Count;
		}
		public bool MoveNext()
		{
			for (i = 0; i < bucket.PeakCount; i++)
			{
				if (bucket.arrayAllocation[i])
				{
					curPosition = i + 1;
					_current = bucket.innerArray[i];
					Count--;
					return true;
				}
				else if (Count == 0)
				{
					return false;
				}
			}
			return false;
		}
		public void Reset ()
		{

		}
		public void Dispose ()
		{

		}
	}
}