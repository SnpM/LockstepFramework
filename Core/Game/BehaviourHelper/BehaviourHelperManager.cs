
namespace Lockstep
{
	public static class BehaviourHelperManager
	{
		private static ILockstepEventsHandler[] Helpers { get; set; }

		public static void Initialize(ILockstepEventsHandler[] helpers)
		{
			Helpers = helpers;
			foreach (ILockstepEventsHandler helper in Helpers)
			{
				helper.EarlyInitialize();
			}

		}

		public static void LateInitialize()
		{
			foreach (ILockstepEventsHandler helper in Helpers)
			{
				helper.Initialize();
			}
			foreach (ILockstepEventsHandler helper in Helpers)
			{
				helper.LateInitialize();
			}
		}

		public static void GameStart()
		{
			foreach (var helper in Helpers)
			{
				helper.GameStart();
			}
		}

		public static void Simulate()
		{
			foreach (ILockstepEventsHandler helper in Helpers)
			{
				helper.Simulate();
			}
		}

		public static void LateSimulate()
		{
			foreach (ILockstepEventsHandler helper in Helpers)
			{
				helper.LateSimulate();
			}
		}

		public static void Execute(Command com)
		{
			foreach (ILockstepEventsHandler helper in Helpers)
			{
				if (helper.GetListenInput() == com.InputCode)
				{
					helper.GlobalExecute(com);
				}
				helper.RawExecute(com);
			}
		}

		public static void Visualize()
		{
			foreach (ILockstepEventsHandler helper in Helpers)
			{
				helper.Visualize();
			}
		}

		public static void LateVisualize()
		{
			foreach (var helper in Helpers)
			{
				helper.LateVisualize();
			}
		}

		public static void Deactivate()
		{
			foreach (ILockstepEventsHandler helper in Helpers)
			{
				helper.Deactivate();
			}
		}
	}
}