using UnityEngine;
using System.Collections;
using Lockstep;
public class MovementGroup {

	#region Static Containers
	public static FastList<MovementGroup> ActiveGroups = new FastList<MovementGroup>();
	public static FastStack<MovementGroup> InactiveGroups = new FastStack<MovementGroup>();
	public static MovementGroup LastCreatedGroup;
	public static void Simulate ()
	{
		for( i = 0; i < ActiveGroups.Count; i++)
		{
			moveGroup = ActiveGroups[i];
			moveGroup.CalculateAndExecuteBehaviors ();
			InactiveGroups.Add (moveGroup);
		}
		ActiveGroups.FastClear ();

	}
	static MovementGroup moveGroup;
	public static MovementGroup CreateGroup (Command com)
	{
		if (InactiveGroups.Count > 0)
		{
			moveGroup = InactiveGroups.Pop ();
		}
		else {
			moveGroup = new MovementGroup();
		}
		moveGroup.IndexID = ActiveGroups.Count;
		moveGroup.Initialize (com);
		ActiveGroups.Add (moveGroup);
		LastCreatedGroup = moveGroup;
		return moveGroup;
	}

	public static void DeactivateGroup (MovementGroup group)
	{
		for (i = 0; i < group.Movers.Count; i++)
		{
			group.Movers[i].MyMovementGroup = null;
		}
		group.Movers.FastClear ();
		ActiveGroups.RemoveAt (group.IndexID);
		InactiveGroups.Add (group);
		group.IndexID = -1;
	}

	#endregion;

	const int MinimumGroupSize = 3;
	static int i,j, count, smallIndex, bigIndex, hash;
	public FastList<Move> Movers;
	public Vector2d GroupDirection;
	public Vector2d GroupPosition;
	public Vector2d Destination;
	public int IndexID;

	public void Initialize (Command com)
	{
		Movers = new FastList<Move>(com._select.selectedAgentLocalIDs.Count);
		Destination = com._position;
	}

	public void Add (Move mover)
	{
		if (mover.MyMovementGroup != null)
		{
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

	public void CalculateAndExecuteBehaviors ()
	{
		if (Movers.Count > 2)
		{
			for (i = 0; i < Movers.Count; i++)
			{
				GroupPosition += Movers[i].Body.Position;
			}
			GroupPosition /= Movers.Count;

			GroupDirection = Destination - GroupPosition;
			GroupDirection.Normalize ();
		}
		else {
			for (i = 0; i < Movers.Count;i++)
			{
				Move mover = Movers[i];
				mover.Destination = Destination;
				mover.StartMove ();
			}
		}
	}
}
