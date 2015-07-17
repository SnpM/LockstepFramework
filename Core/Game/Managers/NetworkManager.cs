using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

namespace Lockstep
{
	public static class NetworkManager
	{
		public static bool Offline = true;
		private static FastList<Command> BufferedCommands = new FastList<Command> ();
		private static FastList<byte> BufferedBytes = new FastList<byte> ();
		private static FastList<byte> ReceivedBytes = new FastList<byte> ();
		
		public static void Initialize ()
		{
		}

		static void HandleonDataDetailed (ushort sender, byte tag, ushort subject, object data)
		{
			ReceivedBytes = new FastList<byte> ((byte[])data);
		}

		public static void Simulate ()
		{
			if (Offline)
			{
				Frame frame = FrameManager.Frames[LockstepManager.FrameCount];

				frame.AddCommands (BufferedCommands.innerArray, 0, BufferedCommands.Count);
			}
			else
			{
				BufferedBytes.FastClear ();
				BufferedBytes.AddRange (BitConverter.GetBytes (LockstepManager.FrameCount));
				for (i = 0; i < BufferedCommands.Count; i++)
				{
					BufferedBytes.AddRange (BufferedCommands[i].Serialized);
				}

				Frame frame = FrameManager.Frames[LockstepManager.FrameCount];
				while (Index < ReceivedBytes.Count)
				{
					frame.AddCommand (new Command ());
					Index += frame.Commands[frame.Commands.Count - 1].Reconstruct (ReceivedBytes.innerArray, Index);
				}
				ReceivedBytes.FastClear ();
			}


			BufferedCommands.FastClear ();
		}

		public static void SendCommand (Command com)
		{
			BufferedCommands.Add (com);
		}

		static int i, j, Index;
	}
}