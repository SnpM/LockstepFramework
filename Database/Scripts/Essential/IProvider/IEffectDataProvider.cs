using UnityEngine;
using System.Collections;

namespace Lockstep.Data
{
    public interface IEffectDataProvider
    {
        IEffectData[] EffectData {get;}
    }
}