//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================


using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using FastCollections;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Lockstep.Legacy
{
	public partial class LSBody : MonoBehaviour
	{

        private void Awake()
        {
            if (Application.isEditor == false)
            {
                throw new System.Exception("Legacy.LSBody on the object '" + this.gameObject + "' should not be in a build. It should automatically replace itself with UnityLSBody in the eidtor.");
            }
        }

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


        #region Lockstep variables

        private bool _forwardNeedsSet = false;

		private bool ForwardNeedsSet {
			get { return _forwardNeedsSet; }
			set { _forwardNeedsSet = value; }
		}

		private Vector2d _forward;

		public Vector2d Forward {
			get {
				return Rotation.ToDirection ();
			}
			set {
				Rotation = value.ToRotation ();
			}
		}

		[Lockstep]
		public bool PositionChanged { get; set; }

		[Lockstep]
		public Vector2d Position {
			get {
				return _position;
			}
			set {
				_position = value;
				this.PositionChanged = true;
			}
		}

		private bool _rotationChanged;

		[Lockstep]
		public bool RotationChanged {
			get {
				return _rotationChanged;
			}
			set {
				if (value)
					ForwardNeedsSet = true;
				_rotationChanged = value;
			}
		}


		[Lockstep]
		public Vector2d Rotation {
			get {
				return _rotation;
			}
			set {
				_rotation = value;
				this.RotationChanged = true;
			}
		}

		[Lockstep]
		public bool HeightPosChanged { get; set; }

		[Lockstep]
		public long HeightPos {
			get { return _heightPos; }
			set {
				_heightPos = value;
				this.HeightPosChanged = true;
			}
		}

		[Lockstep]
		public bool VelocityChanged { get; set; }

		[Lockstep]
		public Vector2d Velocity {
			get { return _velocity; }
			set {
				_velocity = value;
				VelocityChanged = true;
			}
		}

		public Vector2d LastPosition { get; private set; }

		internal uint RaycastVersion { get; set; }

        #endregion


        #region Other variables
        internal Vector3 _visualPosition;

		public Vector3 VisualPosition { get { return _visualPosition; } }

		public LSAgent Agent { get; private set; }

		public long FastRadius { get; private set; }

		public bool PositionChangedBuffer { get; private set; }
		//D
		public bool SetPositionBuffer { get; private set; }
		//ND

		
		public bool RotationChangedBuffer { get; private set; }
		//D
		private bool SetRotationBuffer { get; set; }
		//ND

		bool SetVisualPosition { get; set; }

		bool SetVisualRotation { get; set; }

		private bool Setted { get; set; }

		
		public long VelocityFastMagnitude { get; private set; }

        #endregion
        
        #region Extra Processes
        private void AddChild (LSBody child)
		{
			if (Children == null)
				Children = new FastBucket<LSBody> ();
			Children.Add (child);
		}

		private void RemoveChild (LSBody child)
		{
			Children.Remove (child);
		}

		private FastBucket<LSBody> Children;
		public Vector2d[] RealPoints;
		public Vector2d[] Edges;
		public Vector2d[] EdgeNorms;

        
		public long XMin { get; private set; }

		public long XMax { get; private set; }

		public long YMin { get; private set; }

		public long YMax { get; private set; }

		public int PastGridXMin;
		public int PastGridXMax;
		public int PastGridYMin;
		public int PastGridYMax;

		public long HeightMin { get; private set; }

		public long HeightMax { get; private set; }

		public delegate void CollisionFunction (LSBody other);

		private Dictionary<int,CollisionPair> _collisionPairs;
		private HashSet<int> _collisionPairHolders;

		internal Dictionary<int,CollisionPair> CollisionPairs {
			get {
				return _collisionPairs.IsNotNull () ? _collisionPairs : (_collisionPairs = new Dictionary<int, CollisionPair> ());
			}
		}

		internal HashSet<int> CollisionPairHolders {
			get {
				return _collisionPairHolders ?? (_collisionPairHolders = new HashSet<int> ());
			}
		}

		internal void NotifyContact (LSBody other, bool isColliding, bool isChanged)
		{
			if (isColliding) {
				if (isChanged) {
					if (onContactEnter.IsNotNull ())
						onContactEnter (other);
				}
				if (onContact != null)
					onContact (other);
				
			} else {
				if (isChanged) {
					if (onContactExit != null)
						onContactExit (other);
				}
			}
		}

		public event CollisionFunction onContact;
		public event CollisionFunction onContactEnter;
		public event CollisionFunction onContactExit;

		public int ID { get; private set; }

		private int _dynamicID = -1;

		internal int DynamicID { get { return _dynamicID; } set { _dynamicID = value; } }

		public int Priority { get; set; }

		#region Serialized

		[SerializeField, FormerlySerializedAs ("Shape")]
		protected ColliderType _shape = ColliderType.None;

		public ColliderType Shape { get { return _shape; } }

		[SerializeField, FormerlySerializedAs ("IsTrigger")]
		private bool _isTrigger;

		public bool IsTrigger { get { return _isTrigger; } }

		[SerializeField, FormerlySerializedAs ("Layer")]
		private int _layer;

		public int Layer { get { return _layer; } }

		[SerializeField,FixedNumber, FormerlySerializedAs ("HalfWidth")]
		private long _halfWidth = FixedMath.Half;

		public long HalfWidth { get { return _halfWidth; } }

		[SerializeField,FixedNumber, FormerlySerializedAs ("HalfHeight")]
		public long _halfHeight = FixedMath.Half;

		public long HalfHeight { get { return _halfHeight; } }

		[SerializeField,FixedNumber, FormerlySerializedAs ("Radius")]
		protected long _radius = FixedMath.Half;

		public long Radius { get { return _radius; } }

		[SerializeField]
		protected bool _immovable;

		public bool Immovable { get; private set; }

		[SerializeField, FormerlySerializedAs ("_priority")]
		private int _basePriority;

		public int BasePriority { get { return _basePriority; } }

		[SerializeField, FormerlySerializedAs ("Vertices")]
		private Vector2d[] _vertices;

		public Vector2d[] Vertices { get { return _vertices; } }

		[SerializeField, FixedNumber]
		private long _height = FixedMath.One;

		[Lockstep (true)]
		public long Height { get {return _height;}}

		[SerializeField]
		private Transform _positionalTransform;

		public Transform PositionalTransform { get ; set; }

		private bool _canSetVisualPosition;

		public bool CanSetVisualPosition {
			get {
				return _canSetVisualPosition;
			}
			set {
				_canSetVisualPosition = value && PositionalTransform != null;
			}
		}

		[SerializeField]
		private Transform _rotationalTransform;

		public Vector3 _rotationOffset;

		public Transform RotationalTransform { get; set; }

		private bool _canSetVisualRotation;

		public bool CanSetVisualRotation {
			get {
				return _canSetVisualRotation && RotationalTransform != null;
			}
			set {
				_canSetVisualRotation = value;
			}
		}

		public Vector3d Position3d {
			get {
				return this.Position.ToVector3d (this.HeightPos);
			}
		}

		#endregion

		private Vector2d[] RotatedPoints;

		public void Setup (LSAgent agent)
		{

		}

		private bool OutMoreThanSet { get; set; }

		public bool OutMoreThan { get; private set; }

		public void GeneratePoints ()
		{

		}

		public void GenerateBounds ()
		{
			
		}

		public void Initialize (Vector3d StartPosition, Vector2d StartRotation, bool isDynamic = true)
		{
			
		}

		void CheckVariables ()
		{

		}

		public void BuildPoints ()
		{
			
		}

		public void BuildBounds ()
		{

		}


		public void Simulate ()
		{}

		
		public void BuildChangedValues ()
		{
			
		}

		public void SetVisuals ()
		{

		}

		private void DoSetVisualPosition (Vector3 pos)
		{
	
		}

		private void DoSetVisualRotation (Vector2d rot)
		{

		}

		public void SetExtrapolatedVisuals ()
		{
            
		}

		Vector3 lastVisualPos;
		Quaternion lastVisualRot;
		//Quaternion visualRot = Quaternion.identity;

		public void Visualize ()
		{
		}

		public void LerpOverReset ()
		{
            
		}

	
		
	


		void Reset ()
		{
			
		}

		void OnDrawGizmos ()
		{
		}

#endregion

        #if UNITY_EDITOR
        public void Replace () {
            Debug.Log("1");
			if (this.gameObject.GetComponent<UnityLSBody>() != null) {
				return;
			}
			var uBody = this.gameObject.AddComponent<UnityLSBody>();
			//var body = uBody.InternalBody;
			UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(uBody);
			SerializedProperty Shape;
			//Enum
			SerializedProperty IsTrigger;
			//bool
			SerializedProperty Layer;
			//int
			SerializedProperty BasePriority;
			//int
			SerializedProperty HalfWidth;
			//long
			SerializedProperty HalfHeight;
			//long
			SerializedProperty Radius;
			//long
			SerializedProperty Immovable;
			//bool
			//SerializedProperty Vertices;
			//Vector2d[]
			SerializedProperty Height;
			//long
			SerializedProperty PositionalTransform;
			//transform
			SerializedProperty RotationalTransform;
			#region fold
			Shape = so.FindProperty("_internalBody").FindPropertyRelative("_shape");
			IsTrigger = so.FindProperty("_internalBody").FindPropertyRelative("_isTrigger");
			Layer = so.FindProperty("_internalBody").FindPropertyRelative("_layer");
			BasePriority = so.FindProperty("_internalBody").FindPropertyRelative("_basePriority");
			HalfWidth = so.FindProperty("_internalBody").FindPropertyRelative("_halfWidth");
			HalfHeight = so.FindProperty("_internalBody").FindPropertyRelative("_halfHeight");
			Radius = so.FindProperty("_internalBody").FindPropertyRelative("_radius");
			Immovable = so.FindProperty("_internalBody").FindPropertyRelative("_immovable");
			//Vertices = so.FindProperty("_internalBody").FindPropertyRelative("_vertices");
			Height = so.FindProperty("_internalBody").FindPropertyRelative("_height");
			PositionalTransform = so.FindProperty("_internalBody").FindPropertyRelative("_positionalTransform");
			RotationalTransform = so.FindProperty("_internalBody").FindPropertyRelative("_rotationalTransform");
			#endregion

			Shape.enumValueIndex = (int)this.Shape;
			IsTrigger.boolValue = (bool)this.IsTrigger;
			Layer.intValue = (int)this.Layer;
			BasePriority.intValue = (int)this.BasePriority;
			HalfWidth.longValue = (long)this.HalfWidth;
			HalfHeight.longValue = (long)this.HalfHeight;
			Radius.longValue = (long)this.Radius;
			Immovable.boolValue = (bool)this.Immovable;
			//Vertices hard to carry over
			Height.longValue = (long)this.Height;
			PositionalTransform.objectReferenceValue = this.PositionalTransform;
			RotationalTransform.objectReferenceValue = this.RotationalTransform;
			so.ApplyModifiedProperties();
			EditorUtility.SetDirty(uBody);
		}
        #endif

    }


}
