using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

        //Adds the agent and returns a ticket number
        public void Add (LSInfluencer influencer) {
            byte teamID = influencer.Agent.Controller.ControllerID;
            FastBucket<LSInfluencer> bucket;
            if (!LocatedAgents.TryGetValue(teamID, out bucket)) {
                bucket = new FastBucket<LSInfluencer>();
                LocatedAgents.Add(teamID, bucket);
            }
            influencer.NodeTicket = bucket.Add(influencer);
        }

        public void Remove (LSInfluencer influencer) {
            LocatedAgents[influencer.Agent.Controller.ControllerID].RemoveAt(influencer.NodeTicket);
        }

        public IEnumerable<FastBucket<LSInfluencer>> BucketsWithAllegiance (LSAgent source, AllegianceType allegiance) {
            foreach (KeyValuePair<byte,FastBucket<LSInfluencer>> pair in LocatedAgents) {
                if ((source.Controller.GetAllegiance(pair.Key) & allegiance) != 0) {
                    yield return pair.Value;
                }
            }
        }
	}
}