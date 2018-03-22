using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lockstep;
using Lockstep.Data;
using Lockstep.Experimental;
using FastCollections;
using Lockstep.Experimental.Data;
namespace Lockstep.Experimental
{
    public class ResourceHelper : BehaviourHelper
    {
        Dictionary<string, ResourceType> ResourceTypeMap;
       public FastList<ResourceType> ResourceTypes = new FastList<ResourceType>();
        public static ResourceHelper Instance { get; private set; }
        protected override void OnEarlyInitialize()
        {
            Instance = this;
            ResourceTypeMap = new Dictionary<string, ResourceType>();
            ResourceTypes.Clear();

            KindlworldDatabase database;
            if (LSDatabaseManager.TryGetDatabase<KindlworldDatabase> (out database))
            {
                foreach (var item in database.ResourceData)
                {
                    var resourceType = new ResourceType(item);
                    ResourceTypes.Add(resourceType);
                    ResourceTypeMap.Add(resourceType.Name, resourceType);
                }
            }
            else
            {
                Debug.Log("Kindlworld database not found");
            }
        }
    }
}