using System;
using System.Collections;
using System.Collections.Generic;
namespace Lockstep
{
	public class InfluencerBucket
	{
		#region Constants
		const int MaxCapacity = 64;
		const int StartCapacity = 8;
		#endregion;

		#region Static helpers
		static int leIndex;
		static int i;
		#endregion

		public int PeakCount;
		public int Capacity = StartCapacity;
		public int PeakCapacity;
		public ulong arrayAllocation;
		public FastStack<int> OpenSlots = new FastStack<int>(4);

		public LSInfluencer[] innerArray = new LSInfluencer[StartCapacity];

		public bool Add (LSInfluencer item)
		{
			leIndex = OpenSlots.Count > 0 ? OpenSlots.Pop () : PeakCount++;
			if (leIndex >= MaxCapacity)
			{
				PeakCount = MaxCapacity;
				return false;
			}
			if (PeakCount == Capacity)
			{
				Capacity *= 2;
				if (Capacity > MaxCapacity) Capacity = MaxCapacity;
				Array.Resize (ref innerArray, Capacity);
			}
			LSUtility.SetBitTrue(ref arrayAllocation,leIndex);
			item.bucketIndex = leIndex;
			innerArray[leIndex] = item;
			return true;
		}

		public void Remove (LSInfluencer item)
		{
			leIndex = item.bucketIndex;
			OpenSlots.Add (leIndex);

			LSUtility.SetBitFalse (ref arrayAllocation, leIndex);
			if (leIndex == PeakCount - 1)
			{
				PeakCount--;
				for (i = leIndex - 1; i >= 0; i--)
				{
					if (LSUtility.GetBitTrue (arrayAllocation,i))
					{
						PeakCount--;
					}
				}
			}
			else {
				LSUtility.SetBitFalse(ref arrayAllocation,leIndex);
			}
		}

		public IEnumerator GetEnumerator ()
		{
			return new InfluenceBucketEnumerator(this);
		}
	}
	public class InfluenceBucketEnumerator : IEnumerator {
		#region Foreach Implementation
		public InfluenceBucketEnumerator(InfluencerBucket bucket)
		{
			_bucket = bucket;
		}
		InfluencerBucket _bucket;
		int position = -1;
		
		//IEnumerator
		public bool MoveNext()
		{
			position++;
			while (position <= _bucket.PeakCount)
			{
				if (LSUtility.GetBitTrue (_bucket.arrayAllocation, position))
				{
					break;
				}
				position++;
			}
			return (position < _bucket.PeakCount);
		}
		
		//IEnumerable
		public void Reset()
		{position = -1;}
		
		
		//IEnumerable
		public object Current
		{
			get { return _bucket.innerArray[position];}
		}
		
		#endregion
	}
}