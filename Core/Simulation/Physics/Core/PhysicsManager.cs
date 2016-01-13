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

            
        static double FixedDeltaTime
        {
            get
            {
                return 1d / LockstepManager.FrameRate;
            }
        }

        static int VisualSetSpread {
            get {
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

        public const int MaxSimObjects = 5000;
        public static LSBody[] SimObjects = new LSBody[MaxSimObjects];
        private static Dictionary<int,CollisionPair> CollisionPairs = new Dictionary<int,CollisionPair>(MaxSimObjects);
        public static FastList<CollisionPair> FastCollisionPairs = new FastList<CollisionPair>(MaxSimObjects);

        #endregion

        #region Assignment Variables

        public static bool[] SimObjectExists = new bool[MaxSimObjects];
        public static int PeakCount = 0;
        private static FastStack<int> CachedIDs = new FastStack<int>(MaxSimObjects / 8);
        public static int AssimilatedCount = 0;

        #endregion

        #region Visualization

        #endregion

        public static void Setup()
        {
            Partition.Setup();
        }


        public static void Initialize()
        {
            PeakCount = 0;

            CachedIDs.FastClear();
            SimObjectExists.Clear();
            //CollisionPairs.Clear ();
            //SimObjects.Clear ();
            CollisionPair.CurrentCollisionPair = null;

            PeakCount = 0;
            AssimilatedCount = 0;

            FastCollisionPairs.FastClear();

            Partition.Initialize();

            if (SettingsChanged)
            {
                SettingsChanged = false;
            }

            AccumulatedTime = 0;
            LastTime = 0;
        }


        public static void Simulate()
        {
            for (int i = 0; i < PeakCount; i++)
            {
                if (SimObjectExists [i])
                {

                    LSBody b1 = SimObjects [i];
                    b1.EarlySimulate();
                }
            }
            Partition.CheckAndDistributeCollisions();
            for (int i = 0; i < PeakCount; i++)
            {
                if (SimObjectExists [i])
                {
                    LSBody b1 = SimObjects [i];
                    b1.Simulate();
                }
            }
            Simulated = true;
        }

        public static void LateSimulate()
        {
			
			

            for (int i = 0; i < PeakCount; i++)
            {
                if (SimObjectExists [i])
                {
                    SimObjects [i].LateSimulate();
                }
            }

        }

		public static void Deactivate()
		{
			FastCollisionPairs.Clear();
			CollisionPairs.Clear();
			Partition.Deactivate();
		}

        public static float LerpTime { get ; private set; }
        public static float ExtrapolationAmount {get; private set;}
        public static float LerpDamping { get; private set; }

        private static float LerpDampScaler;

        private static double LastTime { get; set; }

        public static double AccumulatedTime { get; private set; }

        public static bool Simulated { get; private set; }

        public static void Visualize()
        {
            LerpDamping = 1f;
            double curTime = LockstepManager.Seconds;
            AccumulatedTime += (curTime - LastTime) * Time.timeScale;
            LerpTime = (float)(AccumulatedTime / FixedDeltaTime);
            if (LerpTime < 1f)
            {
                for (int i = 0; i < PeakCount; i++)
                {
                    if (SimObjectExists [i])
                    {
                        LSBody b1 = SimObjects [i];
                        b1.Visualize();
                    }
                }
            } else
            {
                AccumulatedTime %= FixedDeltaTime;
                ExtrapolationAmount = LerpTime;
                LerpTime = (float)(AccumulatedTime / FixedDeltaTime);

                if (Simulated)
                {
                    for (int i = 0; i < PeakCount; i++)
                    {
                        if (SimObjectExists [i])
                        {
                            LSBody b1 = SimObjects [i];
                            b1.LerpOverReset();

                            b1.SetVisuals();

                            b1.Visualize();


                        }
                    }
                    Simulated = false;
                }
                else {
                    for (int i = 0; i < PeakCount; i++) {
                        if (SimObjectExists[i]) {
                            LSBody b1 = SimObjects[i];

                            b1.LerpOverReset();
                            b1.SetExtrapolatedVisuals();

                            b1.Visualize();

                        }
                    }
                }

            }
            LastTime = curTime;
        }

        public static float ElapsedTime;


        static int id;
        static LSBody other;
        internal static int Assimilate(LSBody body)
        {
            if (CachedIDs.Count > 0)
            {
                id = CachedIDs.Pop();
            } else
            {
                id = PeakCount;
                PeakCount++;
            }
            SimObjectExists [id] = true;
            SimObjects [id] = body;

            AssimilatedCount++;
            return id;
        }

        private static CollisionPair CreatePair(LSBody body1, LSBody body2)
        {
            int pairIndex = GetCollisionPairIndex(body1.ID, body2.ID);
            CollisionPair pair = new CollisionPair();
            CollisionPairs [pairIndex] = pair;
            FastCollisionPairs.Add(pair);
            pair.Initialize(body1, body2);
            return pair;

        }

        internal static void Dessimilate(LSBody body)
        {
            if (!SimObjectExists [body.ID])
            {
                Debug.LogWarning("Object with ID" + body.ID.ToString() + "cannot be dessimilated because it it not assimilated");
                return;
            }

            SimObjectExists [body.ID] = false;
            CachedIDs.Add(body.ID);

            for (int i = 0; i < FastCollisionPairs.Count; i++)
            {
                CollisionPair pair = FastCollisionPairs.innerArray [i];
                if (pair.Body1 == body || pair.Body2 == body)
                {
                    pair.Deactivate();
                }
            }

            AssimilatedCount--;
            body.Deactivate();
        }

        public static CollisionPair GetCollisionPair(int ID1, int ID2)
        {
            CollisionPair pair;
            int pairIndex = GetCollisionPairIndex(ID1, ID2);
            if (CollisionPairs.TryGetValue(pairIndex, out pair) == false)
            {
                pair = CreatePair(SimObjects [ID1], SimObjects [ID2]);
            }
            return pair;
        }

        public static int GetCollisionPairIndex(int ID1, int ID2)
        {
            if (ID1 < ID2)
            {
                return ID1 * MaxSimObjects + ID2;
            } else
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