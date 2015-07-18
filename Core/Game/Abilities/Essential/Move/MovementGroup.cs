using UnityEngine;
using System.Collections;
using Lockstep;

public class MovementGroup
{

	#region Static Containers
	public static int ActiveGroupCapacity = 512;
	public static MovementGroup[] ActiveGroups = new MovementGroup[ActiveGroupCapacity];
	public static int ActiveGroupPeakCount = 0;
	private static FastStack<int> ActiveGroupOpenSlots = new FastStack<int>(ActiveGroupCapacity);
	public static FastStack<MovementGroup> InactiveGroups = new FastStack<MovementGroup> ();
	public static MovementGroup LastCreatedGroup;

	public static void Simulate ()
	{
		for (i = 0; i < ActiveGroupPeakCount; i++) {
			if (ActiveGroups[i] != null)
			{
				moveGroup = ActiveGroups [i];
				moveGroup.LocalSimulate();
			}
		}
	}

	static MovementGroup moveGroup;
	static int slotIndex;

	public static MovementGroup CreateGroup (Command com)
	{
		moveGroup = InactiveGroups.Count > 0 ? InactiveGroups.Pop () : new MovementGroup();
		slotIndex = ActiveGroupOpenSlots.Count > 0 ? ActiveGroupOpenSlots.Pop () : ActiveGroupPeakCount++;

		if (slotIndex == ActiveGroupCapacity)
		{
			ActiveGroupCapacity *= 2;
			System.Array.Resize (ref ActiveGroups, ActiveGroupCapacity);
		}

		moveGroup.IndexID = slotIndex;
		ActiveGroups[slotIndex] = (moveGroup);
		LastCreatedGroup = moveGroup;
		moveGroup.Initialize (com);
		return moveGroup;
	}


	#endregion;

	const int MinimumGroupSize = 3;
	const long MaximumDistDif = FixedMath.One * 4;
	static int i, j, count, smallIndex, bigIndex, hash;
	public FastList<Move> Movers;
	public int MoversCount;
	public Vector2d GroupDirection;
	public Vector2d GroupPosition;
	public Vector2d Destination;
	public long Radius;
	public int IndexID;

	public void Initialize (Command com)
	{
		Movers = new FastList<Move> (com._select.selectedAgentLocalIDs.Count);
		Destination = com._position;
		CalculatedBehaviors = false;
	}

	public void Add (Move mover)
	{
		if (mover.MyMovementGroup != null)
		{
			mover.MyMovementGroup.Movers.Remove (mover);
		}
		mover.MyMovementGroup = this;
		mover.MyMovementGroupID = IndexID;
		Movers.Add (mover);
		MoversCount++;
	}

	public void Remove (Move mover)
	{
		MoversCount--;
	}

	private bool CalculatedBehaviors;
	public void LocalSimulate ()
	{

		if (!CalculatedBehaviors)
		{
			CalculateAndExecuteBehaviors ();
			CalculatedBehaviors = true;
		}
		if (Movers.Count == 0)
		{
			Deactivate ();
		}
	}

	static long biggestSqrDistance;
	static long currentSqrDistance;
	static long currentDistance;
	static long DistDif;

	public void CalculateAndExecuteBehaviors ()
	{
		if (Movers.Count >= MinimumGroupSize) {
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
					if (DistDif > MaximumDistDif * MoversCount / 128) {
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
				mover.closingDistanceMultiplier = FixedMath.One * 2 / 5;
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

	public void Deactivate ()
	{

		for (i = 0; i < Movers.Count; i++) {
			Move mover = Movers [i];
			mover.MyMovementGroup = null;
			mover.MyMovementGroupID = -1;
		}
		Movers.FastClear ();
		ActiveGroups[this.IndexID] = null;
		ActiveGroupOpenSlots.Add (this.IndexID);
		InactiveGroups.Add (this);
		CalculatedBehaviors = false;
		this.IndexID = -1;

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
