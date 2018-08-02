using UnityEngine;

namespace Lockstep.Data
{
	[System.Serializable]
	public class UnitConfigDataItem : Lockstep.Data.DataItem, IUnitConfigDataItem
	{
		[SerializeField, DataCode("Agents")]
		private string _target;
		public string Target
		{
			get
			{
				return _target;
			}
		}

		[SerializeField]
		private Stat[] _stats;
		public Stat[] Stats
		{
			get
			{
				return _stats;
			}
		}

		protected override void OnManage()
		{
			this._name = Target;
		}

	}

}