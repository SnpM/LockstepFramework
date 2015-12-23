using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
	[Serializable]
	public struct Vector2dHeight
	{
		[SerializeField, FixedNumber]
		private long
			_x;

		public long X {
			get { return _x;}
		}

		[SerializeField, FixedNumber]
		private long _y;

		public long Y { get {return _y;}}

		[SerializeField]
		private long _height;
		public long Height {get {return _height;}}

        public Vector2dHeight (Vector2d vec) {
            _x = vec.x;
            _y = vec.y;
            _height = 0;
        }

		public Vector2dHeight (Vector3 vec3) {
			_x = FixedMath.Create (vec3.x);
			_y = FixedMath.Create (vec3.z);
            _height = FixedMath.Create(vec3.y);
		}

		public Vector2d ToVector2d () {
			return new Vector2d(_x,_y);
		}
		public Vector2d ToOrientedVector2d () {
			return new Vector2d (_y, -_x);
		}
        public Vector3 ToVector3 () {
            return new Vector3(_x.ToFloat(), _height, _y.ToFloat());
        }
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X.ToFloat(), Height, Y.ToFloat());
        }

        public static explicit operator Vector2dHeight (Vector3 vec3) {
            return new Vector2dHeight (vec3);
        }
	}
}