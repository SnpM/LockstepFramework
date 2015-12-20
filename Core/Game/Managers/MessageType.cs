using UnityEngine;
using System.Collections;

namespace Lockstep {
	public enum MessageType : byte
	{
		Input = 100,
		Frame = 101,
		Init = 102,
        Matchmaking = 103,
        Register = 104,
        Test = 105,
    }
}