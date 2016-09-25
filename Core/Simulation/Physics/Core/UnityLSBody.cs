//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================

using UnityEngine;
using System.Collections;

namespace Lockstep
{
	public class UnityLSBody : MonoBehaviour
	{
		[SerializeField]
		private LSBody_ _internalBody;
		public LSBody_ InternalBody {get {return _internalBody;}}
		public void Initialize(Vector3d StartPosition, Vector2d StartRotation, bool isDynamic = true)
		{
			if (_internalBody.IsNull())
				_internalBody = new LSBody_();
			InternalBody.Initialize (StartPosition,StartRotation,isDynamic);
		}

		void Reset () {
			InternalBody.transform = this.transform;
			InternalBody.Reset();
		}
	}
}