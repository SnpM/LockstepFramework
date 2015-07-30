using System;

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
			LSUtility.SetBitFalse(ref arrayAllocation,leIndex);
		}
	}
}