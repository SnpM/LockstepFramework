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
	public sealed partial class LSBody : MonoBehaviour
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
        public long Radius {get {return _radius;}}
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
		
        private Vector2d[] RotatedPoints;
		
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
            Setted = true;
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
				_radius = Radius;
			} else if (Shape == ColliderType.AABox) {
				if (HalfHeight == HalfWidth) {
					_radius = FixedMath.Sqrt ((HalfHeight * HalfHeight * 2) >> FixedMath.SHIFT_AMOUNT);
				} else {
					_radius = FixedMath.Sqrt ((HalfHeight * HalfHeight + HalfWidth * HalfWidth) >> FixedMath.SHIFT_AMOUNT);
				}

			} else if (Shape == ColliderType.Polygon) {
				long BiggestSqrRadius = Vertices [0].SqrMagnitude ();
				for (int i = 1; i < Vertices.Length; i++) {
					long sqrRadius = Vertices [i].SqrMagnitude ();
					if (sqrRadius > BiggestSqrRadius) {
						BiggestSqrRadius = sqrRadius;
					}
				}
				_radius = FixedMath.Sqrt (BiggestSqrRadius);
			}
		}
		
		public void Initialize (Vector2dHeight StartPosition, Vector2d StartRotation)
        {
            if (!Setted) {
                this.Setup(null);
            }
            CheckVariables ();


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


			BuildChangedValues ();

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
				

					_rotation.Normalize ();
				RotationChangedBuffer = true;
				RotationChanged = false;
				this.SetVisualRotation = true;
			} else {
				RotationChangedBuffer = false;
				this.SetVisualRotation = false;

			}
		}

		public void SetVisuals () {



			if (this.SetVisualPosition)
			{
                DoSetVisualPosition (
                    _position.ToVector3(HeightPos.ToFloat())
                );
            }
			
            if (this.SetVisualRotation)
			{
                DoSetVisualRotation (_rotation);
            }
		}
        private void DoSetVisualPosition (Vector3 pos) {
            lastVisualPos = _visualPosition;
            _visualPosition = pos;
            SetPositionBuffer = true;
        }
        private void DoSetVisualRotation (Vector2d rot) {
            lastVisualRot = visualRot;
            visualRot = Quaternion.LookRotation (rot.ToVector3 (0f));
            SetRotationBuffer = true;
        }
        public void SetExtrapolatedVisuals () {
            
            if (this.SetVisualPosition) {
                Vector3 lastPos = this.lastVisualPos;
                Vector3 curPos = this._position.ToVector3(_heightPos.ToFloat());
                Vector3 delta = curPos - lastPos;
                Vector3 prediction = lastPos + delta;
                DoSetVisualPosition (prediction);
            }
            if (this.SetVisualRotation) {
            }

        }
		Vector3 lastVisualPos;
		Quaternion lastVisualRot;
		Quaternion visualRot = Quaternion.identity;
		public void Visualize ()
		{
			if (SetPositionBuffer) {
                //Interpolates between the current position and the interpolation between the last lockstep position and the current lockstep position
                //LerpTime = time passed since last simulation frame
                //LerpDamping = special value calculated based on Time.deltaTime for the extra layer of interpolation
				_positionalTransform.position = Vector3.Lerp (_positionalTransform.position,
				                                              Vector3.Lerp (lastVisualPos, _visualPosition, PhysicsManager.LerpTime),
				                                             PhysicsManager.LerpDamping);
                
			}
            const float rotationLerpDamping = .5f;
			if (SetRotationBuffer) {
                _rotationalTransform.rotation =
                    Quaternion.Lerp(
                        _rotationalTransform.rotation,
                        Quaternion.LerpUnclamped (lastVisualRot, visualRot, PhysicsManager.LerpTime),
                        rotationLerpDamping
                    );
			}
		}
		public void LerpOverReset () {
            SetPositionBuffer = false;
			SetRotationBuffer = false;
		}
		
	
		
		public void Rotate (long cos, long sin)
		{

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
            PhysicsManager.Dessimilate(this);
		}

        public bool HeightOverlaps (long heightPos) {
            return heightPos >= HeightMin && heightPos <= HeightMax;
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
                    Gizmos.DrawWireSphere(this._position.ToVector3(this.HeightPos.ToFloat()),this.Radius.ToFloat());
                    break;
                case ColliderType.AABox:
                    Gizmos.DrawWireCube(
                        this._position.ToVector3(this.HeightPos.ToFloat() + this.Height.ToFloat() / 2),
                        new Vector3(this.HalfWidth.ToFloat() * 2, this.Height.ToFloat(), this.HalfHeight.ToFloat() * 2));
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