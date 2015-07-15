using UnityEngine;
using System.Collections;
using System;
namespace Lockstep {
	public static class InputManager {
		public const int InputCount = 13;
		private static ulong PressedInputs;
		private static ulong PressedDownInputs;
		private static ulong PressedUpInputs;
		public static FastList<InputPair> inputPairs = new FastList<InputPair> ();

		public static void Initialize ()
		{
			PressedInputs = 0;
			PressedDownInputs = 0;
			PressedUpInputs = 0;
			inputPairs.FastClear ();

			inputPairs.Add (new InputPair (InputCode.Q, KeyCode.Q));
			inputPairs.Add (new InputPair (InputCode.W, KeyCode.W));
			inputPairs.Add (new InputPair (InputCode.E, KeyCode.E));
			inputPairs.Add (new InputPair (InputCode.R, KeyCode.R));
			inputPairs.Add (new InputPair (InputCode.A, KeyCode.A));
			inputPairs.Add (new InputPair (InputCode.S, KeyCode.S));
			inputPairs.Add (new InputPair (InputCode.D, KeyCode.D));
			inputPairs.Add (new InputPair (InputCode.F, KeyCode.F));
			inputPairs.Add (new InputPair (InputCode.Z, KeyCode.Z));
			inputPairs.Add (new InputPair (InputCode.X, KeyCode.X));
			inputPairs.Add (new InputPair (InputCode.C, KeyCode.C));
			inputPairs.Add (new InputPair (InputCode.V, KeyCode.V));
		}
		public static void FrameReset ()
		{
			PressedDownInputs = 0;
			PressedUpInputs = 0;

		}

		public static void Update ()
		{
			for (i = 0; i < InputCount; i++)
			{
				castedInputCode = (ulong) 1 << (int)inputPairs[i].inputCode;
				if ((PressedInputs & castedInputCode) == castedInputCode)
				{
					if (Input.GetKeyUp (inputPairs[i].keyCode))
					{
						PressedUpInputs |= castedInputCode;
						PressedDownInputs ^= castedInputCode;
						PressedInputs ^= castedInputCode;
					}
				}
				else {
					if (Input.GetKeyDown(inputPairs[i].keyCode))
					{
						PressedDownInputs |= castedInputCode;
						PressedUpInputs ^= castedInputCode;
						PressedInputs |= castedInputCode;
					}
				}
			}
		}

		public static bool GetInput (InputCode inputCode)
		{
			castedInputCode = (ulong) 1 << (int) inputCode;
			return ((PressedInputs & castedInputCode) == castedInputCode);
		}
		public static bool GetInputDown (InputCode inputCode)
		{
			castedInputCode = (ulong) 1 << (int)inputCode;
			return ((PressedDownInputs & castedInputCode) == castedInputCode);
		}
		public static bool GetInputUp (InputCode inputcode)
		{
			castedInputCode = (ulong) 1 << (int)inputcode;
			return ((PressedUpInputs & castedInputCode) == castedInputCode);
		}
		public static void PressInputDown (InputCode inputCode)
		{
			castedInputCode = (ulong) 1 << (int) inputCode;
			PressedDownInputs |= castedInputCode;
			PressedInputs |= castedInputCode;
		}
		public static void PressInputUp (InputCode inputCode)
		{
			castedInputCode = (ulong) 1 << (int)inputCode;
			PressedUpInputs |= castedInputCode;
			PressedInputs ^= castedInputCode;
		}

		static int i,j;
		static ulong castedInputCode;
	}

	public struct InputPair {
		public InputPair (InputCode input, KeyCode key)
		{
			inputCode = input;
			keyCode = key;
		}
		public InputCode inputCode;
		public KeyCode keyCode;
	}

	public enum InputCode : int {
		Q,W,E,R,
		A,S,D,F,
		Z,X,C,V,
		M
	}
}
