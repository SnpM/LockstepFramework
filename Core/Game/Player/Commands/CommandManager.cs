using System;
using UnityEngine;

namespace Lockstep
{
    public static class CommandManager
    {
        const int defaultSize = 1000;

        public static int LastCommandedFrameCount { get; private set; }

        #region Recording

        public static FastList<byte> RecordedBytes = new FastList<byte>(defaultSize);

        #endregion

        static readonly FastList<Command> outCommands = new FastList<Command>(defaultSize);
        static readonly FastList<byte> bufferedBytes = new FastList<byte>(256);

        public static void Initialize()
        {
            RecordedBytes.FastClear();
            outCommands.FastClear();
            LastCommandedFrameCount = -1;
        }

        public static void Simulate()
        {
            SendOut ();
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

            if (packet == null || packet.Count < 4)
            {
                throw new System.Exception("Packet is null or not valid length");
            }
            int frameCount = BitConverter.ToInt32(packet.innerArray, 0);
            int index = 4;

            if (FrameManager.HasFrame(frameCount) == false)
            {
                Frame frame = new Frame();

                if (packet.Count > 4)
                {
                    RecordedBytes.AddRange(BitConverter.GetBytes((ushort)packet.Count));
                    RecordedBytes.AddRange(packet);
                    while (index < packet.Count)
                    {
                        Command com = new Command();
                        index += com.Reconstruct(packet.innerArray, index);
                        frame.AddCommand(com);
                    }
                    if (frameCount > LastCommandedFrameCount)
                    {
                        LastCommandedFrameCount = frameCount;
                    }
                }
                FrameManager.AddFrame(frameCount, frame);

            } else
            {

            }
        }

        /// <summary>
        /// Sends out all Commands
        /// </summary>
        public static void SendOut () {
            if (outCommands.Count > 0) {
                bufferedBytes.FastClear();

                for (int i = 0; i < outCommands.Count; i++)
                {
                    bufferedBytes.AddRange(outCommands [i].Serialized);
                }
                if (bufferedBytes.Count > 0)
                    ClientManager.Distribute(bufferedBytes.ToArray());

                outCommands.FastClear();
            }
        }

        public static void SendCommand(Command com, bool immediate = false)
        {
            outCommands.Add(com);
            if (immediate)
            {
                SendOut ();
            }
        }
    }

}