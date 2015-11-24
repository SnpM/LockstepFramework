using System;
using UnityEngine;

namespace Lockstep
{
	public static class CommandManager
	{
		const int defaultSize = 1 << 15;
		public const SendState defaultSendState = SendState.Network;
		public static SendState sendType = defaultSendState;

		public static int LastCommandedFrameCount {get; private set;}

        #region Recording
		public static FastList<byte> RecordedBytes = new FastList<byte> (defaultSize);
        #endregion

		static int NextFrameCount;
		static readonly FastList<Command> outCommands = new FastList<Command> (defaultSize);
		static readonly FastList<byte> bufferedBytes = new FastList<byte> (256);
		const bool SendAggregated = false;

		public static void Initialize ()
		{
			RecordedBytes.FastClear ();
			outCommands.FastClear ();
			NextFrameCount = 0;
			LastCommandedFrameCount = -1;
		}

		public static void Simulate ()
		{
			if (sendType == SendState.None) {
				return;
			}

			{
				bufferedBytes.FastClear ();

				switch (sendType) {
				case SendState.Network:
					if (SendAggregated) {
						for (int i = 0; i < outCommands.Count; i++) {
							bufferedBytes.AddRange (outCommands [i].Serialized);
						}
						if (bufferedBytes.Count > 0)
							ClientManager.Distribute (bufferedBytes.ToArray ());
					}
					break;
				case SendState.Offline:
					bufferedBytes.AddRange (BitConverter.GetBytes (LockstepManager.InfluenceFrameCount));
					for (int i = 0; i < outCommands.Count; i++) {
						bufferedBytes.AddRange (outCommands [i].Serialized);
					}
					ProcessPacket (bufferedBytes.ToArray ());
					break;
				}
				outCommands.FastClear ();
				NextFrameCount++;
                
			}
		}

		public static void ProcessPacket (byte[] packet)
		{
			processBytes.FastClear ();
			processBytes.AddRange (packet);
			ProcessPacket (processBytes);
		}

		static FastList<byte> processBytes = new FastList<byte> ();

		public static void ProcessPacket (FastList<byte> packet)
		{
			if (packet == null || packet.Count < 4) {
                throw new System.Exception("Packet is null or not valid length");
			}
			int frameCount = BitConverter.ToInt32 (packet.innerArray, 0);
			int index = 4;

			if (FrameManager.HasFrame (frameCount) == false) {
				Frame frame = new Frame ();

				if (packet.Count > 4) {
					RecordedBytes.AddRange (BitConverter.GetBytes ((ushort)packet.Count));
					RecordedBytes.AddRange (packet);
					while (index < packet.Count) {
						Command com = new Command ();
						index += com.Reconstruct (packet.innerArray, index);
						frame.AddCommand (com);
					}
					if (frameCount > LastCommandedFrameCount) {
						LastCommandedFrameCount = frameCount;
                    }
                }
				FrameManager.AddFrame (frameCount, frame);

			} else {

			}
		}

		public static void SendCommand (Command com)
		{
			if (sendType == SendState.None) {
				return;
			}
			outCommands.Add (com);

			if (SendAggregated == false) {
				bufferedBytes.FastClear ();
				if (sendType == SendState.Network) {
					for (int i = 0; i < outCommands.Count; i++) {
						bufferedBytes.AddRange (outCommands [i].Serialized);
					}
					outCommands.FastClear ();
					if (bufferedBytes.Count > 0)
						ClientManager.Distribute (bufferedBytes.ToArray ());
				}
			}
		}
	}
    
	public enum SendState
	{
		None, //Sends no commands and processes no bytes
		Offline, //Automatically sends commands offline
		Network //Sends and receives commands from the network
	}
}