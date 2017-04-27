using Lockstep;
using System;
using UnityEngine;
using System.Linq;
using FastCollections;

namespace Lockstep
{
    public class DeterminismTester : BehaviourHelper
    {
        public static FastBucket<long> Hashes = new FastBucket<long>();
        bool IsPlayingBack;

        protected override void OnInitialize()
        {
            IsPlayingBack = ReplayManager.IsPlayingBack;
            if (!IsPlayingBack)
                Hashes.FastClear();
        }

        protected override void OnSimulate()
        {
            long hash = LockstepManager.GetStateHash();
            if (IsPlayingBack)
            {

                if (LockstepManager.FrameCount < Hashes.PeakCount &&
                    Hashes.arrayAllocation[LockstepManager.FrameCount])
                {
                    long lastHash = Hashes [LockstepManager.FrameCount];
                    if (lastHash != hash)
                    {
						Debug.Log("Desynced");// frame " + LockstepManager.FrameCount);
                    }
                }
            } else
            {
                Hashes.InsertAt(hash, LockstepManager.FrameCount);
            }
				
        }
    }
}