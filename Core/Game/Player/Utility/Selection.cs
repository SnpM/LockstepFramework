using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
namespace Lockstep {
    public class Selection {
        private static readonly FastList<byte> bufferBites = new FastList<byte>();
        static readonly FastList<LSAgent> bufferAgents = new FastList<LSAgent>();

        private static int bigIndex, smallIndex;
        private static ulong castedBigIndex;
        private static byte cullGroup;
        private static byte castedSmallIndex;
        private static int curIndex;

		public FastList<ushort> selectedAgentLocalIDs = new FastList<ushort>();
        private  BitArray Header;
        private readonly FastList<byte> Data = new FastList<byte>();
       
        private AgentController leAgentController;

        public Selection() {}


        public Selection(FastEnumerable<LSAgent> selectedAgents) {
            
            Serialize(selectedAgents);
        }

        public byte[] GetBytes () {

            bufferBites.FastClear();
            //Serialize header
            int headerLength = Header.Length;
            int headerArraySize = (headerLength - 1) / 8 + 1;

            bufferBites.Add((byte)headerArraySize);
            byte[] headerBytes = new byte[headerArraySize];

            Header.CopyTo(headerBytes, 0);

            bufferBites.AddRange(headerBytes);

            //Serializing the good stuff
            for (int i = 0; i < Header.Length; i++) {
                if (Header.Get(i)) {
                    bufferBites.Add(Data[i]);
                }
            }

        }

        public void Serialize(FastEnumerable<LSAgent> selectedAgents) {

            Data.FastClear();
            selectedAgentLocalIDs.FastClear ();
			bufferAgents.FastClear ();
			selectedAgents.Enumerate (bufferAgents);
            ushort highestID = 0;
            for (int i = 0; i < bufferAgents.Count; i++) {
                ushort id = bufferAgents[i].LocalID;
                if (id > highestID) highestID = id;
            }
            int headerLength = (highestID + 1 - 1) / 8 + 1;
            Header = new BitArray(headerLength, false);
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

            Header.Set(bigIndex, true);
            Data.EnsureCapacity(bigIndex + 1);
            Data[bigIndex] |= (byte)(1 << smallIndex);

			selectedAgentLocalIDs.Add (agent.LocalID);
        }

        public int Reconstruct(byte[] source, int startIndex) {

            curIndex = startIndex;

            byte headerArraySize = source[curIndex++];

            byte[] headerBytes = new byte[headerArraySize];
            Array.Copy(source,curIndex,headerBytes,0,headerArraySize);
            curIndex += headerArraySize;
            Header = new BitArray(headerBytes);
            selectedAgentLocalIDs.FastClear();
            for (int i = 0; i < Header.Length; i++) {
                if (Header.Get(i)) {

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