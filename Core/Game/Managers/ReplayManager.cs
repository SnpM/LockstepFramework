using UnityEngine;
using System.Collections;

namespace Lockstep
{
	public static class ReplayManager
	{
		public static RecordState recordState;

		public static void Simulate ()
		{

		}

	}
	public enum RecordState {
		None,
		Record,
		Playback
	}
}