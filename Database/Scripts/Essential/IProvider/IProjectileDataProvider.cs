using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep.Data
{
    public interface IProjectileDataProvider
    {
        IProjectileData[] ProjectileData {get;}
    }
}