using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;

namespace Lockstep.Data
{
	[System.Serializable]
	public class Stat
	{
		public Stat ()
		{

		}
		public Stat (string configElement, long value)
		{
			_configElement = configElement;
			_value = value;
		}
		[SerializeField, DataCode ("UnitConfigElements")]
		private string _configElement;
		public string ConfigElement {
			get {
				return _configElement;
			}
		}
		[SerializeField, FixedNumber]
		private long _value;
		public long Value {
			get {
				return _value;
			}
		}
	}
}