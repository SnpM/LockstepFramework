using UnityEngine;
using System;
using System.Collections; using FastCollections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lockstep
{
	public static class CoroutineManager
	{

		static FastBucket<Coroutine> Coroutines = new FastBucket<Coroutine>();



		public static void Initialize()
		{
			Coroutines.Clear();
		}

		public static void Simulate()
		{
			for (int i = 0; i < Coroutines.PeakCount; i++)
			{
				if (Coroutines.arrayAllocation[i])
				{
					Coroutine coroutine = Coroutines[i];
					if (coroutine.Active)
					{
						coroutine.Simulate();
					}
				}
			}
		}

		public static Coroutine StartCoroutine(IEnumerator<int> enumerator)
		{
			Coroutine coroutine = new Coroutine();
			coroutine.Initialize(enumerator);
			coroutine.Index = Coroutines.Add(coroutine);
			return coroutine;
		}

		public static void StopCoroutine(Coroutine _coroutine)
		{
			if (_coroutine.Active == false)
			{
				Debug.LogError("Coroutine already stopped");
			}
			Coroutines.RemoveAt(_coroutine.Index);
			_coroutine.Active = false;
			_coroutine.End();
		}

		public static void Deactivate()
		{
			Coroutines.Clear();
		}
	}

	public class Coroutine
	{
		public IEnumerator<int> Enumerator;
		public int WaitFrames;
		public bool Active = true;
		public int Index;

		public void Initialize(IEnumerator<int> enumerator)
		{
			Enumerator = enumerator;
			WaitFrames = 0;
			Active = true;
		}
		public void Simulate()
		{
			WaitFrames--;
			if (WaitFrames > 0)
			{
				return;
			}
			if (Enumerator.MoveNext())
			{
				WaitFrames = (int)Enumerator.Current;
			}
			else {
				CoroutineManager.StopCoroutine(this);
			}
		}
		public void End()
		{
			Active = false;
			Enumerator.Dispose();
		}
	}

}