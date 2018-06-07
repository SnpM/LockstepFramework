using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastCollections;
namespace Lockstep.Extras {
	[System.Serializable]
	public class GhostLSBody : LSBody {
		public void InitializeGhost (Vector3d StartPosition, Vector2d StartRotation, bool isDynamic = true)
		{
			InitializeVariables (StartPosition, StartRotation, isDynamic);
			//Ensure this object does not modify the position of any other objects upon collision as it should not affect collision
			this.Priority = int.MinValue;
			GhostPhysicsManager.Instance.Assimilate (this);
		}
	}
}