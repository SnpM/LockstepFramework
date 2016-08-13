using UnityEngine;
using System.Collections;

namespace Lockstep.Data
{
    public interface IAbilityDataProvider
    {
        AbilityDataItem[] AbilityData { get; }
    }
}