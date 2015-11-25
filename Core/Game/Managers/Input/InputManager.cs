using UnityEngine;
using System.Collections;
using System;
using Lockstep.UI;
public class InputManager : MonoBehaviour {
	public static readonly InputCode[] InputCodes = (InputCode[])Enum.GetValues(typeof(InputCode));
	private static readonly UnityEngine.KeyCode[] mappedKeyCodes = new KeyCode[InputCodes.Length];
	private static readonly uint[] cachedPressedDown = new uint[InputCodes.Length];
	private static uint _Version;
	public static void Setup () {
		mappedKeyCodes[0] = KeyCode.Q;
		mappedKeyCodes[1] = KeyCode.W;
		mappedKeyCodes[2] = KeyCode.E;
		mappedKeyCodes[3] = KeyCode.R;
		mappedKeyCodes[4] = KeyCode.A;
		mappedKeyCodes[5] = KeyCode.S;
		mappedKeyCodes[6] = KeyCode.D;
		mappedKeyCodes[7] = KeyCode.F;
		mappedKeyCodes[8] = KeyCode.Z;
		mappedKeyCodes[9] = KeyCode.X;
		mappedKeyCodes[10] = KeyCode.C;
		mappedKeyCodes[11] = KeyCode.V;
	}
	public static bool GetInputDown (InputCode inputCode) {
		byte inputIndex = (byte)inputCode;
		if (cachedPressedDown[inputIndex] == _Version)
		{
			return true;
		}
		bool pressedDown = Input.GetKeyDown (mappedKeyCodes[inputIndex]);
		if (pressedDown)
		{
			cachedPressedDown[inputIndex] = _Version;
			return true;
		}
		return false;
	}
	public static void PressInputDown (InputCode inputCode) {
		byte inputIndex = (byte)inputCode;
		cachedPressedDown[inputIndex] = _Version;
	}

	const int InformationMouse = 0;
	const int QuickMouse = 1;
	public static bool GetInformationDown () {
		return Input.GetMouseButtonDown (InformationMouse);
	}
	public static bool GetQuickDown () {
		return Input.GetMouseButtonDown (QuickMouse);
	}
	public static void Visualize ()
	{
		_Version++;
	}
}
