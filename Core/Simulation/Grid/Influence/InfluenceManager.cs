using UnityEngine;
using System.Collections; using FastCollections;
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

		public static LSAgent FindClosestAgent(Vector2d position, FastList<LSAgent> agents)
		{
			long sourceX = position.x;
			long sourceY = position.y;
			LSAgent closestAgent = null;
			long closestDistance = 0;
			int foundBuffer = FoundScanBuffer;
			foreach (LSAgent agent in agents)
			{

				if (closestAgent != null)
				{
					long tempDistance = agent.Body._position.FastDistance(sourceX, sourceY);
					if (tempDistance < closestDistance)
					{
						closestAgent = agent;
						closestDistance = tempDistance;
						foundBuffer = FoundScanBuffer;
					}
					else
					{
						foundBuffer--;
					}
				}
				else
				{
					closestAgent = agent;
					closestDistance = agent.Body._position.FastDistance(sourceX, sourceY);
				}
			}
			return closestAgent;
		}

		public static LSAgent Scan(Vector2d position, long radius, Func<LSAgent, bool> agentConditional, Func<byte, bool> bucketConditional)
		{
			ScanAll(position, radius, agentConditional, bucketConditional, bufferAgents);
			return FindClosestAgent(position, bufferAgents);
		}

        static FastList<FastBucket<LSInfluencer>> bufferBuckets = new FastList<FastBucket<LSInfluencer>>();
		public static FastList<LSAgent> bufferAgents = new FastList<LSAgent>();
		public static void ScanAll(Vector2d position, long radius, Func<LSAgent, bool> agentConditional, Func<byte, bool> bucketConditional, FastList<LSAgent> output)
		{
			output.FastClear();
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
						if (tempNode.AgentCount > 0)
						{
                            bufferBuckets.FastClear();
                            tempNode.GetBucketsWithAllegiance(bucketConditional,bufferBuckets);
                            for (int i = 0; i < bufferBuckets.Count; i++)
							{
                                FastBucket<LSInfluencer> tempBucket = bufferBuckets[i];
								BitArray arrayAllocation = tempBucket.arrayAllocation;
								for (int j = 0; j < tempBucket.PeakCount; j++)
								{
									if (arrayAllocation.Get(j))
									{
										LSAgent tempAgent = tempBucket[j].Agent;

										long distance = (tempAgent.Body.Position - position).FastMagnitude();
										if (distance < fastRadius)
										{

											if (agentConditional(tempAgent))
											{
												output.Add(tempAgent);
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

		}

		#endregion
	}
}