using UnityEngine;
using System.Collections;

namespace Lockstep.Data
{
    public interface IAgentDataProvider
    {
        IAgentData[] AgentData {get;}
    }
}