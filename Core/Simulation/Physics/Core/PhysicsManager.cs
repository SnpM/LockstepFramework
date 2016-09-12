//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================

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


		static double FixedDeltaTime {
			get {
				return 1d / LockstepManager.FrameRate;
			}
		}

		static int VisualSetSpread {
			get {
				return 2;
			}
		}

		public static bool SettingsChanged { get; private set; }

		private static PhysicsSettings _settings = new PhysicsSettings ();

		public static PhysicsSettings Settings {
			get {
				return _settings;
			}
			set {
				_settings = value;
				SettingsChanged = true;
			}
		}

		#endregion

		#region Counters

		#endregion

		#region Simulation Variables 

		public const int DefaultMaxSimObjects = 200 * 200;
		private static int _maxSimObjects = DefaultMaxSimObjects;
		public static int MaxSimObjects {
			get {
				return _maxSimObjects;
			}
			set {
				_maxSimObjects = value;
			}
		}
		public static LSBody [] SimObjects = new LSBody [MaxSimObjects];
		public static FastBucket<LSBody> DynamicSimObjects = new FastBucket<LSBody> (MaxSimObjects / 4);

		private static Dictionary<int, CollisionPair> CollisionPairs = new Dictionary<int, CollisionPair> (MaxSimObjects);

		#endregion

		#region Assignment Variables

		public static int PeakCount = 0;
		private static FastStack<int> CachedIDs = new FastStack<int> (MaxSimObjects / 8);
		public static int AssimilatedCount = 0;

		#endregion

		#region Visualization

		#endregion

		public static void Setup ()
		{
			SimObjects = new LSBody [MaxSimObjects];
			Partition.Setup ();
		}


		public static void Initialize ()
		{
			Raycaster._Version = 0;
			PeakCount = 0;

			CachedIDs.FastClear ();
			//CollisionPairs.Clear ();
			//SimObjects.Clear ();
			CollisionPair.CurrentCollisionPair = null;

			PeakCount = 0;
			AssimilatedCount = 0;

			Partition.Initialize ();

			if (SettingsChanged) {
				SettingsChanged = false;
			}

			AccumulatedTime = 0;
			LastTime = 0;
		}


		public static void Simulate ()
		{

			Partition.CheckAndDistributeCollisions ();

			Simulated = true;
		}

		public static void LateSimulate ()
		{


			for (int i = 0; i < DynamicSimObjects.PeakCount; i++) {
                LSBody b1 = DynamicSimObjects.innerArray [i];
				if (b1.IsNotNull ()) {
					b1.Simulate ();
				}
			}

		}

		public static void Deactivate ()
		{
			CollisionPairs.Clear ();
			Partition.Deactivate ();
		}

		public static float LerpTime { get; private set; }
		public static float ExtrapolationAmount { get; private set; }
		public static float LerpDamping { get; private set; }

		private static float LerpDampScaler;

		private static double LastTime { get; set; }

		public static double AccumulatedTime { get; private set; }

		public static bool Simulated { get; private set; }

		public static void Visualize ()
		{
			LerpDamping = 1f;
			double curTime = LockstepManager.Seconds;
			AccumulatedTime += (curTime - LastTime) * Time.timeScale;
			LerpTime = (float)(AccumulatedTime / FixedDeltaTime);
			if (LerpTime < 1f) {
				for (int i = 0; i < DynamicSimObjects.PeakCount; i++) {
                    LSBody b1 = DynamicSimObjects.innerArray [i];

					if (b1.IsNotNull ()) {
						b1.Visualize ();
					}
				}
			} else {
				AccumulatedTime %= FixedDeltaTime;
				ExtrapolationAmount = LerpTime;
				LerpTime = (float)(AccumulatedTime / FixedDeltaTime);

				if (Simulated) {
					for (int i = 0; i < DynamicSimObjects.PeakCount; i++) {
                        LSBody b1 = DynamicSimObjects.innerArray [i];

						if (b1.IsNotNull ()) {
							b1.LerpOverReset ();

							b1.SetVisuals ();

							b1.Visualize ();
						}
					}
					Simulated = false;
				} else {
					for (int i = 0; i < DynamicSimObjects.PeakCount; i++) {
                        LSBody b1 = DynamicSimObjects.innerArray [i];

						if (b1.IsNotNull ()) {
							b1.LerpOverReset ();
							b1.SetExtrapolatedVisuals ();

							b1.Visualize ();

						}
					}
				}

			}
			LastTime = curTime;
		}

		public static float ElapsedTime;


		static int id;
		static LSBody other;
		internal static int Assimilate (LSBody body, bool isDynamic)
		{
			if (CachedIDs.Count > 0) {
				id = CachedIDs.Pop ();
			} else {
				id = PeakCount;
				PeakCount++;
			}
			SimObjects [id] = body;

            //Important: If isDynamic is false, PhysicsManager won't check to update the item every frame. When the object is changed, it must be updated manually.
			if (isDynamic) {
				body.DynamicID = DynamicSimObjects.Add (body);
			}
			AssimilatedCount++;
			return id;
		}

		private static CollisionPair CreatePair (LSBody body1, LSBody body2)
		{
			int pairIndex = GetCollisionPairIndex (body1.ID, body2.ID);
			CollisionPair pair = new CollisionPair ();
			CollisionPairs [pairIndex] = pair;
			pair.Initialize (body1, body2);
			return pair;

		}

		internal static void Dessimilate (LSBody body)
		{
			int tid = body.ID;

			if (!SimObjects [tid].IsNotNull ()) {
				Debug.LogWarning ("Object with ID" + body.ID.ToString () + "cannot be dessimilated because it it not assimilated");
				return;
			}

			SimObjects [tid] = null;
			CachedIDs.Add (tid);

			for (int i = 0; i < PhysicsManager.PeakCount; i++) {
				if (PhysicsManager.SimObjects [i].IsNotNull ()) {
					if (i != tid) {
						int pairIndex = GetCollisionPairIndex (i, tid);
						CollisionPair pair;
						if (CollisionPairs.TryGetValue (pairIndex, out pair)) {
							pair.Deactivate ();
						}
					}
				}
			}

			if (body.DynamicID >= 0) {
				DynamicSimObjects.RemoveAt (body.DynamicID);
				body.DynamicID = -1;
			}
		}

		public static CollisionPair GetCollisionPair (int ID1, int ID2)
		{
			CollisionPair pair;
			int pairIndex = GetCollisionPairIndex (ID1, ID2);
			if (SimObjects [ID1].IsNotNull() && SimObjects [ID2].IsNotNull()) {
				if (CollisionPairs.TryGetValue (pairIndex, out pair) == false) {
					pair = CreatePair (SimObjects [ID1], SimObjects [ID2]);
				} else if (pair.Active == false) {
					pair.Initialize (SimObjects [ID1], SimObjects [ID2]);
				}
				return pair;

			}
			return null;
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
				Physics2D.GetIgnoreLayerCollision (body1.Layer, body2.Layer) == false &&
				(!body1.Immovable || !body2.Immovable) &&
				(!body1.IsTrigger || !body2.IsTrigger) &&
				(body1.Shape != ColliderType.None && body2.Shape != ColliderType.None)) {
				return true;
			}
			return false;
		}

	}
}