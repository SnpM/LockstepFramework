using UnityEngine;
using System.Collections; using FastCollections;
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
        }
        public static void Simulate ()
        {
            for (int i = ProjectileBucket.PeakCount - 1; i >= 0; i--)
			{
                if (ProjectileBucket.arrayAllocation[i])
				{
					ProjectileBucket[i].Simulate ();
				}
			}

            for (int i = NDProjectileBucket.PeakCount - 1; i >= 0; i--) {
                if (NDProjectileBucket.arrayAllocation[i]) {
                    NDProjectileBucket[i].Simulate();
                }
            }
        }
        public static void Visualize ()
	    {
            for (int i = ProjectileBucket.PeakCount - 1; i >= 0; i--)
			{
				if (ProjectileBucket.arrayAllocation[i]) {
					if (ProjectileBucket[i] != null) {
						ProjectileBucket[i].Visualize();
					}
				}
			}
            VisualizeBucket (NDProjectileBucket);
        }
        private static void VisualizeBucket (FastBucket<LSProjectile> bucket) {
            for (int i = bucket.PeakCount - 1; i >= 0; i--) {
                if (bucket.arrayAllocation[i]) {
                    bucket[i].Visualize();
                }
            }
        }

		public static void Deactivate ()
		{
            for (int i = ProjectileBucket.PeakCount - 1; i >= 0; i--)
			{
                if (ProjectileBucket.arrayAllocation[i])
				{
					EndProjectile (ProjectileBucket[i]);
				}
			}
            for (int i = NDProjectileBucket.PeakCount - 1; i>= 0; i--) {
                if (NDProjectileBucket.arrayAllocation[i]) {
                    EndProjectile(NDProjectileBucket[i]);
                }
            }
		}

        public static int GetStateHash () {
            int hash = 23;
            for (int i = ProjectileBucket.PeakCount - 1; i>= 0; i--) {
                if (ProjectileBucket.arrayAllocation[i]) {
                    LSProjectile proj = ProjectileManager.ProjectileBucket[i];
                    hash ^= proj.GetStateHash ();
                }
            }
            return hash;
        }

        private static LSProjectile NewProjectile (string projCode)
		{
            IProjectileData projData = CodeDataMap[projCode];
			if (projData.GetProjectile().gameObject != null) {
				curProj = ((GameObject)GameObject.Instantiate<GameObject>(projData.GetProjectile().gameObject)).GetComponent<LSProjectile>();
				if (curProj != null) {
					curProj.Setup(projData);
					return curProj;
				}
				else return null;
			}
			else return null;
		}
        public static LSProjectile Create (string projCode, LSAgent source, Vector3d offset, AllegianceType targetAllegiance, Func<LSAgent,bool> agentConditional,Action<LSAgent> hitEffect) {
            Vector2d relativePos = offset.ToVector2d();
            Vector2d worldPos = relativePos.Rotated(source.Body.Rotation) + source.Body.Position;
            Vector3d pos = new Vector3d(worldPos.x,worldPos.y,offset.z + source.Body.HeightPos);
            AgentController sourceController = source.Controller;
            LSProjectile proj = Create (
                projCode,
                pos,
                agentConditional,
                (bite) => {
                return ((sourceController.GetAllegiance(bite) & targetAllegiance) != 0);
            }
                ,
                hitEffect);
            return proj;
        }
        public static LSProjectile Create (string projCode, Vector3d position, Func<LSAgent,bool> agentConditional, Func<byte,bool> bucketConditional, Action<LSAgent> hitEffect)
		{
            curProj = RawCreate (projCode);

            int id = ProjectileBucket.Add(curProj);
			curProj.Prepare (id, position,agentConditional,bucketConditional, hitEffect, true);
			return curProj;
		}
        private static LSProjectile RawCreate (string projCode) {
            if (ProjectilePool.ContainsKey (projCode) == false) {
                Debug.Log(projCode + " Caused boom");
                return null;
            }
            FastStack<LSProjectile> pool = ProjectilePool[projCode];
            if (pool.Count > 0)
            {
                curProj = pool.Pop ();
            }
            else {
                curProj = NewProjectile (projCode);
            } 
            return curProj;
        }
		public static void Fire (LSProjectile projectile)
		{
			if (projectile != null) {
				projectile.LateInit();
			}
		}

        private static FastBucket<LSProjectile> NDProjectileBucket = new FastBucket<LSProjectile>();
        public static LSProjectile NDCreateAndFire (string projCode, Vector3d position, Vector3d direction, bool gravity = false) {
			if (curProj != null) {
				curProj = RawCreate(projCode);
				int id = NDProjectileBucket.Add(curProj);
				curProj.Prepare(id, position, (a) => false, (a) => false, (a) => { }, false);
				curProj.InitializeFree(direction, (a) => false, gravity);
				ProjectileManager.Fire(curProj);
				return curProj;
			}
			else return null;
        }

		public static void EndProjectile (LSProjectile projectile)
		{
            if (projectile.Deterministic) {
    			int id = projectile.ID;
                if(!ProjectileBucket.SafeRemoveAt(id,projectile)) {
                    Debug.Log("BOO! This is a terrible bug.");
                }
            }
            else {
                if (!NDProjectileBucket.SafeRemoveAt(projectile.ID,projectile)) {
                    Debug.Log("BOO! This is a terrible bug.");
                }
            }
			CacheProjectile (projectile);
			projectile.Deactivate ();
		}

		#region ID and allocation management
        private static readonly Dictionary<string, FastStack<LSProjectile>> ProjectilePool = new Dictionary<string, FastStack<LSProjectile>>();
        private static FastBucket<LSProjectile> ProjectileBucket = new FastBucket<LSProjectile>();

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