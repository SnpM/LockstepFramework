using UnityEngine;
using System.Collections;
using Lockstep;
namespace Lockstep.Data
{
    public interface IAgentData : INamedData
    {
        LSAgent GetAgent ();
    }
}