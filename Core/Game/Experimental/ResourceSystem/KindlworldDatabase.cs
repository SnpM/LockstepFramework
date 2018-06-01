using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lockstep;
using Lockstep.Data;
using Lockstep.Experimental;

namespace Lockstep.Experimental.Data
{
    public class KindlworldDatabase : Lockstep.Data.DefaultLSDatabase
    {
        [SerializeField, RegisterData("Resources")]
        protected ResourceDataItem[] _resourceData;
        public ResourceDataItem[] ResourceData
        {
            get
            {
                return _resourceData;
            }
         }
    }
}