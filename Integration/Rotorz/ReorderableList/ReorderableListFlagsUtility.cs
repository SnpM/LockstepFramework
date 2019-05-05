using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Lockstep_Rotorz.ReorderableList
{
	/// <summary>
	/// Predefining flag settings for convenience.
	/// </summary>
	public static class ReorderableListFlagsUtility
	{
		public const ReorderableListFlags DisableAddRemove = ReorderableListFlags.HideAddButton | ReorderableListFlags.HideRemoveButtons;
		public const ReorderableListFlags DefinedItems =
			ReorderableListFlagsUtility.DisableAddRemove |
			ReorderableListFlags.DisableReordering |
			ReorderableListFlags.DisableContextMenu;
	}
}
