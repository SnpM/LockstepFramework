using System;
using UnityEngine;
using System.Collections.Generic;
using FastCollections;
namespace Lockstep
{
    public static class CommandManager
    {

        #region Recording


        #endregion

        static readonly FastList<byte> bufferedBytes = new FastList<byte>(256);

        public static void Initialize()
        {
        }

        public static void Simulate()
        {
            SendOut();
        }
		public static void Visualize()
		{
			SendOut();
		}

        public static void ProcessPacket(byte[] packet)
        {
            processBytes.FastClear();
            processBytes.AddRange(packet);
            ProcessPacket(processBytes);
        }

        static FastList<byte> processBytes = new FastList<byte>();

        public static void ProcessPacket(FastList<byte> packet)
        {
			if (ReplayManager.IsPlayingBack) return;
            if (packet == null || packet.Count < 4)
            {
                throw new System.Exception("Packet is null or not valid length");
            }
            int frameCount = BitConverter.ToInt32(packet.innerArray, 0);
            int index = 4;

            Frame frame = new Frame();

			// packet.Count > 6 is a guard for random extra bytes bug with DarkRift implementation
            if (packet.Count > 4 && packet.Count > 6)
            {
				while (index < packet.Count)
                {
                    Command com = new Command();
                    index += com.Reconstruct(packet.innerArray, index);
                    frame.AddCommand(com);
                }

			}
            ProcessFrame(frameCount, frame);
        }

        public static void ProcessFrame(int frameCount, Frame frame)
        {
            if (FrameManager.HasFrame(frameCount) == false)
            {
                FrameManager.AddFrame(frameCount, frame);
            }

        }

        /// <summary>
        /// Sends out all Commands
        /// </summary>
        public static void SendOut()
        {

			if (bufferedBytes.Count > 0)
			{
				ClientManager.Distribute(bufferedBytes.ToArray());

				bufferedBytes.FastClear();
			}
            
        }
		//static FastList<Command> asdf = new FastList<Command>();
        public static void SendCommand(Command com)
        {
			if (com == null)
			{
				return;
			}

			bufferedBytes.AddRange(com.Serialized);

        }
    }

}