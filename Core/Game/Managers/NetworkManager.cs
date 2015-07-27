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
		public static FastList<byte> ReceivedBytes = new FastList<byte> ();
		public static FastList<byte> AllReceivedBytes = new FastList<byte> (30000);
		
		public static void Initialize ()
		{
			AllReceivedBytes.FastClear ();
		}

		static void HandleonDataDetailed (ushort sender, byte tag, ushort subject, object data)
		{
			ReceivedBytes = new FastList<byte> ((byte[])data);
		}

		public static void Simulate ()
		{
			if (Offline) {
					ReceivedBytes.AddRange (BitConverter.GetBytes (LockstepManager.FrameCount));
					for (i = 0; i < OutCommands.Count; i++) {
						ReceivedBytes.AddRange (OutCommands [i].Serialized);
					}
					AllReceivedBytes.AddRange (BitConverter.GetBytes (ReceivedBytes.Count));
					AllReceivedBytes.AddRange (ReceivedBytes);
			} else {

			}

			int frameCount = BitConverter.ToInt32 (ReceivedBytes.innerArray, 0);
			Index = 4;

			FrameManager.EnsureCapacity (frameCount + 1);

			Frame frame;
			if (!FrameManager.HasFrame [frameCount]) {
				frame = new Frame ();
				FrameManager.AddFrame (frameCount, frame);

				while (Index < ReceivedBytes.Count) {
					Command com = new Command ();
					Index += com.Reconstruct (ReceivedBytes.innerArray, Index);
					frame.AddCommand (com);
				}
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