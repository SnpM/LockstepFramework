using UnityEngine;
using System.Collections;
using System;
namespace Lockstep {
public class PartitionNode {
		
		const int capacity = 512;
		public int[] innerArray = new int[capacity];
		public int Count = 0;

		
		public bool Add(int item)
		{
			if (Count == capacity) return false;
			innerArray[Count] = item;
			Count++;
			return true;
		}
		public void Remove(int item) {

			int index = Array.IndexOf(innerArray, item, 0, Count);
			if (index >= 0) {
				RemoveAt(index);
			}
		}
		public void RemoveAt(int index) {
			Count--;
			Array.Copy(innerArray, index + 1, innerArray, index, Count - index);

			innerArray[Count] = -1;
		}
		public int this[int index]
		{
			get
			{
				return innerArray[index];
			}
			set
			{
				innerArray[index] = value;
			}
		}
		
		public override string ToString()
		{
			string output = string.Empty;
			for (int i = 0; i < Count - 1; i++)
				output += innerArray[i] + ", ";
			
			return output + innerArray[Count - 1];
		}
	}
}