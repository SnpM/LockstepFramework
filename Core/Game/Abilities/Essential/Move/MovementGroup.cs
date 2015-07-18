using UnityEngine;
using System.Collections;
using Lockstep;

public class MovementGroup
{

	#region Static Containers
	public static FastList<MovementGroup> ActiveGroups = new FastList<MovementGroup> ();
	public static FastStack<MovementGroup> InactiveGroups = new FastStack<MovementGroup> ();
	public static MovementGroup LastCreatedGroup;

	public static void Simulate ()
	{
		for (i = 0; i < ActiveGroups.Count; i++) {
			moveGroup = ActiveGroups [i];
			moveGroup.CalculateAndExecuteBehaviors ();
			InactiveGroups.Add (moveGroup);
		}
		ActiveGroups.FastClear ();

	}

	static MovementGroup moveGroup;

	public static MovementGroup CreateGroup (Command com)
	{
		if (InactiveGroups.Count > 0) {
			moveGroup = InactiveGroups.Pop ();
		} else {
			moveGroup = new MovementGroup ();
		}
		moveGroup.IndexID = ActiveGroups.Count;
		ActiveGroups.Add (moveGroup);
		LastCreatedGroup = moveGroup;
		moveGroup.Initialize (com);

		return moveGroup;
	}

	public static void DeactivateGroup (MovementGroup group)
	{
		for (i = 0; i < group.Movers.Count; i++) {
			group.Movers [i].MyMovementGroup = null;
		}
		group.Movers.FastClear ();
		ActiveGroups.RemoveAt (group.IndexID);
		InactiveGroups.Add (group);
		group.IndexID = -1;
	}

	#endregion;

	const int MinimumGroupSize = 3;
	static int i, j, count, smallIndex, bigIndex, hash;
	public FastList<Move> Movers;
	public Vector2d GroupDirection;
	public Vector2d GroupPosition;
	public Vector2d Destination;
	public long Radius;
	public int IndexID;

	public void Initialize (Command com)
	{
		Movers = new FastList<Move> (com._select.selectedAgentLocalIDs.Count);
		Destination = com._position;
	}

	public void Add (Move mover)
	{
		if (mover.MyMovementGroup != null) {
			mover.MyMovementGroup.Remove (mover);
		}
		mover.MyMovementGroup = this;
		mover.MyMovementGroupID = IndexID;
		Movers.Add (mover);
	}

	public void Remove (Move mover)
	{
		mover.MyMovementGroup = null;
		mover.MyMovementGroupID = -1;
		Movers.Remove (mover);
	}

	static long biggestSqrDistance;
	static long currentSqrDistance;
	static long currentDistance;
	static long DistDif;

	public void CalculateAndExecuteBehaviors ()
	{
		if (Movers.Count > 2) {
			for (i = 0; i < Movers.Count; i++) {
				Move mover = Movers [i];
				GroupPosition += mover.Body.Position;
			}

			GroupPosition /= Movers.Count;

			biggestSqrDistance = 0;
			for (i = 0; i < Movers.Count; i++) {
				currentSqrDistance = Movers [i].Body.Position.SqrDistance (GroupPosition.x, GroupPosition.y);
				if (currentSqrDistance > biggestSqrDistance) {
					currentDistance = FixedMath.Sqrt (currentSqrDistance);
					DistDif = currentDistance - Radius;
					if (DistDif > FixedMath.One * 8) {
						ExecuteGroupIndividualMove ();
						return;
					}
					biggestSqrDistance = currentSqrDistance;
					Radius = currentDistance;
				}
			}
			if (GroupPosition.SqrDistance (Destination.x, Destination.y) < (biggestSqrDistance * 5 / 4)) {
				ExecuteGroupIndividualMove ();
				return;
			}


			GroupDirection = Destination - GroupPosition;

			for (i = 0; i < Movers.Count; i++) {
				Move mover = Movers [i];
				mover.Destination = mover.Body.Position + GroupDirection;
				mover.IsFormationMoving = true;
				mover.closingDistanceMultiplier = FixedMath.One / 4;
				mover.StartMove ();
			}

		} else {
			for (i = 0; i < Movers.Count; i++) {
				Move mover = Movers [i];
				mover.closingDistanceMultiplier = FixedMath.One / 4;
				mover.Destination = Destination;
				mover.IsFormationMoving = false;
				mover.StartMove ();
			}
		}
	}

	private void ExecuteGroupIndividualMove ()
	{
		for (i = 0; i < Movers.Count; i++) {
			Move mover = Movers [i];
			mover.closingDistanceMultiplier = FixedMath.One * 3 / 5;
			mover.Destination = Destination;
			mover.IsFormationMoving = false;
			mover.StartMove ();
		}
	}
}
