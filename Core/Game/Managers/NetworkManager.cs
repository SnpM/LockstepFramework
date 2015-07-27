using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

namespace Lockstep
{
	public static class NetworkManager
	{
		public static SendState sendState = SendState.Autosend;
		private static FastList<Command> OutCommands = new FastList<Command> ();
		public static FastList<byte> ReceivedBytes = new FastList<byte> ();
		public static int ReceivedFrameCount;
		public static int IterationCount;


		#region Recording
		public static FastList<byte> RecordedBytes = new FastList<byte> (30000);
		public static int LastRecordedFrame;
		#endregion

		public static void Initialize ()
		{
			RecordedBytes.FastClear ();
			ReceivedBytes.FastClear ();
			OutCommands.FastClear ();
			IterationCount = LockstepManager.NetworkingIterationSpread;
			ReceivedFrameCount = 0;
			LastRecordedFrame = 0;
		}

		static void HandleonDataDetailed (ushort sender, byte tag, ushort subject, object data)
		{
			ReceivedBytes = new FastList<byte> ((byte[])data);
		}

		public static void Simulate ()
		{
			if (IterationCount == 0) {
				switch (sendState) {
				case SendState.Autosend:
					ReceivedBytes.AddRange (BitConverter.GetBytes (LockstepManager.FrameCount));
					for (i = 0; i < OutCommands.Count; i++) {
						ReceivedBytes.AddRange (OutCommands [i].Serialized);
					}

					break;
				}

				if (ReceivedBytes.Count < 4)
					return;



				int frameCount = BitConverter.ToInt32 (ReceivedBytes.innerArray, 0);
				Index = 4;

				FrameManager.EnsureCapacity (frameCount + 1);

				Frame frame;
				if (!FrameManager.HasFrame [frameCount]) {
					ReceivedFrameCount++;
					if (ReceivedBytes.Count > 4) {
						RecordedBytes.AddRange (BitConverter.GetBytes ((ushort)ReceivedBytes.Count));
						RecordedBytes.AddRange (ReceivedBytes);
						LastRecordedFrame = ReceivedFrameCount;
					}

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

				IterationCount = LockstepManager.NetworkingIterationSpread - 1;
			} else {
				IterationCount--;
				FrameManager.AddFrame (LockstepManager.FrameCount, new Frame ());
			}
		}

		public static void SendCommand (Command com)
		{
			if (sendState == SendState.None) {
				return;
			}

			OutCommands.Add (com);
		}

		static int i, j, Index;
	}

	public enum SendState
	{
		None, //Sends no commands and processes no bytes
		Autosend, //Automatically sends commands offline
		Network //Sends and receives commands from the network
	}
}