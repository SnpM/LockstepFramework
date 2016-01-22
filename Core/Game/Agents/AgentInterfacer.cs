using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using Lockstep.Data;
namespace Lockstep.Data
{
	[Serializable]
	public class AgentInterfacer : ObjectDataItem, IAgentDataItem
	{
        public AgentInterfacer (string name, string description) : this(){
            base._name = name;
            base._description = description;
        }
        public AgentInterfacer(){}

        public LSAgent GetAgent () {
            if (this.Prefab != null)
            {
                LSAgent agent = this.Prefab.GetComponent<LSAgent> ();
                if (agent) {
                    return agent;
                }
            }
            return null;
        }
        public int SortDegreeFromAgentType (AgentType agentType) {
            LSAgent agent = GetAgent ();
            if (agent == null) return -1;
            if (agentType == agent.MyAgentType) return 1;
            return 0;
        }
	}
}
