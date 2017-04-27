using UnityEngine;
using System.Collections; using FastCollections;
using System;
using Lockstep;
#if UNITY_EDITOR
using UnityEditor;
using Lockstep.Integration;
#endif
namespace Lockstep.Data
{
	[Serializable]
	public class ProjectileDataItem : ObjectDataItem, IProjectileData
	{
        public LSProjectile GetProjectile () {
            return base.Prefab.GetComponent<LSProjectile>();
        }
    }
}