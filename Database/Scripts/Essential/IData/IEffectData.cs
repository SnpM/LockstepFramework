using UnityEngine;
using System.Collections;

namespace Lockstep.Data
{
    public interface IEffectData : INamedData
    {
        LSEffect GetEffect ();
    }
}