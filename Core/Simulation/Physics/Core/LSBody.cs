//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================
using UnityEngine;

namespace Lockstep
{
	public class LSBody : MonoBehaviour
	{
		public Vector3 visualPosition;
        public Vector2d _position;

		
		public LSAgent Agent { get; private set; }
		
		public long FastRadius { get; private set; }

		
		public bool PositionChanged { get; set; }
		
		public bool PositionChangedBuffer { get; private set; } //D
		public bool SetPositionBuffer; //ND
		public Vector2d Rotation = Vector2d.up;
		
		public bool RotationChanged { get; set; }
		
		private bool RotationChangedBuffer { get; set; } //D
		private bool SetRotationBuffer; //ND

		bool SetVisualPosition;
		bool SetVisualRotation;
		
        public bool Setted {get; private set;}

		public Vector2d _velocity;
		
		public Vector2d Velocity {
			get { return _velocity; }
			set {
				_velocity = value;
				VelocityChanged = true;
			}
		}
		
		public long VelocityFastMagnitude { get; private set; }
		
		public bool VelocityChanged { get; set; }
		
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
						LocalRotation = Rotation;
						LocalRotation.Rotate (_parent.Rotation.x, _parent.Rotation.y); 
						
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
		public long XMin;
		public long XMax;
		public long YMin;
		public long YMax;
		public long PastGridXMin;
		public long PastGridXMax;
		public long PastGridYMin;
		public long PastGridYMax;
		
		public delegate void CollisionFunction (LSBody other);
		
		public CollisionFunction OnContactEnter;
		public CollisionFunction OnContact;
		public CollisionFunction OnContactExit;
		
		public int ID { get; private set; }
		
		[SerializeField]
		private int
			_priority;
		
		public int Priority { get; set; }
		
		public uint RaycastVersion;
		
		#region Serialized
		public ColliderType Shape;
		public bool IsTrigger;
		public  int Layer;
        public long HalfWidth = FixedMath.Half;
        public long HalfHeight = FixedMath.Half;
		public long Radius;
		public bool Immovable;
	
		public Transform _positionalTransform;
		public Transform _rotationalTransform;
		#endregion
		
		private LSBody _parent;
		private Vector2d[] RotatedPoints;
		public Vector2d LocalPosition;
		public Vector2d LocalRotation;
		public Vector2d[] Vertices;
		
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
				RotatedPoints [i].Rotate (Rotation.x, Rotation.y);
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
		
		public void InitializeSerialized ()
		{
            Initialize (this._position, Rotation);
		}
		
		public void Initialize (Vector2d startPosition)
		{
			Initialize (startPosition, Rotation);
		}
		
		public void Initialize (Vector2d StartPosition, Vector2d StartRotation)
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
			
			Priority = _priority;
			Velocity = Vector2d.zero;
			VelocityFastMagnitude = 0;
			_position = StartPosition;
			Rotation = StartRotation;


			_parent = null;
			LocalPosition = Vector2d.zero;
			LocalRotation = Vector2d.up;
			
			XMin = 0;
			XMax = 0;
			YMin = 0;
			YMax = 0;
			
			
			PastGridXMin = long.MaxValue;
			PastGridXMax = long.MaxValue;
			PastGridYMin = long.MaxValue;
			PastGridYMax = long.MaxValue;
			
			if (Shape != ColliderType.None) {
				BuildPoints ();
				BuildBounds ();
			}
			
			ID = PhysicsManager.Assimilate (this);
			Partition.PartitionObject (this);
			
			visualPosition = _position.ToVector3 (0f);
			lastVisualPos = visualPosition;
			_positionalTransform.position = visualPosition;
            UnityEngine.Profiler.maxNumberOfSamplesPerFrame = 7000000;
			visualRot = Quaternion.LookRotation (Rotation.ToVector3 (0f));
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
					RotatedPoints [i].Rotate (Rotation.x, Rotation.y);
					
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
					Rotation.Normalize ();
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
					visualPositionReached = !(SetVisualPosition = _positionalTransform.position != visualPosition);
				}
				if (SetVisualRotation == false) {
					if (visualRotationReached == false)
					visualRotationReached = !(SetVisualRotation = _rotationalTransform.rotation != this.visualRot);
				}
			}
			if (test || this.SetVisualPosition)
			{
				if (HasParent) {
					this._positionalTransform.localPosition = this.LocalPosition.rotatedLeft.ToVector3 (visualPosition.y - _parent.visualPosition.y);
				}
				else {
					lastVisualPos = visualPosition;
					visualPosition.x = _position.x.ToFloat ();
					visualPosition.z = _position.y.ToFloat ();
					SetPositionBuffer = true;
					visualPositionReached = false;
				}
			}
			else if (HasParent == false){
				if (SetPositionBuffer) {
					//_positionalTransform.position = visualPosition;
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
					visualRot = Quaternion.LookRotation (Rotation.ToVector3 (0f));
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
				_positionalTransform.position = Vector3.Lerp (_positionalTransform.position,
				                                              Vector3.Lerp (lastVisualPos, visualPosition, PhysicsManager.LerpTime),
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
				LocalPosition.Rotate (_parent.Rotation.x, _parent.Rotation.y);
			}
		}
		
		public void UpdatePosition ()
		{
			if (HasParent) {
				_position = LocalPosition;
				_position.RotateInverse (_parent.Rotation.x, _parent.Rotation.y);
				_position += _parent._position;
				PositionChanged = true;
			}
		}
		
		public void UpdateRotation ()
		{
			if (HasParent) {
				Rotation = LocalRotation;
				Rotation.RotateInverse (_parent.Rotation.x, _parent.Rotation.y);
				RotationChanged = true;
			}
		}
		
		public void UpdateLocalRotation ()
		{
			if (HasParent) {
				LocalRotation = Rotation;
				LocalRotation.Rotate (_parent.Rotation.x, _parent.Rotation.y);
			}
		}
		
		public void Rotate (long cos, long sin)
		{
			if (HasParent)
				LocalRotation.Rotate (cos, sin);
			else
				Rotation.Rotate (cos, sin);
			RotationChanged = true;
		}
		
		public void SetRotation (long x, long y)
		{
			Rotation = new Vector2d (x, y);
			RotationChanged = true;
		}
		
		public Vector2d TransformDirection (Vector2d worldPos)
		{
			worldPos -= _position;
			worldPos.RotateInverse (Rotation.x, Rotation.y);
			return worldPos;
		}
		
		public Vector2d InverseTransformDirection (Vector2d localPos)
		{
			localPos.Rotate (Rotation.x, Rotation.y);
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

		public void Teleport (Vector2d destination) {
			this._position = destination;
			this.visualPosition = destination.ToVector3 (visualPosition.y);
			this.lastVisualPos = visualPosition;
			this._positionalTransform.position = visualPosition;
		}
		
		public bool GetAgentAbility<T> (out T abil) where T : Ability{
			if (Agent .IsNotNull ()) {
				abil = Agent.GetAbility<T> ();
				if (abil .IsNotNull ()) {
					return true;
				}
			}
			abil = null;
			return false;
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

	}
	
	public enum ColliderType : byte
	{
		None,
		Circle,
		AABox,
		Polygon
	}
}