using System;
using UnityEngine;
namespace Lockstep
{
	public class LSInfluencer
	{
		#region Static Helpers
		static InfluencerBucket tempBucket;
		static GridNode tempNode;
		static LSInfluencer tempInfluencer;
		static InfluenceCoordinate tempCoordinate;
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
			if (LocatedNode != null) LocatedNode.Remove (this);
			LocatedNode = GridManager.GetNode (Body.Position.x,Body.Position.y);
			LocatedNode.Add (this);
		}

		public void Simulate ()
		{
			tempNode = GridManager.GetNode (Body.Position.x,Body.Position.y);

			if (Body.PositionChangedBuffer)
			{
				if (System.Object.ReferenceEquals (tempNode,LocatedNode) == false)
				{
					LocatedNode.LocatedAgents.Remove (this);
					LocatedNode = tempNode;
					LocatedNode.Add (this);
				}
			}
		}

		public LSInfluencer ScanForAny (RangeDelta deltas)
		{
			for (i = 0; i < deltas.coordinates.Length; i++)
			{
				tempCoordinate = deltas.coordinates[i];
				tempBucket = GridManager.GetNode(LocatedNode.gridX + tempCoordinate.x, LocatedNode.gridY + tempCoordinate.y).LocatedAgents;
				if (tempBucket != null)
				for (j = 0; j < tempBucket.PeakCount; j++)
				{
					if (LSUtility.GetBitTrue (tempBucket.arrayAllocation,j))
					{
						tempInfluencer = tempBucket.innerArray[j];
						if (System.Object.ReferenceEquals(tempInfluencer,this) == false)
						{
							return tempInfluencer;
						}
					}
				}
			}
			return null;
		}

		public void Deactivate ()
		{
			LocatedNode.Remove (this);
			LocatedNode = null;
		}
	}
}