using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep.Data
{
    public interface IEssentialDataProvider :
    IAgentDataProvider
    ,IAbilityDataProvider
    ,IEffectDataProvider
    ,IInputDataProvider
    ,IProjectileDataProvider
    {

    }
}