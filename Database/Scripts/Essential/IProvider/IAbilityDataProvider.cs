using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep.Data
{
    public interface IAbilityDataProvider
    {
        AbilityDataItem[] AbilityData { get; }
    }
}