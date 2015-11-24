using UnityEngine;
using System.Collections;
using System;
[Serializable]
public struct VectorRotation {
	[SerializeField]
	private long _cos;
	[SerializeField]
	private long _sin;
	public long Cos {get {return _cos;}}
	public long Sin {get {return _sin;}}

#if UNITY_EDITOR
	[SerializeField]
	private double _angle;
	[SerializeField]
	private bool _timescaled;
#endif
}
