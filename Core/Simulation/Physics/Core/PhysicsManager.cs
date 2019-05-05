//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================
using UnityEngine;
using FastCollections;
using System;

namespace Lockstep
{
	public static class PhysicsManager
	{

		#region User-defined Variables

		public const bool SimulatePhysics = true;

		#region Culling
		//After a certain amount of frames have passed without collision, culling frequency will increase
		//Currently scales to have BlockSize result in CulFrequencyMax
		internal const long CullDistanceStep =
			(((Partition.BlockSize + FixedMath.One * 2) * (Partition.BlockSize + FixedMath.One * 2)) >> FixedMath.SHIFT_AMOUNT)
			/ CullDistanceMax;
		//Maximum amount of frames to wait between checks
		internal const int CullDistanceMax = LockstepManager.FrameRate / 3;
		internal const long CullFastDistanceMax = (FixedMath.One * 4) * (FixedMath.One * 4);

		internal const int CullTimeStep = LockstepManager.FrameRate * 3;
		internal const int CullTimeMax = LockstepManager.FrameRate / 5;
		#endregion

		static int _cullDistributor;
		internal static int CullDistributor
		{
			get
			{
				if (_cullDistributor > 1)
					_cullDistributor = -1;
				return _cullDistributor++;
			}
		}
		static double FixedDeltaTime
		{
			get
			{
				return 1d / LockstepManager.FrameRate;
			}
		}

		static int VisualSetSpread
		{
			get
			{
				return 2;
			}
		}

		public static bool SettingsChanged { get; private set; }

		private static PhysicsSettings _settings = new PhysicsSettings();

		public static PhysicsSettings Settings
		{
			get
			{
				return _settings;
			}
			set
			{
				_settings = value;
				SettingsChanged = true;
			}
		}

		#endregion

		#region Counters

		#endregion

		#region Simulation Variables

		public const int DefaultMaxSimObjects = 2048;
		private static int _maxSimObjects = DefaultMaxSimObjects;

		public static int MaxSimObjects
		{
			get
			{
				return _maxSimObjects;
			}
			set
			{
				_maxSimObjects = value;
			}
		}

		public static LSBody[] SimObjects = new LSBody[MaxSimObjects];
		public static FastBucket<LSBody> DynamicSimObjects = new FastBucket<LSBody>(MaxSimObjects / 4);

		private static long _positionCycler;
		/// <summary>
		/// Prevents units from getting stuck in perfect collisions.
		/// </summary>
		/// <value>The position cycler.</value>
		public static long PositionCycler
		{
			get
			{
				_positionCycler += 13;
				const long maxPositionCycler = FixedMath.One / 256;
				if (_positionCycler > maxPositionCycler)
					_positionCycler -= maxPositionCycler * 2;
				return _positionCycler;

			}
		}
		#endregion

		#region Assignment Variables

		public static int PeakCount = 0;
		private static FastStack<int> CachedIDs = new FastStack<int>(MaxSimObjects / 8);
		public static int AssimilatedCount = 0;

		#endregion

		#region Visualization

		#endregion

		public static void Setup()
		{
			SimObjects = new LSBody[MaxSimObjects];
			Partition.Setup();
		}


		public static void Initialize()
		{

			if (SettingsChanged)
			{
				SettingsChanged = false;
			}


			ResetVars();
		}

		static void ResetVars()
		{
			for (int i = 0; i < PeakCount; i++)
			{
				SimObjects[i] = null;
			}
			DynamicSimObjects.FastClear();
			Raycaster._Version = 0;
			PeakCount = 0;
			CachedIDs.FastClear();

			CollisionPair.CurrentCollisionPair = null;

			PeakCount = 0;
			AssimilatedCount = 0;

			Partition.Initialize();
			RanCollisionPairs.FastClear();
			InactiveCollisionPairs.FastClear();

			_positionCycler = 0;
		}


		public static void Simulate()
		{
			Partition.CheckAndDistributeCollisions();
			Simulated = true;
		}

