namespace Lockstep
{
	public class Stop : ActiveAbility
	{
		protected override void OnExecute(Command com)
		{
			Agent.StopCast();
		}
	}
}