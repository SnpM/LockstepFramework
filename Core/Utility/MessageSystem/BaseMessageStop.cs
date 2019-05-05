namespace Lockstep
{
	public abstract class BaseMessageStop
	{
		public abstract BaseMessageChannel GetChannel(string channelID);
		public abstract void Clear();
	}
}