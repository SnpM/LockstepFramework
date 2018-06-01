using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lockstep;
using UnityEngine.Serialization;
//TODO: System to automatically input format in database.
//Also implement private backing variables
[System.Serializable]
public class MoveStats {
	[SerializeField,FixedNumber]
	private long _speed = FixedMath.One * 4;
	public long Speed {get { return _speed; } set {_speed = value;}}

	[SerializeField,FixedNumber]
	private long _acceleration = FixedMath.One * 8;
	public long Acceleration {
		get {
			return _acceleration;
		}
		set {
			_acceleration = value;
		}
	}

	[SerializeField]
	private bool _canMove = true;
	public bool CanMove {
		get {
			return _canMove;
		}
		set {
			_canMove = value;
		}
	}
		
	[SerializeField]
	private bool _canTurn = true;
	public bool CanTurn {
		get {
			return _canTurn;
		}
		set {
			_canTurn = value;
		}
	}
}
