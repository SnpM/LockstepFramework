using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lockstep
{
	public static class CoroutineManager
	{
		const int MaxCoroutines = 8196;

		static Coroutine[] Coroutines = new Coroutine[MaxCoroutines];

		static int[] OpenSlots = new int[MaxCoroutines];
		static int SlotCount;

		static int HighCount;

		static int i,j, leIndex;
		static Coroutine coroutine;

		public static void Simulate ()
		{
			for (i = 0; i < HighCount; i++)
			{
				coroutine = Coroutines[i];
				if (coroutine.Active)
				{
					coroutine.Simulate ();
				}
			}
		}

		public static Coroutine StartCoroutine (IEnumerator<int> enumerator)
		{
			if (SlotCount > 0)
			{
				leIndex = OpenSlots[--SlotCount];
				coroutine = Coroutines[leIndex];
				coroutine.Initialize (enumerator);
				coroutine.Index = leIndex;
				leIndex++;
				if (leIndex > HighCount) HighCount = leIndex;

			}
			else {
				coroutine = new Coroutine ();
				coroutine.Initialize (enumerator);
				Coroutines[HighCount] = coroutine;
				coroutine.Index = HighCount++;
			}
			return coroutine;
		}


		public static void StopCoroutine (Coroutine _coroutine)
		{
			leIndex = _coroutine.Index;
			OpenSlots[SlotCount++] = leIndex;
			_coroutine.End ();

			/*
			//Lower HighCount as much as possible
			if (_coroutine.Index == HighCount - 1)
			{
				HighCount--;
				for (i = HighCount - 1; i >= 0; i--)
				{
					coroutine = Coroutines[i];
					if (coroutine.Active == false)
					{
						HighCount--;
					}
					else {
						break;
					}
				}
			}
			*/

		}
	}

	public class Coroutine
	{
		public IEnumerator<int> Enumerator;
		public int WaitFrames;
		public bool Active = true;
		public int Index;

		public void Initialize (IEnumerator<int> enumerator)
		{
			Enumerator = enumerator;
			WaitFrames = 0;
			Active = true;
		}
		public void Simulate ()
		{
			if (WaitFrames > 0)
			{
				WaitFrames--;
				return;
			}
			else {
				WaitFrames--;
			}
			if (Enumerator.MoveNext ())
			{
				WaitFrames = (int)Enumerator.Current;
			}
			else {
				CoroutineManager.StopCoroutine (this);
			}
		}
		public void End ()
		{
			Active = false;
			Enumerator.Dispose ();
		}
	}

}