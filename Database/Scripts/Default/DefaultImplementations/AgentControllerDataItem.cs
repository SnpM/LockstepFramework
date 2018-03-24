using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Lockstep.Data
{
	[System.Serializable]
	public class AgentControllerDataItem : DataItem {
		[SerializeField]
		protected AllegianceType _defaultAllegiance;
		public AllegianceType DefaultAllegiance { get { return _defaultAllegiance; } }

		[SerializeField]
		protected bool _playerManaged;
		public bool PlayerManaged { get { return _playerManaged; } }

		[SerializeField,DataCodeAttribute ("Agents")]
		private string _commanderCode;

		public string CommanderCode { get { return _commanderCode; } }


		public AgentControllerDataItem (string name) {
			_name = name;
		}
	}
}
