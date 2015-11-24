using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using Lockstep.Data;
#if UNITY_EDITOR
using UnityEditor;
using prop = UnityEditor.SerializedProperty;
using Lockstep.Integration;
#endif
namespace Lockstep.Data
{
	[Serializable]
	public class AgentInterfacer : ObjectDataItem
	{
        public AgentInterfacer (string name, string description) : base(){
            base._name = name;
            base._description = description;
        }
        public AgentInterfacer(){}
        public AgentCode GetAgentCode () {
            return (AgentCode)base.MappedCode;
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
	}
}
