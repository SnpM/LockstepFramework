using UnityEngine;

namespace Lockstep.Data
{
	[System.Serializable]
	public class AgentControllerDataItem : DataItem
	{
		[SerializeField] protected AllegianceType _defaultAllegiance;
		public AllegianceType DefaultAllegiance { get { return _defaultAllegiance; } }

		[SerializeField] protected bool _playerManaged;

		[SerializeField, DataCode("Agents")]
		private string _commanderCode;

		public string CommanderCode { get { return _commanderCode; } }

		public bool PlayerManaged { get { return _playerManaged; } }

		public AgentControllerDataItem(string name)
		{
			_name = name;
		}
	}
}
