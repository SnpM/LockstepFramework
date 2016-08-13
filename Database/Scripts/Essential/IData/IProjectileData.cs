using UnityEngine;
using System.Collections;

namespace Lockstep.Data
{
    public interface IProjectileData : INamedData
    {
        LSProjectile GetProjectile();
    }
}