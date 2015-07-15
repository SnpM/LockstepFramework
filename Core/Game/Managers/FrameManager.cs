using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Lockstep
{
	public static class FrameManager
	{
		public static Dictionary<int,Frame> Frames = new Dictionary<int, Frame> (30000);

		public static void Initialize ()
		{

		}
		public static void EarlySimulate ()
		{
			Frames.Add (LockstepManager.FrameCount, new Frame ());
		}
		public static void Simulate ()
		{
			Frame curFrame = Frames[LockstepManager.FrameCount];
		}
	}
	public class Frame {
		public FastList<Command> Commands;

		public void AddCommand (Command com)
		{
			if (Commands == null)
			{
				Commands = new FastList<Command> ();
			}
			Commands.Add (com);
		}

		public void AddCommands (Command[] coms, int startIndex, int count)
		{
			if (Commands == null)
			{
				Commands = new FastList<Command> ();
			}
			Commands.AddRange (coms,startIndex,count);
		}
	}
}