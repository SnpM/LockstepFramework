using UnityEngine;

namespace Lockstep.Data
{
	[System.Serializable]
	public class ObjectDataItem : MetaDataItem
	{
		[SerializeField] private GameObject _prefab;
		public GameObject Prefab { get { return _prefab; } }
	}
}