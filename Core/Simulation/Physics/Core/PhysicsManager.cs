using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using System;

namespace Lockstep
{
	public static class PhysicsManager
	{
		#region User-defined Variables
		public const bool SimulatePhysics = true;
		public const int BodySimulationSpread = 1;
		public const int ColSpreadMul = 1;
		#endregion

		#region Counters
		#endregion

		#region Simulation Variables
		public const int MaxSimObjects = 5000;
		public static LSBody[] SimObjects = new LSBody[MaxSimObjects];
		public static CollisionPair[] CollisionPairs = new CollisionPair[MaxSimObjects * MaxSimObjects];
		public static FastList<CollisionPair> FastCollisionPairs = new FastList<CollisionPair> (MaxSimObjects);
		#endregion

		#region Assignment Variables
		public static bool[] SimObjectExists = new bool[MaxSimObjects];
		public static int PeakCount = 0;
		private static FastStack<int> CachedIDs = new FastStack<int> (MaxSimObjects / 8);
		public static int AssimilatedCount = 0;
		#endregion

		#region Visualization
		public static float LerpTime;
		#endregion


		public static void Initialize ()
		{
			PeakCount = 0;

			CachedIDs.FastClear ();
			Array.Clear (SimObjects, 0, SimObjects.Length);
			Array.Clear (SimObjectExists, 0, SimObjectExists.Length);
			Array.Clear (CollisionPairs, 0, CollisionPairs.Length);

			PeakCount = 0;
			AssimilatedCount = 0;
			
			FastCollisionPairs.FastClear ();

			Partition.Initialize ();
		}

		public static int CurCount;
		static CollisionPair pair;
		static int i, j;
		static LSBody b1;

		public static void Simulate ()
		{
			

			for (i = 0; i < PeakCount; i++) {
				if (SimObjectExists [i]) {
					b1 = SimObjects [i];
					b1.EarlySimulate ();
				}
			}
			Partition.CheckAndDistributeCollisions ();
			for (i = 0; i < PeakCount; i++) {
				if (SimObjectExists [i]) {
					b1 = SimObjects [i];
					b1.Simulate ();
				}
			}
		}

		public static float ElapsedTime;

		public static void Visualize ()
		{
			for (i = 0; i < PeakCount; i++) {
				if (SimObjectExists [i]) {
					SimObjects [i].Visualize ();
				}
			}
		}

		static int id;
		static LSBody other;
		static int colPairIndex;

		public static void Assimilate (LSBody body)
		{
			if (CachedIDs.Count > 0) {
				id = CachedIDs.Pop ();
			} else {
				id = PeakCount;
				PeakCount++;
			}
			SimObjectExists [id] = true;
			SimObjects [id] = body;
			body.ID = id;


			for (i = 0; i < id; i++) {
				other = SimObjects [i];
				if (RequireCollisionPair (body, other)) {

					colPairIndex = i * MaxSimObjects + id;

					pair = CollisionPairs [colPairIndex];
					if (pair == null) {
						pair = new CollisionPair ();
						CollisionPairs [colPairIndex] = pair;
					}
					FastCollisionPairs.Add (pair);

					pair.Initialize (other, body);
				}
			}
			for (j = id + 1; j < PeakCount; j++) {
				other = SimObjects [j];
				if (RequireCollisionPair (body, other)) {
					colPairIndex = id * MaxSimObjects + j;

					pair = CollisionPairs [colPairIndex];
					if (pair == null) {
						pair = new CollisionPair ();
						CollisionPairs [colPairIndex] = pair;
					}
					FastCollisionPairs.Add (pair);

					
					pair.Initialize (body, other);
				}
			}

			AssimilatedCount++;
		}

		public static void Dessimilate (LSBody body)
		{
			if (!SimObjectExists [body.ID]) {
				Debug.LogWarning ("Object with ID" + body.ID.ToString () + "cannot be dessimilated because it it not assimilated");
				return;
			}

			SimObjectExists [body.ID] = false;
			CachedIDs.Add (body.ID);

			for (i = 0; i < FastCollisionPairs.Count; i++) {
				pair = FastCollisionPairs.innerArray [i];
				if (pair.Body1 == body || pair.Body2 == body) {
					pair.Deactivate ();
				}
			}

			AssimilatedCount--;
		}

		public static int GetCollisionPairIndex (int ID1, int ID2)
		{
			if (ID1 < ID2) {
				return ID1 * MaxSimObjects + ID2;
			} else {				
				return ID2 * MaxSimObjects + ID1;
			}
		}

		public static bool RequireCollisionPair (LSBody body1, LSBody body2)
		{
			if (
				!Physics2D.GetIgnoreLayerCollision (body1.cachedGameObject.layer, body2.cachedGameObject.layer) &&
				(!body1.Immovable || !body2.Immovable) &&
				(!body1.IsTrigger || !body2.IsTrigger)) {
				return true;
			}
			return false;
		}

	}
}