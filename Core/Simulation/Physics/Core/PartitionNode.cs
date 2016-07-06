using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
    public class PartitionNode
    {
		
        public readonly FastList<int> ContainedObjects = new FastList<int>();

        public int Count { get { return ContainedObjects.Count; } }

        public int PeakCount { get { return ContainedObjects.Count; } }

        public void Reset()
        { 
            ContainedObjects.FastClear();
        }

        int activationID;

        public void Add(int item)
        {
            if (Count == 0)
            {
                activationID = Partition.AddNode(this);
            }
            ContainedObjects.Add(item);
        }

        public void Remove(int item)
        {
            
            if (ContainedObjects.Remove(item))
            {
                if (Count == 0)
                {
					Partition.RemoveNode(activationID);
					activationID = -1;
                }
            }
        }

        static int id1, id2;
        static CollisionPair pair;

        public void Distribute()
        {
            int nodePeakCount = PeakCount;
            for (int j = 0; j < nodePeakCount; j++)
            {
                id1 = ContainedObjects [j];
                for (int k = j + 1; k < nodePeakCount; k++)
                {
                    id2 = ContainedObjects [k];

                    if (id1 != id2) {
                        pair = PhysicsManager.GetCollisionPair(id1, id2);
                        if (System.Object.ReferenceEquals(null, pair) == false && (pair.PartitionVersion != Partition._Version))
                        {
                            pair.CheckAndDistributeCollision();
                            pair.PartitionVersion = Partition._Version;
    							
                        }
                    }
                }
				
            }
        }

        public int this [int index]
        {
            get
            {
                return ContainedObjects [index];
            }
            set
            {
                ContainedObjects [index] = value;
            }
        }

    }
}