using UnityEngine;
using System.Collections;

namespace Lockstep {
	public enum MessageType : byte
	{
		Input,
		Frame,
		Init,
        Matchmaking,
        Register,
        Test,
    }
}