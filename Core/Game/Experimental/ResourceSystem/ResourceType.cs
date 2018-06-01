using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lockstep;
using Lockstep.Data;
namespace Lockstep.Experimental {
	
    public class ResourceType {
        public ResourceType(ResourceDataItem data)
        {
            Name = data.Name;
            Visual = data.Visual;
        }
        public string Name { get; private set; }
        public Sprite Visual { get; private set; }

		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}
    }
}