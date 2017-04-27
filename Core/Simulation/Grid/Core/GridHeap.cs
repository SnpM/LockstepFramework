//Thanks to Sebastian Lague's tutorial: https://www.youtube.com/watch?v=3Dw5d7PlcTM

using UnityEngine;
using System.Collections; using FastCollections;
using System;

namespace Lockstep
{
	public static class GridHeap
	{
		public static GridNode[] items = new GridNode[GridManager.DefaultCapacity];
        static int capacity = GridManager.DefaultCapacity;
		public static uint Count;
		public static uint _Version = 1;
	
		public static void Add (GridNode item)
		{
			item.HeapIndex = Count;
			items [Count++] = item;
			SortUp (item);
			item.HeapVersion = _Version;
		}
        static void EnsureCapacity (int min) {
            if (capacity < min) {
                capacity *= 2;
                if (capacity < min)
                    capacity = min;
                Array.Resize<GridNode> (ref items,capacity);
            }
        }
		static GridNode curNode;
        static GridNode newNode;
		public static GridNode RemoveFirst ()
		{
			curNode = items [0];
            newNode = items[--Count];
            items [0] = newNode;;
			newNode.HeapIndex = 0;
			SortDown (newNode);

			curNode.HeapVersion--;
			return curNode;
		}
	
		public static void UpdateItem (GridNode item)
		{
            //SortUp (item);
		}
	
		public static bool Contains (GridNode item)
		{
			return item.HeapVersion == _Version;
		}

		static uint i;
		public static void FastClear ()
		{
			_Version++;
			Count = 0;
		}
	
		static uint childIndexLeft;
		static uint childIndexRight;
		static uint swapIndex;
		static GridNode swapNode;

		static void SortDown (GridNode item)
		{
			while (true) {
				childIndexLeft = item.HeapIndex * 2 + 1;
				childIndexRight = item.HeapIndex * 2 + 2;
				swapIndex = 0;
			
				if (childIndexLeft < Count) {
					swapIndex = childIndexLeft;
				
					if (childIndexRight < Count) {
						if (items [childIndexLeft].fCost > (items [childIndexRight]).fCost) {
							swapIndex = childIndexRight;
						}
					}
				
					swapNode = items[swapIndex];
					if(item.fCost > swapNode.fCost) {
						Swap (item, swapNode);
					} else {
						return;
					}
				
				} else {
					return;
				}
			}
		}
	
		static uint parentIndex;
		static void SortUp (GridNode item)
		{
            if (item.HeapIndex == 0) return;
			parentIndex = (item.HeapIndex-1) / 2;
		
			while (true) {
				curNode = items [parentIndex];
                if(item.fCost < curNode.fCost) {
					Swap (item, curNode);
				} else {
					return;
				}
                if (parentIndex == 0)
                    return;
                parentIndex = (item.HeapIndex - 1) / 2;
			}
		}
	
		static uint itemAIndex;
		static void Swap (GridNode itemA, GridNode itemB)
		{
			itemAIndex = itemA.HeapIndex;

			items [itemAIndex] = itemB;
			items [itemB.HeapIndex] = itemA;

			itemA.HeapIndex = itemB.HeapIndex;
			itemB.HeapIndex = itemAIndex;
		}
	}

}