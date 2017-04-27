using UnityEngine;
using System.Collections; using FastCollections;

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