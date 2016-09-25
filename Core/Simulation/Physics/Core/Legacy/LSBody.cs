using UnityEngine;
using System.Collections;

namespace Lockstep
{
	public class LSBody : MonoBehaviour
	{
		#region Core deterministic variables

		[SerializeField] //For inspector debugging
		internal Vector2d _position;
		[SerializeField]
		internal Vector2d _rotation = Vector2d.up;
		[SerializeField, FixedNumber]
		internal long _heightPos;
		[SerializeField]
		public Vector2d _velocity;

		#endregion

		#region Serialized

		[SerializeField]
		protected ColliderType _shape = ColliderType.None;

		public ColliderType Shape { get { return _shape; } }

		[SerializeField]
		private bool _isTrigger;

		public bool IsTrigger { get { return _isTrigger; } }

		[SerializeField]
		private int _layer;

		public int Layer { get { return _layer; } }

		[SerializeField,FixedNumber]
		private long _halfWidth = FixedMath.Half;

		public long HalfWidth { get { return _halfWidth; } }

		[SerializeField,FixedNumber]
		public long _halfHeight = FixedMath.Half;

		public long HalfHeight { get { return _halfHeight; } }

		[SerializeField,FixedNumber]
		protected long _radius = FixedMath.Half;

		public long Radius { get { return _radius; } }

		[SerializeField]
		protected bool _immovable;

		public bool Immovable { get; private set; }

		[SerializeField]
		private int _basePriority;

		public int BasePriority { get { return _basePriority; } }

		[SerializeField]
		private Vector2d[] _vertices;

		public Vector2d[] Vertices { get { return _vertices; } }

		[SerializeField, FixedNumber]
		private long _height = FixedMath.One;

		public long Height { get {return _height;}}

		[SerializeField]
		private Transform _positionalTransform;

		public Transform PositionalTransform { get ; set; }


		[SerializeField]
		private Transform _rotationalTransform;

		public Vector3 _rotationOffset;

		public Transform RotationalTransform { get; set; }


		#endregion
	}
}