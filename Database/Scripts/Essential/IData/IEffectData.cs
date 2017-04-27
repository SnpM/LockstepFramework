using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep.Data
{
    public interface IEffectData : INamedData
    {
        LSEffect GetEffect ();
    }
}