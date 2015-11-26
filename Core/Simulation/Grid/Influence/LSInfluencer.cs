using System;
using UnityEngine;

namespace Lockstep
{
	public class LSInfluencer
	{
		#region Static Helpers
		static LSAgent tempAgent;
		static GridNode tempNode;
		#endregion

		#region Collection Helper
		[NonSerialized]
		public int bucketIndex = -1;
		#endregion

		public GridNode LocatedNode { get; private set;}

		public LSBody Body { get; private set; }

		public LSAgent Agent { get; private set; }
		private int nodeIndex;

		public void Setup (LSAgent agent)
		{
			Agent = agent;
			Body = agent.Body;
		}

		public void Initialize ()
		{
			LocatedNode = GridManager.GetNode (Body.Position.x, Body.Position.y);
			nodeIndex = LocatedNode.Add (Agent);
		}

		public void Simulate ()
		{

			if (Body.PositionChangedBuffer) {
				tempNode = GridManager.GetNode (Body.Position.x, Body.Position.y);

				if (System.Object.ReferenceEquals (tempNode, LocatedNode) == false) {
					LocatedNode.RemoveAt (this.nodeIndex);
					nodeIndex = tempNode.Add (Agent);
					LocatedNode = tempNode;
				}
			}
		}

		public void Deactivate ()
		{
			LocatedNode.RemoveAt (this.nodeIndex);
			LocatedNode = null;
		}
        
		static AllegianceType TargetAllegiance;
		static PlatformType TargetPlatform;
		public LSAgent Scan (int deltaCount,
		                  AllegianceType targetAllegiance = AllegianceType.Any,
		                  PlatformType targetPlatform = PlatformType.Any)
		{
			InfluenceManager.Source = Agent;
			InfluenceManager.TargetAllegiance = targetAllegiance;
			InfluenceManager.TargetPlatform = targetPlatform;
			return InfluenceManager.Scan (LocatedNode.ScanX, LocatedNode.ScanY, deltaCount,
			                              InfluenceManager.ScanConditionalSourceWithHealthAction,
			                              Agent.Body.Position.x,
			                              Agent.Body.Position.y);
		}
                          
	}

	public enum PlatformType
	{
		Any,
		Air,
		Ground
	}
}