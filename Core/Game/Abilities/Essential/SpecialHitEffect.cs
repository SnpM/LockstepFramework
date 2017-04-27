using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;
namespace Lockstep
{
	public class SpecialHitEffect : DurationAbility
	{
		protected Scan cachedScan { get; private set;}

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
		protected virtual void ApplyEffect(LSAgent agent)
		{
			OnApplyEffect(agent);
		}
		protected virtual void OnApplyEffect (LSAgent agent)
		{
		}

		protected override void OnStopWorking()
		{
			cachedScan.ExtraOnHit -= ApplyEffect;
		}
	}
}