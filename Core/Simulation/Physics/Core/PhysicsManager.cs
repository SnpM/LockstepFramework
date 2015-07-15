using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

namespace Lockstep
{
	public static class PhysicsManager
	{
		#region User-defined Variables
		public const bool SimulatePhysics = true;
		public const int CollisionIterationSpread = 3;
		public const int ColSpreadMul = CollisionIterationSpread;

		public static int CollisionIterationCount;
		public static int CollisionIterationRemain;
		public static int CollisionIterationMark;
		public static int CollisionPairCount = 0;
		#endregion

		#region Simulation Variables
		public const int MaxSimObjects = 3120;
		public static LSBody[] SimObjects = new LSBody[MaxSimObjects];
		public static CollisionPair[] CollisionPairs = new CollisionPair[MaxSimObjects * MaxSimObjects];
		public static FastList<CollisionPair> FastCollisionPairs = new FastList<CollisionPair> (MaxSimObjects);
		#endregion

		#region Assignment Variables
		public static bool[] SimObjectExists = new bool[MaxSimObjects];
		public static int PeakCount = 0;
		private static Stack<int> CachedIDs = new Stack<int> (MaxSimObjects / 8);
		public static int AssimilatedCount = 0;
		#endregion

		#region Visualization
		public static float LerpTime;
		#endregion
		public static void Initialize ()
		{
			CollisionIterationCount = CollisionIterationSpread;
			CollisionIterationMark = 0;
			Partition.Initialize ();
		}

		static int fastColPairCount;
		static int CurCount;
		static CollisionPair pair;
		static int i,j;
		public static void Simulate ()
		{

			CollisionIterationRemain = (CollisionPairCount) / CollisionIterationSpread + 1;
			if (CollisionIterationCount == CollisionIterationSpread) {
				CollisionIterationCount = 0;
				CollisionIterationMark = 0;
			} else {
				CollisionIterationMark += CollisionIterationRemain;
			}


			fastColPairCount = FastCollisionPairs.Count;
			CurCount = 0;
			for (i = 0; i < fastColPairCount; i++)
			{
				pair = FastCollisionPairs[i];
				CurCount ++;
				if (CollisionIterationRemain == 0 || CurCount < CollisionIterationMark) {
					
					pair.DistributeCollision ();
				} else {
					pair.CheckAndDistributeCollision ();
					CollisionIterationRemain--;
				}
			}


			if (CollisionIterationCount == 0) {


				for (i = 0; i < PeakCount; i++) {
					if (SimObjectExists [i]) {
						LSBody b1 = SimObjects [i];
						b1.EarlySimulate ();
						if (b1.PositionChanged) {
							Partition.Body = b1;
							Partition.PartitionObject ();
						}

						b1.Simulate ();
					}
				}

				Partition.EstablishPartitions ();

			} else {
				for (i = 0; i < PeakCount; i++) {
					if (SimObjectExists [i]) {
						LSBody b1 = SimObjects [i];
						b1.EarlySimulate ();
						b1.Simulate ();
					}
				}
			}



			CollisionIterationCount++;


		}

		public static float ElapsedTime;

		public static void Visualize ()
		{
			return;

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
				if (!Physics2D.GetIgnoreLayerCollision (other.gameObject.layer, body.gameObject.layer)) {
					colPairIndex = i * MaxSimObjects + id;

					pair = CollisionPairs[colPairIndex];
					if (pair == null)
					{
						pair = new CollisionPair ();
						CollisionPairs [colPairIndex] = pair;
						FastCollisionPairs.Add (pair);
					}

					pair.Initialize (other, body);
					CollisionPairCount++;
				}
			}
			for (j = id + 1; j < PeakCount; j++) {
				other = SimObjects [j];
				if (!Physics2D.GetIgnoreLayerCollision (other.gameObject.layer, body.gameObject.layer)) {
					colPairIndex = id * MaxSimObjects + j;

					pair = CollisionPairs[colPairIndex];
					if (pair == null)
					{
						pair = new CollisionPair ();
						CollisionPairs [colPairIndex] = pair;
						FastCollisionPairs.Add (pair);
					}
					
					pair.Initialize (body, other);
					CollisionPairCount++;
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
			CachedIDs.Push (body.ID);

			fastColPairCount = FastCollisionPairs.Count;

			for (i = 0; i < fastColPairCount; i++)
			{
				pair = FastCollisionPairs.innerArray[i];
				if (pair.Body1 == body || pair.Body2 == body)
				{
					pair.Deactivate ();
					CollisionPairCount--;
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

		public static void End ()
		{
			for (i = 0; i < PeakCount; i++) {
				SimObjectExists [i] = false;
			}
			CachedIDs.Clear ();
			PeakCount = 0;
			AssimilatedCount = 0;

			CollisionPairCount = 0;
			FastCollisionPairs = new FastList<CollisionPair> ();
		}
	}
}