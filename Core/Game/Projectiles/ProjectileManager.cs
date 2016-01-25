using UnityEngine;
using System.Collections;
using System;
using Lockstep.Data;
using System.Collections.Generic;
namespace Lockstep {
    public static class ProjectileManager {
		public const int MaxProjectiles = 1 << 13;
        private static string[] AllProjCodes;
        private static readonly Dictionary<string,IProjectileData> CodeDataMap = new Dictionary<string, IProjectileData>();


		public static void Setup ()
        {
           IProjectileDataProvider prov;
            if (LSDatabaseManager.TryGetDatabase<IProjectileDataProvider> (out prov)) {
                IProjectileData[] projectileData = prov.ProjectileData;
                for (int i = 0; i < projectileData.Length; i++)
                {
                    IProjectileData item = projectileData[i];
                    CodeDataMap.Add(item.Name, item);
                    ProjectilePool.Add(item.Name, new FastStack<LSProjectile> ());
                }
            }
        }
        public static void Initialize ()
        {
			Array.Clear(ProjectileActive,0,ProjectileActive.Length);
			OpenSlots.FastClear ();
			PeakCount = 0;
        }
        public static void Simulate ()
        {
			for (int i = 0; i < PeakCount; i++)
			{
				if (ProjectileActive[i])
				{
					ProjectileBucket[i].Simulate ();
				}
			}
        }
        public static void Visualize ()
	    {
			for (int i = 0; i < PeakCount; i++)
			{
				if (ProjectileActive[i])
				{
					ProjectileBucket[i].Visualize ();
				}
			}
        }

		public static void Deactivate ()
		{
			for (int i = 0; i < PeakCount; i++)
			{
				if (ProjectileActive[i])
				{
					EndProjectile (ProjectileBucket[i]);
				}
			}
		}

        public static int GetStateHash () {
            int hash = 23;
            for (int i = 0; i < PeakCount; i++) {
                if (ProjectileActive[i]) {
                    LSProjectile proj = ProjectileManager.ProjectileBucket[i];
                    hash ^= proj.GetStateHash ();
                }
            }
            return hash;
        }

        private static LSProjectile NewProjectile (string projCode)
		{
            IProjectileData projData = CodeDataMap[projCode];
            curProj = ((GameObject)GameObject.Instantiate<GameObject> (projData.GetProjectile().gameObject)).GetComponent<LSProjectile> ();
			curProj.Setup (projData);
			return curProj;
		}
        public static LSProjectile Create (string projCode, LSAgent source, Vector3d offset, AllegianceType targetAllegiance, Func<LSAgent,bool> agentConditional,Action<LSAgent> hitEffect) {
            Vector3d pos = offset;
            pos.SetVector2d(pos.ToVector2d().Rotated(source.Body._rotation.x,source.Body._rotation.y));
            pos.Add(ref source.Body._position);
            return Create (projCode,pos,agentConditional,(bite) => ((source.Controller.GetAllegiance(bite) & targetAllegiance) != 0),hitEffect);
        }
        public static LSProjectile Create (string projCode, Vector3d position, Func<LSAgent,bool> agentConditional, Func<byte,bool> bucketConditional, Action<LSAgent> hitEffect)
		{
			FastStack<LSProjectile> pool = ProjectilePool[projCode];
			if (pool.Count > 0)
			{
				curProj = pool.Pop ();
			}
			else {
				curProj = NewProjectile (projCode);
			}
			int id = GenerateID ();
			ProjectileBucket[id] = curProj;
			ProjectileActive[id] = true;
			curProj.Prepare (id, position,agentConditional,bucketConditional, hitEffect);
			return curProj;
		}
		public static void Fire (LSProjectile projectile)
		{
			projectile.LateInit ();
		}

		public static void EndProjectile (LSProjectile projectile)
		{
			int id = projectile.ID;
			if (ProjectileActive[id] == false) {
				return;
			}
			if (ProjectileBucket[id] != projectile)
			{
				return;
			}
			ProjectileActive[id] = false;
			ProjectileBucket[id] = null;
			OpenSlots.Add (id);

			CacheProjectile (projectile);
			projectile.Deactivate ();
		}

		#region ID and allocation management
        private static readonly Dictionary<string, FastStack<LSProjectile>> ProjectilePool = new Dictionary<string, FastStack<LSProjectile>>();
		private static bool[] ProjectileActive = new bool[MaxProjectiles];
		private static LSProjectile[] ProjectileBucket = new LSProjectile[MaxProjectiles];

		private static FastStack<int> OpenSlots = new FastStack<int>(MaxProjectiles / 4);
		private static int PeakCount;
		private static int GenerateID ()
		{
			if (OpenSlots.Count > 0)
			{
				return OpenSlots.Pop ();
			}
			return PeakCount++;
		}
		private static void CacheProjectile (LSProjectile projectile)
		{
			ProjectilePool[projectile.MyProjCode].Add (projectile);
			/*if (projectile.ID == PeakCount - 1)
			{
				PeakCount--;
				for (int i = projectile.ID - 1; i >= 0; i--)
				{
					if (ProjectileActive[i] == false)
					{
						PeakCount--;
					}
					else {
						break;
					}
				}
			}*/
		}
		#endregion

		#region Helpers
		static LSAgent curAgent;
		static LSProjectile curProj;
		#endregion
    }

}