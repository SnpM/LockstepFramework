using UnityEngine;
using System.Collections;
using System;
namespace Lockstep {
public class PartitionNode {
		
		public readonly FastBucket<int> ContainedObjects = new FastBucket<int> ();
		public int Count {get {return ContainedObjects.Count;}}
		public int PeakCount {get {return ContainedObjects.PeakCount;}}
		public void Reset () { 
			ContainedObjects.FastClear ();
		}
		int activationID;
		public void Add(int item)
		{
			if (Count == 0) {
				activationID = Partition.ActivatedNodes.Add (this);
			}
			ContainedObjects.Add (item);
		}
		public void Remove(int item) {
			if (ContainedObjects.Remove (item)) {
				if (Count == 0) {
					Partition.ActivatedNodes.RemoveAt (activationID);
                    activationID = -1;
				}
			}
		}

		static int id1, id2;
		static CollisionPair pair;
		public void Distribute () {
			int nodePeakCount = PeakCount;
			for (int j = 0; j < nodePeakCount; j++) {
				if (ContainedObjects.arrayAllocation[j])
				{
                    id1 = ContainedObjects[j];
					for (int k = j + 1; k < nodePeakCount; k++) {
						if (ContainedObjects.arrayAllocation[k]) {
                            id2 = ContainedObjects [k];
                            pair = PhysicsManager.GetCollisionPair(id1,id2);
							if (System.Object.ReferenceEquals (null, pair) == false && (pair.PartitionVersion != Partition._Version)) {
								pair.CheckAndDistributeCollision ();
								pair.PartitionVersion = Partition._Version;
							}
						}
					}
				}
			}
		}

		public int this[int index]
		{
			get
			{
				return ContainedObjects[index];
			}
			set
			{
				ContainedObjects[index] = value;
			}
		}

	}
}