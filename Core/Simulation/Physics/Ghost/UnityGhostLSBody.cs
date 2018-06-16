using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Lockstep.Extras {
	public class UnityGhostLSBody : MonoBehaviour {
		[SerializeField]
		private GhostLSBody _internalBody;
		public GhostLSBody InternalBody {get {return _internalBody ?? (_internalBody = new GhostLSBody());}}
		public void Initialize(Vector3d StartPosition, Vector2d StartRotation, bool isDynamic = true)
		{
			if (_internalBody.IsNull())
				_internalBody = new GhostLSBody();
			InternalBody.InitializeGhost (StartPosition,StartRotation,isDynamic);
		}

		void Reset () {
			if (InternalBody.IsNull()) 
				_internalBody = new GhostLSBody();
			InternalBody.transform = this.transform;
			InternalBody.Reset();
		}
	}
}