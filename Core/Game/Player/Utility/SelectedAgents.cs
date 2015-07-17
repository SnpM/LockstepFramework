using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
	public static class SelectedAgents
	{
		public static bool[] arrayAllocated = new bool[SelectionManager.MaximumSelection];
		public static LSAgent[] innerArray = new LSAgent[SelectionManager.MaximumSelection];
		public static int PeakCount = 0;
		public static int Count;

		public static FastStack<int> OpenSlots = new FastStack<int> ();
		static int leIndex;
	
		public static void Add (LSAgent agent)
		{
			if (OpenSlots.Count > 0) {
				leIndex = OpenSlots.Pop ();
			} else {
				leIndex = PeakCount++;
			}
			agent.SelectedAgentsIndex = leIndex;
			innerArray [leIndex] = agent;
			arrayAllocated [leIndex] = true;
			Count++;
		}
	
		public static void Remove (LSAgent agent)
		{
			leIndex = agent.SelectedAgentsIndex;
			agent.SelectedAgentsIndex = -1;
			OpenSlots.Add (leIndex);
			arrayAllocated [leIndex] = false;
			Count--;
		}
	
		public static void FastClear ()
		{
			OpenSlots.FastClear ();
			Array.Clear (arrayAllocated, 0, arrayAllocated.Length);
			PeakCount = 0;
			Count = 0;
		}
	}
}