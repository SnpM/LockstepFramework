namespace Lockstep
{
	public class SpecialHitEffect : DurationAbility
	{
		protected Scan cachedScan { get; private set; }

		protected override void OnSetup()
		{
			cachedScan = Agent.GetAbility<Scan>();
		}
		protected override void OnExecute(Command com)
		{
			base.OnExecute(com);
		}

		protected override void OnStartWorking()
		{
			cachedScan.ExtraOnHit += ApplyEffect;
		}
		protected virtual void ApplyEffect(LSAgent agent, bool isCurrent)
		{
			OnApplyEffect(agent, isCurrent);
		}
		protected virtual void OnApplyEffect(LSAgent agent, bool isCurrent)
		{
		}

		protected override void OnStopWorking()
		{
			cachedScan.ExtraOnHit -= ApplyEffect;
		}
	}
}