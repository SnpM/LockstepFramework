using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastCollections;
using Lockstep;
namespace Lockstep.Extras {
	/// <summary>
	/// Managing bodies that don't affect simulation but can draw values (i.e. collisions) off simulation. 
	/// This is indeterministic and for visual/gameplay features.
	/// </summary>
	public class GhostPhysicsManager : BehaviourHelper {
		public static GhostPhysicsManager Instance {get; private set;}
		FastList<GhostLSBody> GhostBodies;
		protected override void OnInitialize ()
		{
			Instance = this;
			GhostBodies = new FastList<GhostLSBody> ();
		}
		protected override void OnSimulate ()
		{
			for (int i = 0; i < GhostBodies.Count; i++) {
				var ghost = GhostBodies [i];
				GhostSimulateBody (ghost);
				CheckCollisions (ghost);
			}
			bufferPartitionNodes.Clear ();

		}
		protected override void OnVisualize ()
		{
			for (int i = 0; i < GhostBodies.Count; i++) {
				var ghost = GhostBodies [i];
				ghost.SetVisuals ();
			}
		}

		void GhostSimulateBody (GhostLSBody ghost) {
			//Only simulate stuff that doesn't affect lockstep simulation
			ghost._SimVelocity ();
			ghost.BuildChangedValues();
			ghost._SimVisualsCounter ();
		}

		FastList<PartitionNode> bufferPartitionNodes = new FastList<PartitionNode>();
		void CheckCollisions (GhostLSBody ghost) {
			
			bufferPartitionNodes.FastClear ();
			Partition.GetTouchingPartitions (ghost, bufferPartitionNodes);

			//temporarily make collisions only affect body1 (the ghost)
			CollisionPair.OnlyAffectBody1 = true;
			for (int n = 0; n < bufferPartitionNodes.Count; n++) {
				var node = bufferPartitionNodes [n];
				for (int i = 0; i < node.ContainedImmovableObjects.Count; i++) {
					var id = node.ContainedImmovableObjects [i];
					ProcessPair (ghost, id);
				}
				for (int k = 0; k < node.ContainedImmovableObjects.Count; k++) {
					var id = node.ContainedImmovableObjects [k];
					ProcessPair (ghost, id);
				}
			}
			CollisionPair.OnlyAffectBody1 = false;

		}
		void ProcessPair (GhostLSBody ghost, int ID2) {
			var pair = GetCollisionPair (ghost, ID2);
			if (pair == null)
				return;
			//CollisionPair.OnlyAffectBody1 should solve simulation-polluting
			pair.CheckAndDistributeCollision ();
			pair.NotifyBody1 ();

		}

		private static CollisionPair GetCollisionPair (GhostLSBody ghost, int ID2) {
			LSBody body2;
			if ((body2 = PhysicsManager.SimObjects[ID2]).IsNotNull()) {
				CollisionPair pair;
				if (!ghost.CollisionPairs.TryGetValue(body2.ID, out pair)) {
					pair = new CollisionPair ();
					ghost.CollisionPairs.Add(body2.ID, pair);
					pair.Initialize (ghost, body2);
					//Don't modify simulation object
					//body2.CollisionPairHolders.Add(body1.ID);
				}
				return pair;
			}
			return null;
		}
		public void Assimilate (GhostLSBody ghostBody) {
			GhostBodies.Add (ghostBody);
		}

	}
}