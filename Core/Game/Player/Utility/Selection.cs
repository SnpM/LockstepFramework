using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
	public class Selection
	{

		static int i, j;
		static int BigIndex, SmallIndex;
		public AgentController LeAgentController;
		public FastList<int> selectedAgentLocalIDs;
		public ulong Header;
		public byte[] Data = new byte[64];

		public void SerializeFromSelectionManager ()
		{
			Array.Clear (Data,0,64);
			Header = 0;
			for (i = 0; i < SelectedAgents.PeakCount; i++) {
				if (SelectedAgents.arrayAllocated [i]) {
					LSAgent agent = SelectedAgents.innerArray [i];
					BigIndex = (agent.LocalID / 8);
					SmallIndex = (agent.LocalID % 8);
					
					Header |= (ulong)1 << BigIndex;
					Data [BigIndex] |= (byte)(1 << SmallIndex);
				}
			}
		}

		public void Serialize (FastList<LSAgent> selectedAgents)
		{
			Array.Clear (Data,0,64);

			Header = 0;
			for (i = 0; i < selectedAgents.Count; i++) {
				LSAgent agent = selectedAgents.innerArray [i];
				BigIndex = (agent.LocalID / 8);
				SmallIndex = (agent.LocalID % 8);
					
				Header |= (ulong)1 << BigIndex;
				Data [BigIndex] |= (byte)(1 << SmallIndex);

			}
		}

		static ulong castedBigIndex;
		static byte CullGroup;
		static byte castedSmallIndex;
		static int curIndex;

		public int Reconstruct (AgentController agentController, byte[] source, int startIndex)
		{
			curIndex = startIndex;
			Header = BitConverter.ToUInt64 (source, curIndex);
			curIndex += 8;

			if (selectedAgentLocalIDs == null)
				selectedAgentLocalIDs = new FastList<int> (64);
			else
				selectedAgentLocalIDs.FastClear ();

			for (i = 0; i < 64; i++) {
				castedBigIndex = (ulong)1 << i;
				if ((Header & castedBigIndex) == castedBigIndex) {
					CullGroup = source [curIndex++];
					for (j = 0; j < 8; j++) {
						castedSmallIndex = (byte)(1 << j);
						if ((CullGroup & (castedSmallIndex)) == castedSmallIndex)
						{
							selectedAgentLocalIDs.Add (i * 8 + j);
						}
					}
				}
			}
			return curIndex - startIndex;
		}

		public override string ToString ()
		{
			string s = "Selected Agents: ";
			if (selectedAgentLocalIDs != null) {
				for (i = 0; i < selectedAgentLocalIDs.Count; i++) {
					s += selectedAgentLocalIDs [i].ToString () + ", ";
				}
			}
			return s;
		}
	}
}