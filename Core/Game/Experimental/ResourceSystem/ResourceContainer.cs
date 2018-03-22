using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lockstep;
using System;
namespace Lockstep.Experimental
{
    public class ResourceContainer
    {
        public ResourceContainer (ResourceType resourceType)
        {
            MyResourceType = resourceType;
            Total = 0;
        }
        public ResourceType MyResourceType { get; private set; }
		private long _total;

		/// <summary>
		/// Fixed number representing the total amount of resources in this container.
		/// </summary>
		/// <value>The total.</value>
		public long Total {
			get { return _total; }
			private set {
				if (_total != value) {
					_total = value;
					if (onChangeResource != null)
						onChangeResource ();
				}
			}
		}

		public void Add (long amount)
        {
            Total += amount;
			if (onAddResource != null)
				onAddResource (amount);
        }

		public void Clear () {
			_total = 0;
			onAddResource = null;
			onChangeResource = null;
		}

		public event Action<long> onAddResource;
		public event Action onChangeResource;
    }
}