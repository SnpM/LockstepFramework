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
		//TODO: THIS DICTIONARY IS NOT DETERMINISTIC WHEN ITERATING!
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
			var bucket = LocatedAgents [influencer.Agent.Controller.ControllerID];
			bucket.RemoveAt(influencer.NodeTicket);
			//Important! This ensure sync for the next game session.
			if (bucket.Count == 0) {
				bucket.SoftClear ();
			}
			AgentCount--;
        }

        //Using this for no garbage collection from enumeration
        private FastList<KeyValuePair<byte,FastBucket<LSInfluencer>>> FastIterationBuckets = new FastList<KeyValuePair<byte,FastBucket<LSInfluencer>>>();

		public FastList<KeyValuePair<byte,FastBucket<LSInfluencer>>> GetBuckets()
		{
			return FastIterationBuckets;
		}

        public void GetBucketsWithAllegiance (Func<byte,bool> bucketConditional, FastList<FastBucket<LSInfluencer>> output) {
            for (int i = 0; i < FastIterationBuckets.Count; i++) {
                var pair = FastIterationBuckets[i];
                if (bucketConditional(pair.Key))
                    output.Add(pair.Value);
            }
        }
	}
}