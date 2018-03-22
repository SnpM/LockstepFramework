using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lockstep;
namespace Lockstep.Experimental {
	public class ResourcePanel : MonoBehaviour {
		public ResourceElement ElementTemplate;
		ResourceController Controller {get; set;}

		public void RegisterController (ResourceController controller) {
			Controller = controller;
			for (int i = 0; i < controller.ResourceContainers.Count; i++) {
				var container = controller.ResourceContainers[i];
				var element = GameObject.Instantiate<ResourceElement> (ElementTemplate);
				element.LinkResource (container);
				element.transform.parent = this.transform;
				element.transform.localScale = Vector3.one;
			}
		}
	}
}