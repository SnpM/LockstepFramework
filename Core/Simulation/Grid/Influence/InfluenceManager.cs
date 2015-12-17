using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Lockstep
{
    public static class InfluenceManager
    {
        public static void Initialize()
        {
            //DeltaCache.GenerateCache ();
        }

        public static void Simulate()
        {

        }

        public static int GenerateDeltaCount(long radius)
        {
            radius /= GridManager.ScanResolution;
            int ret = FixedMath.Mul(FixedMath.Mul(radius, radius), FixedMath.Pi).CeilToInt();
            //if (ret < 5) ret = 5;
            return ret;
        }

        #region Scanning

        const int FoundScanBuffer = 5;

        public static LSAgent Scan(int gridX, int gridY, int deltaCount,
                                   LSAgent sourceAgent, AllegianceType targetAllegiance)
        {
            long sourceX = sourceAgent.Body.Position.x;
            long sourceY = sourceAgent.Body.Position.y;
            LSAgent closestAgent = null;
            long closestDistance = 0;
            int foundBuffer = FoundScanBuffer;
            foreach (LSAgent agent in ScanAll (gridX, gridY, deltaCount, sourceAgent, targetAllegiance))
            {
                if (FoundScanBuffer == 0)
                    break;
                if (closestAgent != null)
                {
                    long tempDistance = agent.Body.Position.FastDistance(sourceX, sourceY);
                    if (tempDistance < closestDistance)
                    {
                        closestAgent = agent;
                        closestDistance = tempDistance;
                        foundBuffer = FoundScanBuffer;
                    }
                    else {
                        foundBuffer--;
                    }
                } else
                {
                    closestAgent = agent;
                    closestDistance = agent.Body.Position.FastDistance(sourceX, sourceY);
                }
            }
            return closestAgent;
        }

        public static IEnumerable<LSAgent> ScanAll(int gridX, int gridY, int deltaCount,
                                                   LSAgent sourceAgent,
                                                   AllegianceType targetAllegiance)
        {
            long sourceX = sourceAgent.Body.Position.x;
            long sourceY = sourceAgent.Body.Position.y;
            for (int i = 0; i < deltaCount; i++)
            {
                ScanNode tempNode = GridManager.GetScanNode(
                    gridX + DeltaCache.CacheX [i],
                    gridY + DeltaCache.CacheY [i]);

                if (tempNode.IsNotNull())
                {
                    foreach (FastBucket<LSInfluencer> tempBucket in tempNode.BucketsWithAllegiance(sourceAgent, targetAllegiance))
                    {
                        BitArray arrayAllocation = tempBucket.arrayAllocation;
                        for (int j = 0; j < tempBucket.PeakCount; j++)
                        {
                            if (arrayAllocation.Get(j))
                            {
                                LSAgent tempAgent = tempBucket [j].Agent;
                                if (true)//conditional(tempAgent))
                                {
                                    yield return tempAgent;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static LSAgent Source;
        public static PlatformType TargetPlatform;
        public static AllegianceType TargetAllegiance;

        public static readonly Func<LSAgent,bool> ScanConditionalSourceWithHealthAction = ScanConditionalSourceWithHealth;

        private static bool ScanConditionalSourceWithHealth(LSAgent agent)
        {
            if (agent.Healther == null)
            {
                return false;
            }
            return ScanConditionalSource(agent);
        }

        public static readonly Func<LSAgent,bool> ScanConditionalSourceAction = ScanConditionalSource;

        private static bool ScanConditionalSource(LSAgent agent)
        {
            if (System.Object.ReferenceEquals(agent, Source))
                return false;
			

            if ((Source.GetAllegiance(agent) & TargetAllegiance) == 0)
            {
                return false;
            }

            if ((agent.Platform & TargetPlatform) == 0)
                return false;
			
            return true;
        }

        #endregion
    }
}