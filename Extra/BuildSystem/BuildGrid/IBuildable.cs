using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;
using Lockstep.Utility;
using Lockstep.Abilities;

namespace Lockstep{
public interface IBuildable
{

	Coordinate GridPosition { get; set; }
	/// <summary>
	/// Describes the width and height of the buildable. This value does not change on the buildable.
	/// </summary>
	/// <value>The size of the build.</value>
	int BuildSize { get; }
	/// <summary>
	/// Function that relays to the buildable whether or not it's on a valid building spot.
	/// </summary>
	bool IsValidOnGrid { get; set; }
	bool IsMoving { get; set; }

}
}