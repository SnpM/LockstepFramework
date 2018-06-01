using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lockstep;
using FastCollections;
using Lockstep.Experimental.UI;
namespace Lockstep.Experimental
{
    public class ResourceController : Ability
    {
		public FastList<ResourceContainer> ResourceContainers;
		private Dictionary<string, ResourceContainer> ResourceMap = new Dictionary<string, ResourceContainer>();
        protected override void OnSetup()
        {
            ResourceContainers = new FastList<ResourceContainer>(ResourceHelper.Instance.ResourceTypes.Count);
            foreach (var resourceType in ResourceHelper.Instance.ResourceTypes)
            {
                ResourceContainer container = new ResourceContainer(resourceType);
                ResourceContainers.Add(container);
				ResourceMap.Add(resourceType.Name,container);
            }
        }
		protected override void OnInitialize ()
		{
			foreach (var container in ResourceContainers) {
				container.Clear ();
			}
			if (true) {
				UIManager.Instance.resourcePanel.RegisterController (this);
			}
		}

		public ResourceContainer GetContainer(string resourceCode)
        {
            ResourceContainer container;

			if (ResourceMap.TryGetValue(resourceCode, out container))
            {
                return container;
            }
            else
            {
                Debug.LogError("Resource type name '" + name + "' is invalid.");
                throw new System.Exception();
            }
        }

        /// <summary>
        /// Modify the value of the resource. Can be used to subtract resources by adding a negative value.
        /// </summary>f
        /// <param name="resourceType">Identifies the resource type</param>
        /// <param name="amount">How much of the resource should be added</param>
		public void AddResource(string resourceType, long amount)
        {
			GetContainer(resourceType).Add(amount);
        }
		public void AddPackage (ResourcePackage package) {
			AddResource (package.ResourceTypeCode, package.Amount);
		}
		public void AddPackages (ResourcePackage[] packages) {
			foreach (var package in packages)
				AddPackage (package);
		}
    }
}