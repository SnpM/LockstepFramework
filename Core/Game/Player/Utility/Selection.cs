using System;
using System.Collections.Generic;
using System.Collections; using FastCollections;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
namespace Lockstep {
    public class Selection : ICommandData{
        private static readonly FastList<byte> bufferBites = new FastList<byte>();

        private static int bigIndex, smallIndex;
        private static ulong castedBigIndex;
        private static byte cullGroup;
        private static byte castedSmallIndex;
        private static int curIndex;

		public FastList<ushort> selectedAgentLocalIDs = new FastList<ushort>();
        private  BitArray Header;
        private readonly FastList<byte> Data = new FastList<byte>();
       
        public Selection() {}


        static readonly FastList<LSAgent> bufferAgents = new FastList<LSAgent>();
        public Selection(FastEnumerable<LSAgent> selectedAgents) {
            bufferAgents.FastClear();
            selectedAgents.Enumerate(bufferAgents);
            this.AddAgents(bufferAgents.ToArray());
        }

        public byte[] GetBytes () {
            this.Serialize();

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
            return bufferBites.ToArray();
        }

        public void AddAgents (params LSAgent[] agents) {
            for (int i = 0; i < agents.Length; i++) {
                selectedAgentLocalIDs.Add(agents[i].LocalID);
            }
        }

        private void Serialize() {

            Data.FastClear();
            ushort highestID = 0;
            for (int i = 0; i < selectedAgentLocalIDs.Count; i++) {
                ushort id = selectedAgentLocalIDs[i];
                if (id > highestID) highestID = id;
            }
            int headerLength = (highestID + 1 - 1) / 8 + 1;
            Header = new BitArray(headerLength, false);
			for (int i = 0; i < selectedAgentLocalIDs.Count; i++) {
                SerializeID(selectedAgentLocalIDs[i]);
            }

        }

        private void SerializeID(ushort id) {

            bigIndex = (id / 8);
            smallIndex = (id % 8);

            Header.Set(bigIndex, true);
            Data.EnsureCapacity(bigIndex + 1);
            Data[bigIndex] |= (byte)(1 << smallIndex);
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

        public void Write (Writer writer) {
            writer.Write(this.GetBytes());
        }
        public void Read (Reader reader) {
            int move = this.Reconstruct(reader.Source, reader.Position);
            reader.MovePosition(move);
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