		internal static FastBucket<InstanceCollisionPair> RanCollisionPairs = new FastBucket<InstanceCollisionPair>();
		internal static FastQueue<InstanceCollisionPair> InactiveCollisionPairs = new FastQueue<InstanceCollisionPair>();

		public struct InstanceCollisionPair
		{
			public InstanceCollisionPair(ushort version, CollisionPair pair)
			{
				Version = version;
				Pair = pair;
			}
			public ushort Version;
			public CollisionPair Pair;
		}

		public static bool ResetAccumulation { get; private set; }
		public static void LateSimulate()
		{
			//TODO: Look into this
			int inactiveFrameThreshold = LockstepManager.FrameRate * 8;


			for (int i = 0; i < RanCollisionPairs.PeakCount; i++)
			{
				if (RanCollisionPairs.arrayAllocation[i])
				{
					var instancePair = RanCollisionPairs[i];
					var pair = RanCollisionPairs[i].Pair;

					if (instancePair.Version != instancePair.Pair._Version)
					{
						//pair is removed at Deactivate so no longer possible
					}
					else
					{
						if (pair._ranIndex >= 0)
						{
							RanCollisionPairs.RemoveAt(pair._ranIndex);
							pair._ranIndex = -1;
							InactiveCollisionPairs.Add(instancePair);
						}
					}
				}
			}

			//Clear the buffer of collision pairs to turn off and pool
			while (InactiveCollisionPairs.Count > 0)
			{
				var instancePair = InactiveCollisionPairs.Peek();
				var pair = instancePair.Pair;

				if (pair.Active)
				{
					//It's active again! Get it out of inactives and move on to the next guy.
					InactiveCollisionPairs.Remove();
				}

				var passedFrames = LockstepManager.FrameCount - pair.LastFrame;
				if (passedFrames >= inactiveFrameThreshold)
				{
					InactiveCollisionPairs.Remove();
					FullDeactivateCollisionPair(pair);
				}
				else
				{
					break;
				}
			}

			for (int i = 0; i < DynamicSimObjects.PeakCount; i++)
			{
				LSBody b1 = DynamicSimObjects.innerArray[i];
				if (b1.IsNotNull())
				{
					b1.Simulate();
				}
			}
			ResetAccumulation = true;

		}

		public static void Deactivate()
		{
			Partition.Deactivate();

		}


		public static bool Simulated { get; private set; }
		public static double AccumulatedTime { get; private set; }
		public static double ExpectedAccumulation { get; private set; }
		public static float LerpTime { get; private set; }
		public static void LateVisualize()
		{

		}
		public static void Visualize()
		{
			LerpTime = Time.fixedDeltaTime;
			if (ResetAccumulation)
			{
				AccumulatedTime = 0;
			}
			AccumulatedTime += Time.deltaTime;
			ExpectedAccumulation = AccumulatedTime / LerpTime;
			for (int i = 0; i < DynamicSimObjects.PeakCount; i++)
			{
				LSBody b1 = DynamicSimObjects.innerArray[i];
				if (b1.IsNotNull())
				{
					b1.SetVisuals();
				}
			}

			ResetAccumulation = false;

		}
		public static float ElapsedTime;


		static int id;
		static LSBody other;

		internal static int Assimilate(LSBody body, bool isDynamic)
		{
			if (CachedIDs.Count > 0)
			{
				id = CachedIDs.Pop();
			}
			else
			{
				id = PeakCount;
				PeakCount++;
				if (PeakCount == SimObjects.Length)
				{
					//very very expensive
					Array.Resize(ref SimObjects, SimObjects.Length * 2);
				}
			}
			SimObjects[id] = body;

			//Important: If isDynamic is false, PhysicsManager won't check to update the item every frame. When the object is changed, it must be updated manually.
			if (isDynamic)
			{
				body.DynamicID = DynamicSimObjects.Add(body);
			}
			AssimilatedCount++;
			return id;
		}

		private static FastStack<CollisionPair> CachedCollisionPairs = new FastStack<CollisionPair>();

