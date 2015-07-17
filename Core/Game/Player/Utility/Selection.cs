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
		public FastList<LSAgent> selectedAgents;
		public ulong Header;
		public byte[] Data;

		public void SerializeFromSelectionManager ()
		{
			Data = new byte[64];
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
			Data = new byte[64];
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

			if (selectedAgents == null)
				selectedAgents = new FastList<LSAgent> (64);
			else
				selectedAgents.FastClear ();

			for (i = 0; i < 64; i++) {
				castedBigIndex = (ulong)1 << i;
				if ((Header & castedBigIndex) == castedBigIndex) {
					CullGroup = source [curIndex++];
					for (j = 0; j < 8; j++) {
						castedSmallIndex = (byte)(1 << j);
						if ((CullGroup & (castedSmallIndex)) == castedSmallIndex)
						{
							selectedAgents.Add (agentController.ActiveAgents [(ushort)(i * 8 + j)]);
						}
					}
				}
			}
			return curIndex - startIndex;
		}

		public override string ToString ()
		{
			string s = "Selected Agents: ";
			if (selectedAgents != null) {
				for (i = 0; i < selectedAgents.Count; i++) {
					s += selectedAgents [i].LocalID.ToString () + ", ";
				}
			}
			return s;
		}
	}
}