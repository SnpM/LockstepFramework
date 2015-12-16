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

        static bool agentFound;
        static LSAgent tempAgent;
        static LSAgent closestAgent;
        static long closestDistance;
        static long tempDistance;
        static LSAgent secondClosest;
        static ScanNode tempNode;
        static FastBucket<LSAgent> tempBucket;
        static BitArray arrayAllocation;

        public static LSAgent Scan(int gridX, int gridY, int deltaCount,
                              Func<LSAgent,bool> conditional, long sourceX, long sourceY)
        {
            agentFound = false;
            for (int i = 0; i < deltaCount; i++)
            {
                tempNode = GridManager.GetScanNode(
                    gridX + DeltaCache.CacheX [i],
                    gridY + DeltaCache.CacheY [i]);
				
                if (tempNode.IsNotNull() && tempNode.LocatedAgents.IsNotNull())
                {
                    tempBucket = tempNode.LocatedAgents;
                    arrayAllocation = tempBucket.arrayAllocation;
                    for (int j = 0; j < tempBucket.PeakCount; j++)
                    {
                        if (arrayAllocation.Get(j))
                        {
                            tempAgent = tempBucket [j];
                            if (conditional(tempAgent))
                            {
                                if (agentFound)
                                {
                                    if ((tempDistance = tempAgent.Body.Position.FastDistance(sourceX, sourceY)) < closestDistance)
                                    {
                                        secondClosest = closestAgent;
                                        closestAgent = tempAgent;
                                        closestDistance = tempDistance;
                                    }
                                } else
                                {
                                    closestAgent = tempAgent;
                                    agentFound = true;
                                    closestDistance = tempAgent.Body.Position.FastDistance(sourceX, sourceY);
                                }
                            }
                        }
                    }
                    if (agentFound)
                    {
                        return closestAgent;
                    }
                }
            }
            return null;
        }

        public static void ScanAll(int gridX, int gridY, int deltaCount, FastList<LSAgent> outputAgents,
                              Func<LSAgent,bool> conditional)
        {
            outputAgents.FastClear();
            for (int i = 0; i < deltaCount; i++)
            {
                tempNode = GridManager.GetScanNode(
                    gridX + DeltaCache.CacheX [i],
                    gridY + DeltaCache.CacheY [i]);
				
                if (tempNode.IsNotNull() && tempNode.LocatedAgents.IsNotNull())
                {
                    tempBucket = tempNode.LocatedAgents;
                    arrayAllocation = tempBucket.arrayAllocation;
                    for (int j = 0; j < tempBucket.PeakCount; j++)
                    {
                        if (arrayAllocation.Get(j))
                        {
                            tempAgent = tempBucket [j];
                            if (conditional(tempAgent))
                            {
                                outputAgents.Add(tempAgent);
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