using UnityEngine;
using System.Collections; using FastCollections;
using System.Collections.Generic;
using System;
namespace Lockstep{
	public class ScanNode {

		public ScanNode ()
		{
		}

        public void Setup (int x, int y) {
            this.X = x;
            this.Y = y;
        }
        //One bucket for each AC of units that lands on this ScanNode
        private Dictionary<byte,FastBucket<LSInfluencer>> LocatedAgents = new Dictionary<byte,FastBucket<LSInfluencer>> ();

		public int X;
		public int Y;
		public int AgentCount;
        //Adds the agent and returns a ticket number
        public void Add (LSInfluencer influencer) {
            byte teamID = influencer.Agent.Controller.ControllerID;
            FastBucket<LSInfluencer> bucket;
            if (!LocatedAgents.TryGetValue(teamID, out bucket)) {
                bucket = new FastBucket<LSInfluencer>();
                LocatedAgents.Add(teamID, bucket);
                FastIterationBuckets.Add(new KeyValuePair<byte, FastBucket<LSInfluencer>>(teamID,bucket));
            }
            influencer.NodeTicket = bucket.Add(influencer);
			AgentCount++;
        }

        public void Remove (LSInfluencer influencer) {
            LocatedAgents[influencer.Agent.Controller.ControllerID].RemoveAt(influencer.NodeTicket);
			AgentCount--;
        }

        //Using this for no garbage collection from enumeration
        private FastList<KeyValuePair<byte,FastBucket<LSInfluencer>>> FastIterationBuckets = new FastList<KeyValuePair<byte,FastBucket<LSInfluencer>>>();

        public void GetBucketsWithAllegiance (Func<byte,bool> bucketConditional, FastList<FastBucket<LSInfluencer>> output) {
            for (int i = 0; i < FastIterationBuckets.Count; i++) {
                var pair = FastIterationBuckets[i];
                if (bucketConditional(pair.Key))
                    output.Add(pair.Value);
            }
        }
        /*
        public IEnumerable<FastBucket<LSInfluencer>> BucketsWithAllegiance (Func<byte,bool> bucketConditional) {
            foreach (KeyValuePair<byte,FastBucket<LSInfluencer>> pair in LocatedAgents) {
                if (bucketConditional (pair.Key))
                {
                    yield return pair.Value;
                }
            }
        }*/
	}
}