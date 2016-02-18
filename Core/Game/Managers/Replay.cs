using System;

namespace Lockstep
{
	[Serializable]
	public sealed class Replay
	{
		public byte[] Content;

		// TODO: Temporary for tests, remove later
		public long hash;
	}
}