using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep.Data
{
    public interface IProjectileData : INamedData
    {
        LSProjectile GetProjectile();
    }
}