using UnityEngine;

namespace Lockstep
{
	/// <summary>
	/// Global singleton abilities. Attach to the Manager gameobject or child.
	/// </summary>
	public abstract class BehaviourHelper : MonoBehaviour, ILockstepEventsHandler
	{
		private ushort CachedListenInput;

		public virtual ushort ListenInput
		{
			get { return 0; }
		}

		public ushort GetListenInput()
		{
			return CachedListenInput;
		}

		public void Initialize()
		{
			OnInitialize();
		}

		public void EarlyInitialize()
		{
			CachedListenInput = ListenInput;

			OnEarlyInitialize();
		}

		protected virtual void OnEarlyInitialize()
		{

		}

		protected virtual void OnInitialize()
		{
		}

		public void LateInitialize()
		{
			this.OnLateInitialize();
		}

		protected virtual void OnLateInitialize()
		{
		}

		public void Simulate()
		{
			OnSimulate();
		}

		protected virtual void OnSimulate()
		{
		}

		public void LateSimulate()
		{
			OnLateSimulate();
		}

		protected virtual void OnLateSimulate()
		{
		}

		public void Visualize()
		{
			OnVisualize();
		}

		protected virtual void OnVisualize()
		{
		}

		public void LateVisualize()
		{
			OnLateVisualize();
		}

		protected virtual void OnLateVisualize()
		{

		}

		public void GlobalExecute(Command com)
		{
			OnExecute(com);
		}

		protected virtual void OnExecute(Command com)
		{
		}

		public void RawExecute(Command com)
		{
			OnRawExecute(com);
		}

		protected virtual void OnRawExecute(Command com)
		{

		}

		public void GameStart()
		{
			OnGameStart();
		}

		protected virtual void OnGameStart()
		{

		}

		public void Deactivate()
		{
			OnDeactivate();
		}

		protected virtual void OnDeactivate()
		{
		}
	}
}