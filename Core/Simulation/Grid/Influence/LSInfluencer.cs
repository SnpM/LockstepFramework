using System;
using UnityEngine;

namespace Lockstep
{
	public class LSInfluencer
	{
		#region Static Helpers
		static InfluencerBucket tempBucket;
		static GridNode tempNode;
		static InfluenceCoordinate tempCoordinate;
		static LSAgent tempAgent;
		static int i, j;
		#endregion

		#region Collection Helper
		public int bucketIndex;
		#endregion

		public GridNode LocatedNode;
		public LSBody Body;
		public LSAgent Agent;

		public void Initialize (LSAgent agent)
		{
			Agent = agent;
			Body = agent.Body;
			if (LocatedNode != null)
				LocatedNode.Remove (this);
			LocatedNode = GridManager.GetNode (Body.Position.x, Body.Position.y);
			LocatedNode.Add (this);
		}

		public void Simulate ()
		{
			tempNode = GridManager.GetNode (Body.Position.x, Body.Position.y);

			if (Body.PositionChangedBuffer) {
				if (System.Object.ReferenceEquals (tempNode, LocatedNode) == false) {
					LocatedNode.LocatedAgents.Remove (this);
					LocatedNode = tempNode;
					LocatedNode.Add (this);
				}
			}
		}

		#region Scanning
		public LSAgent Scan (RangeDelta deltas,
		                     bool CheckAllegiance = false,
		                     AllegianceType allegianceType = AllegianceType.Neutral)
		{
			for (i = 0; i < deltas.coordinates.Length; i++) {
				tempCoordinate = deltas.coordinates [i];
				tempNode = GridManager.GetNode (LocatedNode.gridX + tempCoordinate.x, LocatedNode.gridY + tempCoordinate.y);

				if (tempNode != null && tempNode.LocatedAgents != null) {
					tempBucket = tempNode.LocatedAgents;
					for (j = 0; j < tempBucket.PeakCount; j++) {
						if (LSUtility.GetBitTrue (tempBucket.arrayAllocation, j)) {
							tempAgent = tempBucket.innerArray [j].Agent;
							if (System.Object.ReferenceEquals (tempAgent, Agent) == false) {
								if (CheckAllegiance)
								{
									if (Agent.MyAgentController.DiplomacyFlags
									    [tempAgent.MyAgentController.ControllerID] != allegianceType) continue;
								}
								return tempAgent;
							}
						}
					}
				}
			}
			return null;
		}
		public void ScanAll (RangeDelta deltas,
		                     FastList<LSAgent> outputAgents,
		                     bool CheckAllegiance = false,
		                     AllegianceType allegianceType = AllegianceType.Neutral)
		{
			outputAgents.FastClear ();
			for (i = 0; i < deltas.coordinates.Length; i++) {
				tempCoordinate = deltas.coordinates [i];
				tempNode = GridManager.GetNode (LocatedNode.gridX + tempCoordinate.x, LocatedNode.gridY + tempCoordinate.y);
				
				if (tempNode != null && tempNode.LocatedAgents != null) {
					tempBucket = tempNode.LocatedAgents;
					for (j = 0; j < tempBucket.PeakCount; j++) {
						if (LSUtility.GetBitTrue (tempBucket.arrayAllocation, j)) {
							tempAgent = tempBucket.innerArray [j].Agent;
							if (System.Object.ReferenceEquals (tempAgent, Agent) == false) {
								if (CheckAllegiance)
								{
									if (Agent.MyAgentController.DiplomacyFlags
									    [tempAgent.MyAgentController.ControllerID] != allegianceType) continue;
								}
								outputAgents.Add(tempAgent);
							}
						}
					}
				}
			}
		}


		#endregion

		public void Deactivate ()
		{
			LocatedNode.Remove (this);
			LocatedNode = null;
		}
	}
}