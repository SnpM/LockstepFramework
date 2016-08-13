using UnityEngine;
using System.Collections;

namespace Lockstep.Data
{
    public interface IProjectileDataProvider
    {
        IProjectileData[] ProjectileData {get;}
    }
}