using UnityEngine;
using System.Collections; using FastCollections;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Lockstep.Data
{

    [System.Serializable]
	public class ObjectDataItem : MetaDataItem
	{
		[SerializeField]
		private GameObject
			_prefab;
		
		public GameObject Prefab { get { return _prefab; } }
	}
}