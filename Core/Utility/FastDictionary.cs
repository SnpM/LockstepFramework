using UnityEngine;
using System.Collections;

/// <summary>
/// Experimenting with dictionary. Not yet functional.
/// </summary>
public class FastDictionary<TKey,TValue> {
	static int bigIndex, smallIndex, hashCode;
	static uint indexBitShift;
	static TValue[] curBucket;
	static uint curFlags;

	private const int _bucketCount = 32;
	private const int MaximumCapacity = _bucketCount * 32;
	public TValue[][] Buckets = new TValue[_bucketCount][];
	public uint[] BucketFlags = new uint[_bucketCount];

	public void Add (TKey key, TValue value)
	{
		hashCode = NormalizedHashCode(key.GetHashCode ());
		bigIndex = hashCode % _bucketCount;
		smallIndex = (hashCode * hashCode) % 32;
		indexBitShift = (uint)1 << smallIndex;

		curFlags = BucketFlags[bigIndex];
		curBucket = Buckets[bigIndex];
		if (curBucket == null)
		{
			curBucket = new TValue[32];
			Buckets[bigIndex] = curBucket;
		}

		if ((curFlags & indexBitShift) == indexBitShift)
		{
			//Resolve collision
		}
		else {
			curBucket[smallIndex] = value;
			curFlags |= indexBitShift;
		}

	}

	private static int NormalizedHashCode (int Hash)
	{
		if (Hash < 0) Hash = -Hash;
		return Hash;
	}
}
