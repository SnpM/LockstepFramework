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
		private static FastList<Command> OutCommands = new FastList<Command> ();
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
			if (Offline) {
				ReceivedBytes.AddRange (BitConverter.GetBytes(LockstepManager.FrameCount));
				for(i = 0; i < OutCommands.Count; i++)
				{
					ReceivedBytes.AddRange (OutCommands[i].Serialized);
				}
			} else {

			}

			Frame frame = FrameManager.Frames [LockstepManager.FrameCount];
			int frameCount = BitConverter.ToInt32 (ReceivedBytes.innerArray,0);
			Index = 4;
			while (Index < ReceivedBytes.Count) {
				Command com = new Command();
				Index += com.Reconstruct (ReceivedBytes.innerArray, Index);
				frame.AddCommand (com);
			}

			ReceivedBytes.FastClear ();
			OutCommands.FastClear ();
		}

		public static void SendCommand (Command com)
		{
			OutCommands.Add (com);
		}

		static int i, j, Index;
	}
}