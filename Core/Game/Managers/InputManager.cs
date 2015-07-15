using UnityEngine;
using System.Collections;
using System;
namespace Lockstep {
	public static class InputManager {
		public const int InputCount = 32;
	}

	public enum InputCode : byte {
		Q,W,E,R,
		A,S,D,F,
		Z,X,C,V,
		M
	}
}
