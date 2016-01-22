using UnityEngine;
using System.Collections;
using Lockstep;
namespace Lockstep.Data
{
    public interface IAgentDataItem
    {
        LSAgent GetAgent ();
        string Name {get;}
    }
}