using FastCollections;
using System;

namespace Lockstep
{
	public class ScanNode
	{
		public void Setup(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		//Agents are sorted into buckets based on their AC
		private FastList<FastBucket<LSInfluencer>> AgentBuckets = new FastList<FastBucket<LSInfluencer>>();

		public int X;
		public int Y;
		public int AgentCount;
		//Adds the agent and returns a ticket number
		public void Add(LSInfluencer influencer)
		{
			byte controllerID = influencer.Agent.Controller.ControllerID;

			if (AgentBuckets.Count <= controllerID)
			{
				//fill up indices up till the desired bucket's index
				for (int i = controllerID - AgentBuckets.Count; i >= 0; i--)
				{
					AgentBuckets.Add(null);
				}
			}

			FastBucket<LSInfluencer> bucket = AgentBuckets[controllerID];
			if (bucket == null)
			{
				//A new bucket for the controller must be created
				bucket = new FastBucket<LSInfluencer>();
				AgentBuckets[controllerID] = bucket;
			}

			influencer.NodeTicket = bucket.Add(influencer);
			AgentCount++;
		}

		public void Remove(LSInfluencer influencer)
		{
			var bucket = AgentBuckets[influencer.Agent.Controller.ControllerID];
			bucket.RemoveAt(influencer.NodeTicket);
			//Important! This ensure sync for the next game session.
			if (bucket.Count == 0)
			{
				//Buckets can be SoftCleared beause previous allocation flags will be outside the scope of the new bucket's cycle
				bucket.SoftClear();
			}
			AgentCount--;
		}

		public void GetBucketsWithAllegiance(Func<byte, bool> bucketConditional, FastList<FastBucket<LSInfluencer>> output)
		{
			//Linear search for desired buckets
			for (byte i = 0; i < AgentBuckets.Count; i++)
			{
				var bucket = AgentBuckets[i];
				if (bucket.IsNotNull())
				{
					if (bucketConditional(i))
						output.Add(bucket);
				}
			}
		}
	}
}