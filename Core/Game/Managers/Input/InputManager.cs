using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
	public static class InputManager
	{
		public static int InputCount = 0;
		private static ulong PressedInputs;
		private static ulong PressedDownInputs;
		private static ulong PressedUpInputs;
		public static FastList<InputPair> inputPairs = new FastList<InputPair> ();

		public static void Initialize ()
		{
			PressedInputs = 0;
			PressedDownInputs = 0;
			PressedUpInputs = 0;
			InputCount = 0;
			inputPairs.FastClear ();

			AddInput (InputCode.Q, KeyCode.Q);
			AddInput (InputCode.W, KeyCode.W);
			AddInput (InputCode.E, KeyCode.E);
			AddInput (InputCode.R, KeyCode.R);
			AddInput (InputCode.A, KeyCode.A);
			AddInput (InputCode.S, KeyCode.S);
			AddInput (InputCode.D, KeyCode.D);
			AddInput (InputCode.F, KeyCode.F);
			AddInput (InputCode.Z, KeyCode.Z);
			AddInput (InputCode.X, KeyCode.X);
			AddInput (InputCode.C, KeyCode.C);
			AddInput (InputCode.V, KeyCode.V);
			AddInput (InputCode.M, KeyCode.M);
		}

		public static void AddInput (InputCode inputCode, KeyCode keyCode)
		{
			inputPairs.Add (new InputPair (inputCode, keyCode));
			InputCount++;
		}

		public static void Simulate ()
		{
			PressedDownInputs = 0;
			PressedUpInputs = 0;

		}

		public static void Visualize ()
		{
			for (i = 0; i < InputCount; i++) {
				castedInputCode = (ulong)1 << (int)inputPairs [i].inputCode;
				if ((PressedInputs & castedInputCode) == castedInputCode) {
					if (Input.GetKeyUp (inputPairs [i].keyCode)) {
						PressedUpInputs |= castedInputCode;
						PressedDownInputs ^= castedInputCode;
						PressedInputs ^= castedInputCode;
					}
				} else {
					if (Input.GetKeyDown (inputPairs [i].keyCode)) {
						PressedDownInputs |= castedInputCode;
						PressedUpInputs ^= castedInputCode;
						PressedInputs |= castedInputCode;
					}
				}
			}
		}

		public static bool GetInput (InputCode inputCode)
		{
			castedInputCode = (ulong)1 << (int)inputCode;
			return ((PressedInputs & castedInputCode) == castedInputCode);
		}

		public static bool GetInputDown (InputCode inputCode)
		{
			castedInputCode = (ulong)1 << (int)inputCode;
			return ((PressedDownInputs & castedInputCode) == castedInputCode);
		}

		public static bool GetInputUp (InputCode inputcode)
		{
			castedInputCode = (ulong)1 << (int)inputcode;
			return ((PressedUpInputs & castedInputCode) == castedInputCode);
		}

		public static void PressInputDown (InputCode inputCode)
		{
			castedInputCode = (ulong)1 << (int)inputCode;
			PressedDownInputs |= castedInputCode;
			PressedInputs |= castedInputCode;
		}

		public static void PressInputUp (InputCode inputCode)
		{
			castedInputCode = (ulong)1 << (int)inputCode;
			PressedUpInputs |= castedInputCode;
			PressedInputs ^= castedInputCode;
		}

		static int i, j;
		static ulong castedInputCode;
	}

	public struct InputPair
	{
		public InputPair (InputCode input, KeyCode key)
		{
			inputCode = input;
			keyCode = key;
		}

		public InputCode inputCode;
		public KeyCode keyCode;
	}


}
