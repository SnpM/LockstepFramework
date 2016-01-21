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
					}
					else {
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

		public static LSAgent Scan(Vector2d position, int deltaCount, Func<LSAgent,bool> agentConditional, Func<byte,bool> bucketConditional)
		{
            int gridX;
            int gridY;
            GridManager.GetCoordinates(position.x,position.y, out gridX, out gridY);
			return FindClosestAgent(position, ScanAll (gridX, gridY, deltaCount, agentConditional, bucketConditional));
		}

        public static IEnumerable<LSAgent> ScanAll(int gridX, int gridY, int deltaCount, Func<LSAgent,bool> agentConditional, Func<byte,bool> bucketConditional)
        {
            for (int i = 0; i < deltaCount; i++)
            {
                ScanNode tempNode = GridManager.GetScanNode(
                    gridX + DeltaCache.CacheX [i],
                    gridY + DeltaCache.CacheY [i]);

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
                                if (agentConditional(tempAgent))
                                {
                                    yield return tempAgent;
                                }
                            }
                        }
                    }
                }
            }
        }

		public static IEnumerable<LSAgent> ScanAllCone(int gridX, int gridY, long radius, long angle,
			LSAgent sourceAgent,
			AllegianceType targetAllegiance)
		{
			Vector2d center = sourceAgent.Body._position;
			Vector2d rotation = sourceAgent.Body._rotation;

			int deltaCount = InfluenceManager.GenerateDeltaCount (radius);
			long num = radius * radius;
			long num2 = angle * angle >> 16;

			for (int i = 0; i < deltaCount; i++)
			{
				ScanNode tempNode = GridManager.GetScanNode(
					gridX + DeltaCache.CacheX [i],
					gridY + DeltaCache.CacheY [i]);

				if (tempNode.IsNotNull())
				{
                    foreach (FastBucket<LSInfluencer> tempBucket in tempNode.BucketsWithAllegiance(
                        (bite) => ((sourceAgent.Controller.GetAllegiance(bite) & targetAllegiance) != 0))
                    )
					{
						BitArray arrayAllocation = tempBucket.arrayAllocation;
						for (int j = 0; j < tempBucket.PeakCount; j++)
						{
							if (arrayAllocation.Get(j))
							{
								LSAgent tempAgent = tempBucket [j].Agent;

								Vector2d agentPos = tempAgent.Body._position;
								Vector2d difference = agentPos - center;

								long num3 = difference.FastMagnitude ();
								if (num3 <= num && difference.Dot (rotation.x, rotation.y) > 0L)
								{
									num3 >>= 16;
									long num4 = rotation.Cross (difference.x, difference.y);
									num4 *= num4;
									num4 >>= 16;
									if (num4 < num2 * num3 >> 16)
									{
										yield return tempAgent;
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