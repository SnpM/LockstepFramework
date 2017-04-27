using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
	[System.Serializable]
	public class PathObject
	{
		[SerializeField,HideInInspector]
		protected string _path;

		[SerializeField]
		private BundleType _bundleType;
		public BundleType BundleType {get {return _bundleType;} set { _bundleType = value; }}

		public string Path { get { return _path; } }

		bool Setted = false;
		private UnityEngine.Object _object;

		public UnityEngine.Object Object {
			get {
				return Setted ? _object : (_object = Load ());
			}
		}
		protected virtual UnityEngine.Object Load () {
			
			return PathObjectFactory.Load (this);
		}


	}
}