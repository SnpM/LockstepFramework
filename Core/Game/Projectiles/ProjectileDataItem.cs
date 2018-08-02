using System;

namespace Lockstep.Data
{
	[Serializable]
	public class ProjectileDataItem : ObjectDataItem, IProjectileData
	{
		public LSProjectile GetProjectile()
		{
			return base.Prefab.GetComponent<LSProjectile>();
		}
	}
}