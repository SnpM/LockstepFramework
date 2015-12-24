//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================

using UnityEngine;
using UnityEngine.Serialization;
namespace Lockstep
{
	public class LSBody : MonoBehaviour
	{
        #region Core deterministic variables
        [SerializeField] //For inspector debugging
        internal Vector2d _position;
        [SerializeField]
        internal Vector2d _rotation = Vector2d.up;
        [SerializeField]
        internal long _heightPos;
        [SerializeField]
        public Vector2d _velocity;
        #endregion

        #region Lockstep variables
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
        [Lockstep]
        public bool RotationChanged { get; set; }


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
        public bool HeightPosChanged {get; set;}
        [Lockstep]
        public long HeightPos {
            get {return _heightPos;}
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
        #endregion
        internal Vector3 _visualPosition;
        public Vector3 VisualPosition {get {return _visualPosition;}}
        		
		public LSAgent Agent { get; private set; }
		
		public long FastRadius { get; private set; }
		
		public bool PositionChangedBuffer { get; private set; } //D
        public bool SetPositionBuffer {get; private set;} //ND

		
		public bool RotationChangedBuffer { get; private set; } //D
        private bool SetRotationBuffer {get; set;} //ND

        bool SetVisualPosition {get; set;}
        bool SetVisualRotation {get; set;}
		
        private bool Setted {get; set;}

		
		public long VelocityFastMagnitude { get; private set; }
		
		
		public LSBody Parent {
			get { return HasParent ? _parent : null; }
			set {
				if (value != _parent) {
					if (HasParent) {
						_parent.RemoveChild (this);
					}
					if (value == null) {
						HasParent = false;
					} else {
						if (value.HasParent) {
							throw new System.Exception ("Cannot parent object to object with parent");
						}
						if (this.Children .IsNotNull () && this.Children.Count > 0) {
							throw new System.Exception ("Cannot child object with children");
						}
						HasParent = true;
						_parent = value;
						_parent.AddChild (this);
						this._positionalTransform.parent = _parent._positionalTransform;
						this._rotationalTransform.parent = _parent._rotationalTransform;
						UpdateLocalPosition ();
						UpdateLocalRotation ();
						LocalRotation = _rotation;
						LocalRotation.Rotate (_parent._rotation.x, _parent._rotation.y); 
						
					}
				}
			}
		}
		
		public bool HasParent { get; private set; }
		
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
		public Vector2d[] EdgeNorms;
        public long XMin {get; private set;}
        public long XMax {get; private set;}
        public long YMin {get; private set;}
        public long YMax {get; private set;}
        public int PastGridXMin;
		public int PastGridXMax;
		public int PastGridYMin;
		public int PastGridYMax;
        public long HeightMin {get; private set;}
        public long HeightMax {get; private set;}
		
		public delegate void CollisionFunction (LSBody other);
		
		public CollisionFunction OnContactEnter;
		public CollisionFunction OnContact;
		public CollisionFunction OnContactExit;
		
		public int ID { get; private set; }
		
		
		public int Priority { get; set; }
				
		#region Serialized
        [SerializeField, FormerlySerializedAs ("Shape")]
        private ColliderType _shape = ColliderType.None;
        public ColliderType Shape {get {return _shape;}}
        [SerializeField, FormerlySerializedAs ("IsTrigger")]
        private bool _isTrigger;
        public bool IsTrigger {get {return _isTrigger;}}
        [SerializeField, FormerlySerializedAs ("Layer")]
        private int _layer;
        public int Layer {get {return _layer;}}
        [SerializeField,FixedNumber, FormerlySerializedAs("HalfWidth")]
        private long _halfWidth = FixedMath.Half;
        public long HalfWidth {get {return _halfWidth;}}
        [SerializeField,FixedNumber, FormerlySerializedAs ("HalfHeight")]
        public long _halfHeight = FixedMath.Half;
        public long HalfHeight {get {return _halfHeight;}}
        [SerializeField,FixedNumber, FormerlySerializedAs ("Radius")]
        private long _radius = FixedMath.Half;
		public long Radius;
        [SerializeField, FormerlySerializedAs ("Immovable")]
        private bool _immovable = false;
		public bool Immovable;
        [SerializeField, FormerlySerializedAs("_priority")]
        private int _basePriority;
        public int BasePriority {get {return _basePriority;}}
        [SerializeField, FormerlySerializedAs("Vertices")]
        private Vector2d[] _vertices;
        public Vector2d[] Vertices {get {return _vertices;}}
        [SerializeField, FixedNumber]
        private long _height = FixedMath.One;
        public long Height {get {return _height;}}
	
        [SerializeField]
		private Transform _positionalTransform;
        public Transform PositionalTransform {get {return _positionalTransform;}}
        [SerializeField]
		private Transform _rotationalTransform;
        public Transform RotationalTransform {get {return _rotationalTransform;}}
		#endregion
		
		private LSBody _parent;
		private Vector2d[] RotatedPoints;
		public Vector2d LocalPosition;
		public Vector2d LocalRotation;
		
		public void Setup (LSAgent agent)
		{
			FastRadius = Radius * Radius;
			if (_positionalTransform == null)
				_positionalTransform = base.transform;
			if (_rotationalTransform == null)
				_rotationalTransform = base.transform;
			if (Shape == ColliderType.Polygon) {
				Immovable = true;
			}
			if (Shape != ColliderType.None) {
				GeneratePoints ();
				GenerateBounds ();
			}
			Agent = agent;
		}
		
		public void GeneratePoints ()
		{
			if (Shape != ColliderType.Polygon) {
				return;
			}
			RotatedPoints = new Vector2d[Vertices.Length];
			for (int i = 0; i < Vertices.Length; i++) {
				RotatedPoints [i] = Vertices [i];
                RotatedPoints [i].Rotate (_rotation.x, _rotation.y);
			}
			RealPoints = new Vector2d[Vertices.Length];
			EdgeNorms = new Vector2d[Vertices.Length];
		}
		
		public void GenerateBounds ()
		{
			if (Shape == ColliderType.Circle) {
				Radius = Radius;
			} else if (Shape == ColliderType.AABox) {
				if (HalfHeight == HalfWidth) {
					Radius = FixedMath.Sqrt ((HalfHeight * HalfHeight * 2) >> FixedMath.SHIFT_AMOUNT);
				} else {
					Radius = FixedMath.Sqrt ((HalfHeight * HalfHeight + HalfWidth * HalfWidth) >> FixedMath.SHIFT_AMOUNT);
				}

			} else if (Shape == ColliderType.Polygon) {
				long BiggestSqrRadius = Vertices [0].SqrMagnitude ();
				for (int i = 1; i < Vertices.Length; i++) {
					long sqrRadius = Vertices [i].SqrMagnitude ();
					if (sqrRadius > BiggestSqrRadius) {
						BiggestSqrRadius = sqrRadius;
					}
				}
				Radius = FixedMath.Sqrt (BiggestSqrRadius);
			}
		}
		
		public void Initialize (Vector2dHeight StartPosition, Vector2d StartRotation)
        {
            if (!Setted) {
                this.Setup(null);
                Setted = true;
            }
            CheckVariables ();

			Parent = null;
			
			PositionChanged = true;
			RotationChanged = true;
			VelocityChanged = true;
			PositionChangedBuffer = false;
			RotationChangedBuffer = false;
			
			Priority = _basePriority;
			Velocity = Vector2d.zero;
			VelocityFastMagnitude = 0;
            _position = StartPosition.ToVector2d();
            _heightPos = StartPosition.Height;
			_rotation = StartRotation;


			_parent = null;
			LocalPosition = Vector2d.zero;
			LocalRotation = Vector2d.up;
			
			XMin = 0;
			XMax = 0;
			YMin = 0;
			YMax = 0;
			
			
			PastGridXMin = int.MaxValue;
			PastGridXMax = int.MaxValue;
			PastGridYMin = int.MaxValue;
			PastGridYMax = int.MaxValue;
			
			if (Shape != ColliderType.None) {
				BuildPoints ();
				BuildBounds ();
			}
			
			ID = PhysicsManager.Assimilate (this);
			Partition.PartitionObject (this);
			
            _visualPosition = _position.ToVector3 (HeightPos.ToFloat());
			lastVisualPos = _visualPosition;
			_positionalTransform.position = _visualPosition;
            UnityEngine.Profiler.maxNumberOfSamplesPerFrame = 7000000;
			visualRot = Quaternion.LookRotation (_rotation.ToVector3 (0f));
			lastVisualRot = visualRot;
			_positionalTransform.rotation = visualRot;
		}

        void CheckVariables () {
            if (_positionalTransform == null)
                this._positionalTransform = base.transform;
            if (_rotationalTransform == null)
                this._rotationalTransform = base.transform;
        }
		
		public void BuildPoints ()
		{
			if (Shape != ColliderType.Polygon) {
				return;
			}
			int VertLength = Vertices.Length;
			
			if (RotationChanged) {
				for (int i = 0; i < VertLength; i++) {
					RotatedPoints [i] = Vertices [i];
					RotatedPoints [i].Rotate (_rotation.x, _rotation.y);
					
					EdgeNorms [i] = RotatedPoints [i];
					if (i == 0) {
						EdgeNorms [i].Subtract (ref RotatedPoints [VertLength - 1]);
					} else {
						EdgeNorms [i].Subtract (ref RotatedPoints [i - 1]);
					}
					EdgeNorms [i].Normalize ();
					EdgeNorms [i].RotateRight ();
				}
			}
			for (int i = 0; i < Vertices.Length; i++) {
				RealPoints [i].x = RotatedPoints [i].x + _position.x;
				RealPoints [i].y = RotatedPoints [i].y + _position.y;
			}
		}
		
		public void BuildBounds ()
		{
            HeightMin = HeightPos;
            HeightMax = HeightPos + Height;
			if (Shape == ColliderType.Circle) {
				XMin = -Radius + _position.x;
				XMax = Radius + _position.x;
				YMin = -Radius + _position.y;
				YMax = Radius + _position.y;
			} else if (Shape == ColliderType.AABox) {
				XMin = -HalfWidth + _position.x;
				XMax = HalfWidth + _position.x;
				YMin = -HalfHeight + _position.y;
				YMax = HalfHeight + _position.y;
			} else if (Shape == ColliderType.Polygon) {
				XMin = _position.x;
				XMax = _position.x;
				YMin = _position.y;
				YMax = _position.y;
				for (int i = 0; i < Vertices.Length; i++) {
					Vector2d vec = RealPoints [i];
					if (vec.x < XMin) {
						XMin = vec.x;
					} else if (vec.x > XMax) {
						XMax = vec.x;
					}
					
					if (vec.y < YMin) {
						YMin = vec.y;
					} else if (vec.y > YMax) {
						YMax = vec.y;
					}
				}
			}
		}
		
		public void EarlySimulate ()
		{
			if (HasParent)
				return;

			if (VelocityChanged) {
				VelocityFastMagnitude = Velocity.FastMagnitude ();
				VelocityChanged = false;
			}
			
			if (VelocityFastMagnitude != 0) {
				_position.x += Velocity.x;
				_position.y += Velocity.y;
				PositionChanged = true;
			}
			
			if (PositionChanged) {
				Partition.UpdateObject (this);
			}
			if (RotationChanged) {
			} else {
			}
		}
		
		public void Simulate ()
		{
			if (HasParent)
				return;
			
			if (PositionChanged || RotationChanged) {
				if (PositionChanged) {

				} else {
				}
				
				if (RotationChanged) {
				} else {
				}
				
			} else {
				
			}
			
		}
		
		public void LateSimulate ()
		{
			if (HasParent) {
				ChildSimulate ();
			}
			if (PhysicsManager.SetVisuals) {
				SetVisuals ();
			}
			BuildChangedValues ();

		}
		
		private void ChildSimulate ()
		{
			
			if (_parent.RotationChangedBuffer) {
				UpdateRotation ();
				UpdatePosition ();
			} else {
				if (_parent.PositionChangedBuffer || this.PositionChanged) {
					UpdatePosition ();
				}
				if (this.RotationChanged) {
					UpdateRotation ();
				}
			}
			
		}
		
		public void BuildChangedValues ()
		{
			if (PositionChanged || RotationChanged) {
				BuildPoints ();
				BuildBounds ();
			}
			if (PositionChanged) {
				PositionChangedBuffer = true;
				PositionChanged = false;
				this.SetVisualPosition = true;
			} else {
				PositionChangedBuffer = false;
				this.SetVisualPosition = false;
			}
			
			if (RotationChanged) {
				
				if (HasParent)
					LocalRotation.Normalize ();
				else
					_rotation.Normalize ();
				RotationChangedBuffer = true;
				RotationChanged = false;
				this.SetVisualRotation = true;
			} else {
				RotationChangedBuffer = false;
				this.SetVisualRotation = false;

			}
		}
		private bool visualPositionReached;
		private bool visualRotationReached;
		private void SetVisuals () {

			const bool test = false;
			if (HasParent) {

			}
			else {
				if (SetVisualPosition == false) {
					if (visualPositionReached == false)
					visualPositionReached = !(SetVisualPosition = _positionalTransform.position != _visualPosition);
				}
				if (SetVisualRotation == false) {
					if (visualRotationReached == false)
					visualRotationReached = !(SetVisualRotation = _rotationalTransform.rotation != this.visualRot);
				}
			}
			if (test || this.SetVisualPosition)
			{
				if (HasParent) {
					this._positionalTransform.localPosition = this.LocalPosition.rotatedLeft.ToVector3 (_visualPosition.y - _parent._visualPosition.y);
				}
				else {
					lastVisualPos = _visualPosition;
					_visualPosition.x = _position.x.ToFloat ();
                    _visualPosition.y = HeightPos.ToFloat();
					_visualPosition.z = _position.y.ToFloat ();
					SetPositionBuffer = true;
					visualPositionReached = false;
				}
			}
			else if (HasParent == false){
				if (SetPositionBuffer) {
					//_positionalTransform.position = _visualPosition;
					SetPositionBuffer = false;
				}
			}

			if (test || this.SetVisualRotation)
			{
				if (HasParent) {
					this._rotationalTransform.localRotation = Quaternion.LookRotation (this.LocalRotation.rotatedLeft.ToVector3 (0f));
				}
				else {
					lastVisualRot = visualRot;
					visualRot = Quaternion.LookRotation (_rotation.ToVector3 (0f));
					SetRotationBuffer = true;
					visualRotationReached = false;
				}
			}
			else if (HasParent == false) {
				if (SetRotationBuffer) {
					//_rotationalTransform.rotation = visualRot;
					SetRotationBuffer = false;
				}
			}
		}

		Vector3 lastVisualPos;
		Quaternion lastVisualRot;
		Quaternion visualRot = Quaternion.identity;
		public void Visualize ()
		{
			if (HasParent) return;

			if (SetPositionBuffer) {
                //Interpolates between the current position and the interpolation between the last lockstep position and the current lockstep position
                //LerpTime = time passed since last simulation frame
                //LerpDamping = special value calculated based on Time.deltaTime for the extra layer of interpolation
				_positionalTransform.position = Vector3.Lerp (_positionalTransform.position,
				                                              Vector3.Lerp (lastVisualPos, _visualPosition, PhysicsManager.LerpTime),
				                                             PhysicsManager.LerpDamping);
                
			}
			
			if (SetRotationBuffer) {
				_rotationalTransform.rotation = Quaternion.Lerp(_rotationalTransform.rotation, Quaternion.LerpUnclamped (lastVisualRot, visualRot, PhysicsManager.LerpTime), PhysicsManager.LerpDamping * 2f);
			}
		}
		public void LerpOverReset () {
			if (HasParent) return;
			SetPositionBuffer = false;
			SetRotationBuffer = false;
		}
		
		public void UpdateLocalPosition ()
		{
			if (HasParent) {
				LocalPosition = _position - _parent._position;
				LocalPosition.Rotate (_parent._rotation.x, _parent._rotation.y);
			}
		}
		
		public void UpdatePosition ()
		{
			if (HasParent) {
				_position = LocalPosition;
				_position.RotateInverse (_parent._rotation.x, _parent._rotation.y);
				_position += _parent._position;
				PositionChanged = true;
			}
		}
		
		public void UpdateRotation ()
		{
			if (HasParent) {
				_rotation = LocalRotation;
				_rotation.RotateInverse (_parent._rotation.x, _parent._rotation.y);
				RotationChanged = true;
			}
		}
		
		public void UpdateLocalRotation ()
		{
			if (HasParent) {
				LocalRotation = _rotation;
				LocalRotation.Rotate (_parent._rotation.x, _parent._rotation.y);
			}
		}
		
		public void Rotate (long cos, long sin)
		{
			if (HasParent)
				LocalRotation.Rotate (cos, sin);
			else
				_rotation.Rotate (cos, sin);
			RotationChanged = true;
		}
		
		public void SetRotation (long x, long y)
		{
			_rotation = new Vector2d (x, y);
			RotationChanged = true;
		}
		
		public Vector2d TransformDirection (Vector2d worldPos)
		{
			worldPos -= _position;
			worldPos.RotateInverse (_rotation.x, _rotation.y);
			return worldPos;
		}
		
		public Vector2d InverseTransformDirection (Vector2d localPos)
		{
			localPos.Rotate (_rotation.x, _rotation.y);
			localPos += _position;
			return localPos;
		}
		
		public override int GetHashCode ()
		{
			return ID;
		}
		
		public void Deactivate ()
		{
			Parent = null;
			if (Children .IsNotNull ())
			for (int i = 0; i < Children.PeakCount; i++) {
				if (Children.arrayAllocation [i]) {
					Children [i].Parent = null;
				}
			}
            PhysicsManager.Dessimilate(this);
		}

        public bool HeightOverlaps (long heightPos) {
            return heightPos >= HeightMin && HeightPos <= HeightMax;
        }
        public bool HeightOverlaps (long heightMin, long heightMax) {
            return heightMax >= HeightMin && heightMin <= HeightMax;
        }

        long GetCeiledSnap (long f, long snap) {
            return (f + snap - 1) / snap * snap;
        }
        long GetFlooredSnap (long f, long snap) {
            return (f / snap) * snap;
        }
        public void GetCoveredSnappedPositions (long snapSpacing, FastList<Vector2d> output) {
            long referenceX = 0,
            referenceY = 0;
            long xmin = GetFlooredSnap (this.XMin - FixedMath.Half, snapSpacing);
            long ymin = GetFlooredSnap (this.YMin - FixedMath.Half, snapSpacing);

            long xmax = GetCeiledSnap (this.XMax + FixedMath.Half - xmin, snapSpacing) + xmin;
            long ymax = GetCeiledSnap (this.YMax + FixedMath.Half - ymin, snapSpacing) + ymin;
            //Used for getting snapped positions this body covered
            for (long x = xmin; x < xmax; x+= snapSpacing) {
                for (long y = ymin; y < ymax; y += snapSpacing) {
                    Vector2d checkPos = new Vector2d(x,y);
                    if (IsPositionCovered (checkPos)) {
                        output.Add (checkPos);
                    }
                }
            }
        }
        public bool IsPositionCovered (Vector2d position) {
            //Checks if this body covers a position

            //Different techniques for different shapes
            switch (this.Shape) {
                case ColliderType.Circle:
                    long maxDistance = this.Radius + FixedMath.Half;
                    maxDistance *= maxDistance;
                    if ((this._position - position).FastMagnitude() > maxDistance)
                        return false;
                    goto case ColliderType.AABox;
                case ColliderType.AABox:
                    return position.x + FixedMath.Half >= this.XMin && position.x - FixedMath.Half <= this.XMax
                        && position.y + FixedMath.Half >= this.YMin && position.y - FixedMath.Half <= this.YMax;
                    break;
            }
            return false;
        }

        void Reset () {
            this._positionalTransform = this.transform;
            this._rotationalTransform = this.transform;
        }
        void OnDrawGizmos () {
            //Don't draw gizmos before initialization
            if (Application.isPlaying == false) return;
            switch (this.Shape) {
                case ColliderType.Circle:
                    Gizmos.DrawWireSphere(this._position.ToVector3(transform.position.y + .5f),this.Radius.ToFloat());
                    break;
            }
        }
	}
	
	public enum ColliderType : byte
	{
		None,
		Circle,
		AABox,
		Polygon
	}
}