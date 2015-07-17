using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Lockstep
{
	public static class FrameManager
	{
		static int i;

		const int StartCapacity = 30000;
		public static bool[] HasFrame = new bool[StartCapacity];
		public static Frame[] Frames = new Frame[StartCapacity];
		public static int Capacity = StartCapacity;

		public static int ForeSight = 0;
		public static int NextFrame;

		public static bool CanAdvanceFrame {get{return ForeSight > 0;}}

		public static void Initialize ()
		{
			ForeSight = 0;
			NextFrame = 0;
		}

		public static void Simulate ()
		{
			ForeSight--;
			Frame frame = Frames[LockstepManager.FrameCount];
			if (frame.Commands != null)
			for (i = 0; i < frame.Commands.Count; i++)
			{

			}
		}

		public static void CheckForesight ()
		{

		}
		
		public static void AddFrame (int index, Frame frame)
		{
			if (index >= Capacity)
			{
				Capacity *= 2;
				Array.Resize (ref Frames,Capacity);
				Array.Resize (ref HasFrame, Capacity);
			}
			Frames[index] = frame;
			HasFrame[index] = true;

			if (index == NextFrame) {
				ForeSight++;
				NextFrame++;
				while (HasFrame[NextFrame])
				{
					NextFrame++;
				}
			}
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