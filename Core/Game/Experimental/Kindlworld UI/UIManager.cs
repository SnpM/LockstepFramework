using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Lockstep.Experimental.UI {
	public class UIManager : MonoBehaviour {
		public static UIManager Instance {get; private set;}
		[SerializeField]
		private ResourcePanel _resourcePanel;
		public ResourcePanel resourcePanel {get {return _resourcePanel;}}
		void Awake () {
			Instance = this;
		}
	}
}
