using UnityEngine;
using System.Collections; using FastCollections;
using System;
using System.Collections.Generic;

using Lockstep.Data;
namespace Lockstep.Data
{
	[Serializable]
	public class AgentDataItem : ObjectDataItem, IAgentData
	{


        public AgentDataItem (string name, string description) : this(){
            base._name = name;
            base._description = description;
        }
        public AgentDataItem(){
            
        }

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

#if UNITY_EDITOR

		GameObject lastPrefab;
		protected override void OnManage ()
		{
			
			if (lastPrefab != Prefab) {
				if (string.IsNullOrEmpty(Name))
					this._name = Prefab.name;
				lastPrefab = Prefab;
			}
		}
		
		#endif

	}
}