		private static CollisionPair CreatePair(LSBody body1, LSBody body2)
		{
			CollisionPair pair;
			if (CachedCollisionPairs.Count > 0)
			{
				pair = CachedCollisionPairs.Pop();
			}
			else
			{
				pair = new CollisionPair();
			}
			pair.Initialize(body1, body2);
			return pair;

		}
		public static void RemovePairReferences(CollisionPair pair)
		{
			pair.Body1.CollisionPairs.Remove(pair.Body2.ID);
			pair.Body2.CollisionPairHolders.Remove(pair.Body1.ID);
		}
		public static void FullDeactivateCollisionPair(CollisionPair pair)
		{
			if (pair.Active)
			{
				DeactivateCollisionPair(pair);
				RemovePairReferences(pair);
			}

		}
		public static void DeactivateCollisionPair(CollisionPair pair)
		{
			if (pair.Active)
			{
				if (pair._ranIndex >= 0)
				{
					PhysicsManager.RanCollisionPairs.RemoveAt(pair._ranIndex);
					pair._ranIndex = -1;
				}

				PoolPair(pair);
			}
		}

		public static void PoolPair(CollisionPair pair)
		{
			pair.Deactivate();
			if (LockstepManager.PoolingEnabled)
				CachedCollisionPairs.Add(pair);
		}

		internal static void Dessimilate(LSBody body)
		{
			int tid = body.ID;

			if (!SimObjects[tid].IsNotNull())
			{
				Debug.LogWarning("Object with ID" + body.ID.ToString() + "cannot be dessimilated because it it not assimilated");
				return;
			}

			SimObjects[tid] = null;
			CachedIDs.Add(tid);


			if (body.DynamicID >= 0)
			{
				DynamicSimObjects.RemoveAt(body.DynamicID);
				body.DynamicID = -1;
			}
		}
		/// <summary>
		/// Takes away some safety checks
		/// </summary>

		internal static CollisionPair GetCollisionPairRaw(int ID1, int ID2)
		{
			LSBody body1;
			LSBody body2;
			if ((body1 = SimObjects[ID1]).IsNotNull() && (body2 = SimObjects[ID2]).IsNotNull())
			{
				if (body1.ID < body2.ID)
				{
				}
				else
				{
					var temp = body1;
					body1 = body2;
					body2 = temp;
				}

				CollisionPair pair;
				if (!body1.CollisionPairs.TryGetValue(body2.ID, out pair))
				{
					pair = CreatePair(body1, body2);
					body1.CollisionPairs.Add(body2.ID, pair);
					body2.CollisionPairHolders.Add(body1.ID);
				}
				return pair;
			}
			return null;
		}
		public static CollisionPair GetCollisionPair(int ID1, int ID2)
		{
			LSBody body1;
			LSBody body2;
			if ((body1 = SimObjects[ID1]).IsNotNull() && (body2 = SimObjects[ID2]).IsNotNull())
			{
				if (body1.ID < body2.ID)
				{

				}
				else
				{
					var temp = body1;
					body1 = body2;
					body2 = temp;
				}

				if (!RequireCollisionPair(body1, body2))
					return null;

				CollisionPair pair;
				if (!body1.CollisionPairs.TryGetValue(body2.ID, out pair))
				{
					pair = CreatePair(body1, body2);
					body1.CollisionPairs.Add(body2.ID, pair);
					body2.CollisionPairHolders.Add(body1.ID);
				}
				return pair;
			}
			return null;
		}

		public static int GetCollisionPairIndex(int ID1, int ID2)
		{
			if (ID1 < ID2)
			{
				return ID1 * MaxSimObjects + ID2;
			}
			else
			{
				return ID2 * MaxSimObjects + ID1;
			}
		}



		public static bool RequireCollisionPair(LSBody body1, LSBody body2)
		{
			if (
				Physics2D.GetIgnoreLayerCollision(body1.Layer, body2.Layer) == false &&
				(!body1.Immovable || !body2.Immovable) &&
				(!body1.IsTrigger || !body2.IsTrigger) &&
				(body1.Shape != ColliderType.None && body2.Shape != ColliderType.None))
			{
				return true;
			}
			return false;
		}

	}
}