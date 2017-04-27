using UnityEngine;
using System.Collections; using FastCollections;
using System;

namespace Lockstep
{
	public class PartitionNode
	{

		public readonly FastList<int> ContainedDynamicObjects = new FastList<int> ();
		public readonly FastList<int> ContainedImmovableObjects = new FastList<int> ();

		public int DynamicCount { get { return ContainedDynamicObjects.Count; } }


		public void Reset ()
		{
			ContainedDynamicObjects.FastClear ();
			ContainedImmovableObjects.FastClear ();
		}

		int activationID;

		public void Add (int item)
		{
			if (DynamicCount == 0) {
				activationID = Partition.AddNode (this);
			}
			ContainedDynamicObjects.Add (item);
		}

		public void AddImmovable (int item)
		{
			ContainedImmovableObjects.Add (item);

		}

		public void Remove (int item)
		{
			//todo get rid of this linear search
			if (ContainedDynamicObjects.Remove (item)) {
				if (DynamicCount == 0) {
					Partition.RemoveNode (activationID);
					activationID = -1;
				}
			} else {
				Debug.LogError ("Item not removed");
			}
		}

		public void RemoveImmovable (int item)
		{
			if (ContainedImmovableObjects.Remove (item)) {

			}
		}

		static int id1, id2;
		static CollisionPair pair;

		public void Distribute ()
		{
			int nodePeakCount = DynamicCount;
			int immovableObjectsCount = ContainedImmovableObjects.Count;
			for (int j = 0; j < nodePeakCount; j++) {
				id1 = ContainedDynamicObjects [j];
				for (int k = j + 1; k < nodePeakCount; k++) {
					id2 = ContainedDynamicObjects [k];
					if (id1 != id2) {
						ProcessPair ();
					}
				}
				for (int k = 0; k < immovableObjectsCount; k++) {
					id2 = ContainedImmovableObjects [k];
					ProcessPair ();
				}

			}


		}

		void ProcessPair ()
		{
			Partition.count++;
			pair = PhysicsManager.GetCollisionPair (id1, id2);
			if (pair.IsNotNull ()) {
				if (pair.PartitionVersion != Partition._Version) {
					pair.PartitionVersion = Partition._Version;
					pair.CheckAndDistributeCollision ();
				}
			}

		}


		public int this [int index] {
			get {
				return ContainedDynamicObjects [index];
			}
			set {
				ContainedDynamicObjects [index] = value;
			}
		}

	}
}