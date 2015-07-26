using System.Collections;
using System;

namespace Lockstep
{
	public class FastList<T>
	{
		private const int DefaultCapacity = 8;
		public T[] innerArray;
		public int Count = 0;
		public int Capacity = DefaultCapacity;
		 
		public FastList (FastList<T> CopyList)
		{
			innerArray = (T[])CopyList.innerArray.Clone ();
			Count = innerArray.Length;
			Capacity = innerArray.Length;
		}

		public FastList (T[] StartArray)
		{
			innerArray = StartArray;
			Count = innerArray.Length;
			Capacity = innerArray.Length;
		}

		public FastList (int StartCapacity)
		{
			Capacity = StartCapacity;
			Initialize ();
		}
		public FastList ()
		{
			Initialize ();
		}

		private void Initialize ()
		{
			innerArray = new T[Capacity];
			Count = 0;
		}

		public void Add (T item)
		{
			EnsureCapacity (Count + 1);
			innerArray [Count++] = item;

		}

		public void AddRange (FastList<T> items)
		{
			ArrayLength = items.Count;
			EnsureCapacity (Count + ArrayLength + 1);
			for (i = 0; i < ArrayLength; i++)
			{
				innerArray[Count++] = items[i];
			}
		}

		public void AddRange (T[] items)
		{
			ArrayLength = items.Length;
			EnsureCapacity (Count + ArrayLength + 1);
			for (i = 0; i < ArrayLength; i++)
			{
				innerArray[Count++] = items[i];
			}
		}
		public void AddRange (T[] items, int startIndex, int count)
		{
			EnsureCapacity (Count + count + 1);
			for (i = 0; i < count; i++)
			{
				innerArray[Count++] = items[i + startIndex];
			}
		}

		public void Remove (T item)
		{
			
			int index = Array.IndexOf (innerArray, item, 0, Count);
			if (index >= 0) {
				RemoveAt (index);
			}
		}

		public void RemoveAt (int index)
		{
			Count--;
			innerArray [index] = default(T);
			Array.Copy (innerArray, index + 1, innerArray, index, Count - index);
			
		}

		public T[] ToArray ()
		{
			T[] retArray = new T[Count];
			Array.Copy (innerArray,0,retArray,0,Count);
			return retArray;
		}

		public bool Contains (T item)
		{
			return Array.IndexOf (innerArray,item,0,Count) != -1;
		}

		static int i;
		static int HighCount;
		static int SwapCount;
		static int ReverseCount;
		static T swapItem;
		static int ArrayLength;

		public void Reverse ()
		{
			//Array.Reverse (innerArray,0,Count);
			HighCount = Count / 2;
			ReverseCount = Count - 1;
			for (i = 0; i < HighCount; i++)
			{
				swapItem = innerArray[i];
				innerArray[i] = innerArray[ReverseCount];
				innerArray[ReverseCount] = swapItem;

				ReverseCount--;
			}
		}

		private void EnsureCapacity (int min)
		{
			if (Capacity < min)
			{
				Capacity *= 2;
				if (Capacity < min) {
					Capacity = min;
				}
				Array.Resize (ref innerArray, Capacity);
			}
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
			for (int i = 0; i < Capacity; i++)
			{
				innerArray[i] = default(T);
			}
			Count = 0;
		}
		
		/// <summary>
		/// Marks elements for overwriting. Note: this list will still keep references to objects.
		/// </summary>
		public void FastClear ()
		{
			Count = 0;
		}

		public void CopyTo (FastList<T> target)
		{
			Array.Copy (innerArray,0,target.innerArray,0,Count);
			target.Count = Count;
			target.Capacity = Capacity;
		}
		
		public override string ToString ()
		{
			string output = string.Empty;
			for (int i = 0; i < Count - 1; i++)
				output += innerArray [i] + ", ";
			
			return output + innerArray [Count - 1];
		}

	}

}