using UnityEngine;

namespace Lockstep
{
	public abstract class EnvironmentSaver : MonoBehaviour
	{
		public void Save()
		{
			OnSave();

		}
		protected virtual void OnSave()
		{

		}
		public void EarlyApply()
		{
			OnEarlyApply();
		}
		protected virtual void OnEarlyApply()
		{

		}
		public void Apply()
		{
			OnApply();
		}
		protected virtual void OnApply()
		{

		}
		public void LateApply()
		{
			this.OnLateApply();
		}
		protected virtual void OnLateApply()
		{

		}
	}
}