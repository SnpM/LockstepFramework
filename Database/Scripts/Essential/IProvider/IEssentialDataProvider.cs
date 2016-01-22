using UnityEngine;
using System.Collections;

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