using System;
namespace Lockstep
{
	[Serializable]
	public sealed class Replay
	{
		public byte[] Content;
		public long hash;
		public string Name;
		public string Date;
		public float Seconds {
			get { return (float)FrameManager.LoadedFrames / (float)LockstepManager.FrameRate; }
		}

		public string SerializeAsString()
		{
			String base64String = Convert.ToBase64String(Content);
			return base64String;
		}

		public void DeserializeString(string data)
		{
			Content = Convert.FromBase64String(data);
		}
	}
}