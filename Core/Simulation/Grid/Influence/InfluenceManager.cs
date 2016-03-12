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

        public static LSAgent FindClosestAgent(Vector2d position, IEnumerable<LSAgent> agents)
        {
            long sourceX = position.x;
            long sourceY = position.y;
            LSAgent closestAgent = null;
            long closestDistance = 0;
            int foundBuffer = FoundScanBuffer;
            foreach (LSAgent agent in agents)
            {
                if (FoundScanBuffer == 0)
                    break;
                if (closestAgent != null)
                {
                    long tempDistance = agent.Body._position.FastDistance(sourceX, sourceY);
                    if (tempDistance < closestDistance)
                    {
                        closestAgent = agent;
                        closestDistance = tempDistance;
                        foundBuffer = FoundScanBuffer;
                    } else
                    {
                        foundBuffer--;
                    }
                } else
                {
                    closestAgent = agent;
                    closestDistance = agent.Body._position.FastDistance(sourceX, sourceY);
                }
            }
            return closestAgent;
        }

        public static LSAgent Scan(Vector2d position, long radius, Func<LSAgent,bool> agentConditional, Func<byte,bool> bucketConditional)
        {

            return FindClosestAgent(position, ScanAll(position, radius, agentConditional, bucketConditional));
        }

        public static IEnumerable<LSAgent> ScanAll(Vector2d position, long radius, Func<LSAgent,bool> agentConditional, Func<byte,bool> bucketConditional)
        {
            int xMin = ((position.x - radius - GridManager.OffsetX) / (long)GridManager.ScanResolution).ToInt();
            int xMax = ((position.x + radius - GridManager.OffsetX) / (long)GridManager.ScanResolution).CeilToInt();
            int yMin = ((position.y - radius - GridManager.OffsetY) / (long)GridManager.ScanResolution).ToInt();
            int yMax = ((position.y + radius - GridManager.OffsetY) / (long)GridManager.ScanResolution).CeilToInt();

            long fastRadius = radius * radius;
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    ScanNode tempNode = GridManager.GetScanNode(
                                            x,
                                            y);

                    if (tempNode.IsNotNull())
                    {
                        foreach (FastBucket<LSInfluencer> tempBucket in tempNode.BucketsWithAllegiance(bucketConditional))
                        {
                            BitArray arrayAllocation = tempBucket.arrayAllocation;
                            for (int j = 0; j < tempBucket.PeakCount; j++)
                            {
                                if (arrayAllocation.Get(j))
                                {
                                    LSAgent tempAgent = tempBucket [j].Agent;

                                    long distance = (tempAgent.Body.Position - position).FastMagnitude();
                                    if (distance < fastRadius)
                                    {

                                        if (agentConditional(tempAgent))
                                        {
                                            yield return tempAgent;
                                        }
                                    }
                                    else {
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        #endregion
    }
}