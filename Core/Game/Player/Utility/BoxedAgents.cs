/*using UnityEngine;
using System.Collections; using FastCollections;
using System;
namespace Lockstep
{
	public static class BoxedAgents
	{
		public static LSAgent[] innerArray = new LSAgent[SelectionManager.MaximumSelection];
		public static int BufferCount;
		public static int Count;
		public static uint _Version = 1;
		public static uint _BufferVersion = 1;

		public static void Add (LSAgent agent)
		{
			innerArray[Count++] = agent;
			agent.BoxVersion = _Version;
		}

		public static void FastClear ()
		{
			BufferCount = Count;
			Count = 0;
			_BufferVersion = _Version;
			_Version++;
		}
	}
}*/