using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep.Data
{
    public interface IAgentDataProvider
    {
        IAgentData[] AgentData {get;}
    }
}