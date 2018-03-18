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

		public AgentControllerDataItem (string name) {
			_name = name;
		}
	}
}
