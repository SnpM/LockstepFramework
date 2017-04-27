using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;
namespace Lockstep.Data
{
    public interface IAgentData : INamedData
    {
        LSAgent GetAgent ();
    }
}