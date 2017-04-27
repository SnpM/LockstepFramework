using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep.Data
{
    public interface IEffectDataProvider
    {
        IEffectData[] EffectData {get;}
    }
}