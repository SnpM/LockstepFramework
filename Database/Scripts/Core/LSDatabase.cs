using UnityEngine;
using System.Collections; using FastCollections;
using System.Collections.Generic;
using System;
using Lockstep;
using Lockstep.Data;

namespace Lockstep.Data {
	//TODO: Implement usage of IDatabase rather than LSDatabase
    [Serializable]
	public abstract class LSDatabase : ScriptableObject, IDatabase {

    }

}
