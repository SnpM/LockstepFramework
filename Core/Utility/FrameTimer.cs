using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lockstep
{
	/// <summary>
	/// Helps with discrete time-based events
	/// </summary>
	public class FrameTimer
	{
		public int Interval { get; private set; }
		/// <summary>
		/// Pauses the frame advancement
		/// </summary>
		public bool IsPaused { get; set; }
		private int accumulator;

		/// <summary>
		/// Interval: How many inactive frames between each active frame.
		/// </summary>
		/// <param name="interval"></param>
		public FrameTimer(int interval)
		{
			Interval = interval;
		}
		public FrameTimer()
		{
			Interval = 0;
		}
		/// <summary>
		/// Returns true once every Interval frames
		/// </summary>
		/// <returns></returns>
		public bool AdvanceFrame()
		{
			accumulator++;
			if (accumulator >= Interval)
			{
				accumulator = 0;
				return true;
			}
			return false;
		}
		public bool AdvanceFrames(int amount)
		{
			accumulator += amount;
			if (accumulator >= Interval)
			{
				accumulator %= Interval;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			accumulator = 0;
		}
		/// <summary>
		/// Reset the timer and use a new interval
		/// </summary>
		/// <param name="newInterval"></param>
		public void Reset(int newInterval)
		{
			Interval = newInterval;
			Reset();
		}
	}
}