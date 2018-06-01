using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lockstep;
namespace Lockstep.Experimental {
	[System.Serializable]
	public class ResourcePackage {
		[SerializeField,DataCode("Resources")]
		private string _resourceTypeCode;
		public string ResourceTypeCode { get { return _resourceTypeCode; } }

		[SerializeField,FixedNumber]
		private long _amount;
		public long Amount {get {return _amount;}}
	}
}