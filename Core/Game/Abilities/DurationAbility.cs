using UnityEngine;
using System.Collections; using FastCollections;
namespace Lockstep
{
	public class DurationAbility : ActiveAbility
	{
		[SerializeField, FrameCount]
		private int _duration;

		public virtual int Duration
		{
			get
			{
				return _duration;
			}
		}

		[Lockstep(true)]
		private int Timer { get; set; }
		public bool IsWorking { get { return Timer > 0; } }
		protected override void OnExecute(Command com)
		{
			Timer = Duration;
			OnStartWorking();
			if (!IsWorking)
			{
				OnWorking();
				OnStopWorking();
			}
		}

		protected override void OnSimulate()
		{
			if (IsWorking)
			{
				OnWorking();
				Timer--;
				if (!IsWorking)
				{
					OnStopWorking();
				}
			}
		}

		protected virtual void OnStartWorking()
		{

		}
		protected virtual void OnWorking()
		{

		}
		protected virtual void OnStopWorking()
		{

		}

	}
}