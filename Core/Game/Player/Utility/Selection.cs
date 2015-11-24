using System;
using System.Collections.Generic;
namespace Lockstep {
    public class Selection {
        private static int bigIndex, smallIndex;
        private static ulong castedBigIndex;
        private static byte cullGroup;
        private static byte castedSmallIndex;
        private static int curIndex;

		public FastList<ushort> selectedAgentLocalIDs = new FastList<ushort>();
        public ulong Header;
        public readonly byte[] Data = new byte[64];

        private AgentController leAgentController;

        public Selection() {}

		static readonly FastList<LSAgent> bufferAgents = new FastList<LSAgent>();

        public Selection(FastEnumerable<LSAgent> selectedAgents) {
            Serialize(selectedAgents);
        }

        public void Serialize(FastEnumerable<LSAgent> selectedAgents) {
            Array.Clear(Data, 0, 64);
            Header = 0;
			selectedAgentLocalIDs.FastClear ();
			bufferAgents.FastClear ();
			selectedAgents.Enumerate (bufferAgents);
			for (int i = 0; i < bufferAgents.Count; i++) {
                SerializeAgent(bufferAgents[i]);
            }
        }

        private void SerializeAgent(LSAgent agent) {
            if (leAgentController == null) {
                leAgentController = agent.Controller;
            }

            bigIndex = (agent.LocalID / 8);
            smallIndex = (agent.LocalID % 8);

            Header |= (ulong)1 << bigIndex;
            Data[bigIndex] |= (byte)(1 << smallIndex);

			selectedAgentLocalIDs.Add (agent.LocalID);
        }

        public int Reconstruct(byte[] source, int startIndex) {
            curIndex = startIndex;
            Header = BitConverter.ToUInt64(source, curIndex);
            curIndex += 8;

            selectedAgentLocalIDs.FastClear();
            

			for (int i = 0; i < 64; i++) {
                castedBigIndex = (ulong)1 << i;
                if ((Header & castedBigIndex) == castedBigIndex) {
                    cullGroup = source[curIndex++];
					for (int j = 0; j < 8; j++) {
                        castedSmallIndex = (byte)(1 << j);
                        if ((cullGroup & (castedSmallIndex)) == castedSmallIndex) {
                            selectedAgentLocalIDs.Add((ushort)(i * 8 + j));
                        }
                    }
                }
            }
            return curIndex - startIndex;
        }

        public override string ToString() {
            string s = "Selected Agents: ";
            if (selectedAgentLocalIDs .IsNotNull ()) {
				for (int i = 0; i < selectedAgentLocalIDs.Count; i++) {
                    s += selectedAgentLocalIDs[i] + ", ";
                }
            }
            return s;
        }
    }
}