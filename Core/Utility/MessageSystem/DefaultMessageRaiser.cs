namespace Lockstep
{
	public static class DefaultMessageRaiser
	{
		private static bool _do = false;
		public static bool Do { get { return _do; } }

		static readonly DefaultMessage EmptyMessage = new DefaultMessage();

		public static void EarlySetup()
		{

			if (Do)
				MessageManager.Instance.Invoke<DefaultMessage>(EmptyMessage, "EarlySetup");
		}
		public static void LateSetup()
		{
			if (Do)
				MessageManager.Instance.Invoke<DefaultMessage>(EmptyMessage, "LateSetup");
		}

		public static void EarlyInitialize()
		{
			if (Do)
				Lockstep.MessageManager.Instance.Invoke<DefaultMessage>(EmptyMessage, "EarlyInitialize");
		}

		public static void LateInitialize()
		{
			if (Do)
			{
				MessageManager.Instance.Invoke<DefaultMessage>(EmptyMessage, "Startup");
				MessageManager.Instance.Invoke<DefaultMessage>(EmptyMessage, "LateInitialize");
			}
		}

		public static void EarlySimulate()
		{
			if (Do)
				Lockstep.MessageManager.Instance.Invoke<DefaultMessage>(EmptyMessage, "EarlySimulate");
		}

		public static void LateSimulate()
		{
			if (Do)
				MessageManager.Instance.Invoke<DefaultMessage>(EmptyMessage, "LateSimulate");
		}

		public static void EarlyVisualize()
		{
			if (Do)
				Lockstep.MessageManager.Instance.Invoke<DefaultMessage>(EmptyMessage, "EarlyVisualize");
		}

		public static void LateVisualize()
		{
			if (Do)
				MessageManager.Instance.Invoke<DefaultMessage>(EmptyMessage, "LateVisualize");

		}

		public static void EarlyDeactivate()
		{
			if (Do)
				MessageManager.Instance.Invoke<DefaultMessage>(EmptyMessage, "EarlyDeactivate");
		}

		public static void LateDeactivate()
		{
			if (Do)
				MessageManager.Instance.Invoke<DefaultMessage>(EmptyMessage, "LateDeactivate");

		}

		public static void Execute(Command com)
		{
			if (Do)
				MessageManager.Instance.Invoke<Command>(com, "Execute");
		}

		public static void Reset()
		{
			MessageManager.Instance.Reset();
		}
	}
}