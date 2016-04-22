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
        [SerializeField, FixedNumber]
        internal long _heightPos;
        [SerializeField]
        public Vector2d _velocity;

        #endregion

        #region Lockstep variables

        private bool ForwardNeedsSet;
        private Vector2d _forward;

        public Vector2d Forward
        {
            get
            {
                if (ForwardNeedsSet)
                {
                    _forward = _rotation.ToDirection();
                    ForwardNeedsSet = false;
                }
                return _forward;
            }
        }

        [Lockstep]
        public bool PositionChanged { get; set; }

        [Lockstep]
        public Vector2d Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                this.PositionChanged = true;
            }
        }

        private bool _rotationChanged;

        [Lockstep]
        public bool RotationChanged
        {
            get
            {
                return _rotationChanged;
            }
            set
            {
                if (value)
                    ForwardNeedsSet = true;
                _rotationChanged = value;
            }
        }


        [Lockstep]
        public Vector2d Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
                this.RotationChanged = true;
            }
        }

        [Lockstep]
        public bool HeightPosChanged { get; set; }

        [Lockstep]
        public long HeightPos
        {
            get { return _heightPos; }
            set
            {
                _heightPos = value;
                this.HeightPosChanged = true;
            }
        }

        [Lockstep]
        public bool VelocityChanged { get; set; }

        [Lockstep]
        public Vector2d Velocity
        {
            get { return _velocity; }
            set
            {
                _velocity = value;
                VelocityChanged = true;
            }
        }

        public Vector2d LastPosition { get; private set; }

        internal uint RaycastVersion { get; set; }

        #endregion

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



        private void AddChild(LSBody child)
        {
            if (Children == null)
                Children = new FastBucket<LSBody>();
            Children.Add(child);
        }

        private void RemoveChild(LSBody child)
        {
            Children.Remove(child);
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

        public delegate void CollisionFunction(LSBody other);

        public CollisionFunction OnContactEnter;
        public CollisionFunction OnContact;
        public CollisionFunction OnContactExit;

        public int ID { get; private set; }

		
        public int Priority { get; set; }

        #region Serialized

        [SerializeField, FormerlySerializedAs("Shape")]
        private ColliderType _shape = ColliderType.None;

        public ColliderType Shape { get { return _shape; } }

        [SerializeField, FormerlySerializedAs("IsTrigger")]
        private bool _isTrigger;

        public bool IsTrigger { get { return _isTrigger; } }

        [SerializeField, FormerlySerializedAs("Layer")]
        private int _layer;

        public int Layer { get { return _layer; } }

        [SerializeField,FixedNumber, FormerlySerializedAs("HalfWidth")]
        private long _halfWidth = FixedMath.Half;

        public long HalfWidth { get { return _halfWidth; } }

        [SerializeField,FixedNumber, FormerlySerializedAs("HalfHeight")]
        public long _halfHeight = FixedMath.Half;

        public long HalfHeight { get { return _halfHeight; } }

        [SerializeField,FixedNumber, FormerlySerializedAs("Radius")]
        private long _radius = FixedMath.Half;

        public long Radius { get { return _radius; } }

        [SerializeField]
        private bool _immovable;

        public bool Immovable { get { return _immovable || this.Shape != ColliderType.Circle; } }

        [SerializeField, FormerlySerializedAs("_priority")]
        private int _basePriority;

        public int BasePriority { get { return _basePriority; } }

        [SerializeField, FormerlySerializedAs("Vertices")]
        private Vector2d[] _vertices;

        public Vector2d[] Vertices { get { return _vertices; } }

        [SerializeField, FixedNumber]
        private long _height = FixedMath.One;

        [Lockstep(true)]
        public long Height { get; private set; }

        [SerializeField]
        private Transform _positionalTransform;

        public Transform PositionalTransform { get ; set; }

        private bool _canSetVisualPosition;

        public bool CanSetVisualPosition
        {
            get
            {
                return _canSetVisualPosition;
            }
            set
            {
                _canSetVisualPosition = value && PositionalTransform != null;
            }
        }

        [SerializeField]
        private Transform _rotationalTransform;

        public Transform RotationalTransform { get; set; }

        private bool _canSetVisualRotation;

        public bool CanSetVisualRotation
        {
            get
            {
                return _canSetVisualRotation && RotationalTransform != null;
            }
            set
            {
                _canSetVisualRotation = value;
            }
        }

        #endregion

        private Vector2d[] RotatedPoints;

        public void Setup(LSAgent agent)
        {

            if (Shape == ColliderType.Polygon)
            {
            }
            if (Shape != ColliderType.None)
            {
                GeneratePoints();
                GenerateBounds();
            }
            Agent = agent;
            Setted = true;

            Height = _height;
        }

        private bool OutMoreThanSet { get; set; }

        public bool OutMoreThan { get; private set; }

        public void GeneratePoints()
        {
            if (Shape != ColliderType.Polygon)
            {
                return;
            }
            RotatedPoints = new Vector2d[Vertices.Length];
            RealPoints = new Vector2d[Vertices.Length];
            Edges = new Vector2d[Vertices.Length];
            EdgeNorms = new Vector2d[Vertices.Length];
        }

        public void GenerateBounds()
        {
            if (Shape == ColliderType.Circle)
            {
                _radius = Radius;
            } else if (Shape == ColliderType.AABox)
            {
                if (HalfHeight == HalfWidth)
                {
                    _radius = FixedMath.Sqrt((HalfHeight * HalfHeight * 2) >> FixedMath.SHIFT_AMOUNT);
                } else
                {
                    _radius = FixedMath.Sqrt((HalfHeight * HalfHeight + HalfWidth * HalfWidth) >> FixedMath.SHIFT_AMOUNT);
                }

            } else if (Shape == ColliderType.Polygon)
            {
                long BiggestSqrRadius = Vertices [0].SqrMagnitude();
                for (int i = 1; i < Vertices.Length; i++)
                {
                    long sqrRadius = Vertices [i].SqrMagnitude();
                    if (sqrRadius > BiggestSqrRadius)
                    {
                        BiggestSqrRadius = sqrRadius;
                    }
                }
                _radius = FixedMath.Sqrt(BiggestSqrRadius);
                FastRadius = this.Radius * this.Radius;
            }
        }

        public void Initialize(Vector2dHeight StartPosition, Vector2d StartRotation)
        {
            PositionalTransform = _positionalTransform;
            RotationalTransform = _rotationalTransform;
            if (!Setted)
            {
                this.Setup(null);
            }
            this.RaycastVersion = 0;

            this.HeightPosChanged = true;

            CheckVariables();

            PositionChanged = true;
            RotationChanged = true;
            VelocityChanged = true;
            PositionChangedBuffer = true;
            RotationChangedBuffer = true;
			
            Priority = _basePriority;
            Velocity = Vector2d.zero;
            VelocityFastMagnitude = 0;
            LastPosition = _position = StartPosition.ToVector2d();
            _heightPos = StartPosition.Height;
            _rotation = StartRotation;
            ForwardNeedsSet = true;

            XMin = 0;
            XMax = 0;
            YMin = 0;
            YMax = 0;
			
			
            PastGridXMin = int.MaxValue;
            PastGridXMax = int.MaxValue;
            PastGridYMin = int.MaxValue;
            PastGridYMax = int.MaxValue;
			
            if (Shape != ColliderType.None)
            {
                BuildPoints();
                BuildBounds();
            }
			
            ID = PhysicsManager.Assimilate(this);
            Partition.PartitionObject(this);
            if (PositionalTransform != null)
            {
                CanSetVisualPosition = true;
                _visualPosition = _position.ToVector3(HeightPos.ToFloat());
                lastVisualPos = _visualPosition;
                PositionalTransform.position = _visualPosition;
            } else
            {
                CanSetVisualPosition = false;
            }
            if (RotationalTransform != null)
            {
                CanSetVisualRotation = true;
                visualRot = Quaternion.LookRotation(Forward.ToVector3(0f));
                lastVisualRot = visualRot;
                RotationalTransform.rotation = visualRot;
            } else
            {
                CanSetVisualRotation = false;
            }
        }

        void CheckVariables()
        {

        }

        public void BuildPoints()
        {
            if (Shape != ColliderType.Polygon)
            {
                return;
            }
            int VertLength = Vertices.Length;
			
            if (RotationChanged)
            {
                for (int i = 0; i < VertLength; i++)
                {
                    RotatedPoints [i] = Vertices [i];
                    RotatedPoints [i].Rotate(_rotation.x, _rotation.y);
                }
                for (int i = VertLength - 1; i >= 0; i--)
                {
                    int nextIndex = i + 1 < VertLength ? i + 1 : 0;
                    Vector2d point = RotatedPoints [nextIndex];
                    point.Subtract(ref RotatedPoints [i]);
                    point.Normalize();
                    Edges [i] = point;
                    point.RotateRight();
                    EdgeNorms [i] = point;
                }
                if (!OutMoreThanSet)
                {
                    OutMoreThanSet = true;
                    long dot = Edges [0].Cross(Edges [1]);
                    this.OutMoreThan = dot < 0;
                }
            }
            for (int i = 0; i < VertLength; i++)
            {
                RealPoints [i].x = RotatedPoints [i].x + _position.x;
                RealPoints [i].y = RotatedPoints [i].y + _position.y;
            }
        }

        public void BuildBounds()
        {
            HeightMin = HeightPos;
            HeightMax = HeightPos + Height;
            if (Shape == ColliderType.Circle)
            {
                XMin = -Radius + _position.x;
                XMax = Radius + _position.x;
                YMin = -Radius + _position.y;
                YMax = Radius + _position.y;
            } else if (Shape == ColliderType.AABox)
            {
                XMin = -HalfWidth + _position.x;
                XMax = HalfWidth + _position.x;
                YMin = -HalfHeight + _position.y;
                YMax = HalfHeight + _position.y;
            } else if (Shape == ColliderType.Polygon)
            {
                XMin = _position.x;
                XMax = _position.x;
                YMin = _position.y;
                YMax = _position.y;
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vector2d vec = RealPoints [i];
                    if (vec.x < XMin)
                    {
                        XMin = vec.x;
                    } else if (vec.x > XMax)
                    {
                        XMax = vec.x;
                    }
					
                    if (vec.y < YMin)
                    {
                        YMin = vec.y;
                    } else if (vec.y > YMax)
                    {
                        YMax = vec.y;
                    }
                }
            }
        }

        public void EarlySimulate()
        {
            if (VelocityChanged)
            {
                VelocityFastMagnitude = _velocity.FastMagnitude();
                VelocityChanged = false;
            }
            if (VelocityFastMagnitude != 0)
            {

                _position.x += _velocity.x;
                _position.y += _velocity.y;
                PositionChanged = true;
            }
			
            if (PositionChanged || this.PositionChangedBuffer)
            {
                Partition.UpdateObject(this);
            }
            if (RotationChanged)
            {
            } else
            {
            }
        }

        public void Simulate()
        {

            if (PositionChanged || RotationChanged)
            {
                if (PositionChanged)
                {

                } else
                {
                }
				
                if (RotationChanged)
                {
                } else
                {
                }
				
            } else
            {
				
            }
			
        }

        public void LateSimulate()
        {


            BuildChangedValues();

        }

		
        public void BuildChangedValues()
        {
            if (PositionChanged || RotationChanged)
            {
                BuildPoints();
                BuildBounds();
            }
            if (PositionChanged || this.HeightPosChanged)
            {
                LastPosition = _position;
                PositionChangedBuffer = true;
                PositionChanged = false;
                this.SetVisualPosition = true;
				this.HeightPosChanged = false;
            } else
            {
                PositionChangedBuffer = false;
                this.SetVisualPosition = false;
            }
			
            if (RotationChanged)
            {
                _rotation.Normalize();
                RotationChangedBuffer = true;
                RotationChanged = false;
                this.SetVisualRotation = true;
            } else
            {
                RotationChangedBuffer = false;
                this.SetVisualRotation = false;

            }
        }

        public void SetVisuals()
        {



            if (this.SetVisualPosition)
            {
                DoSetVisualPosition(
                    _position.ToVector3(HeightPos.ToFloat())
                );
            }
			
            if (this.SetVisualRotation)
            {
                this.DoSetVisualRotation(_rotation);
            }
        }

        private void DoSetVisualPosition(Vector3 pos)
        {
            lastVisualPos = _visualPosition;
            _visualPosition = pos;
            SetPositionBuffer = true;
        }

        private void DoSetVisualRotation(Vector2d rot)
        {
            lastVisualRot = visualRot;
            visualRot = Quaternion.LookRotation(Forward.ToVector3(0f));
            SetRotationBuffer = true;
        }

        public void SetExtrapolatedVisuals()
        {
            
            if (this.SetVisualPosition)
            {
                Vector3 lastPos = this.lastVisualPos;
                Vector3 curPos = this._position.ToVector3(_heightPos.ToFloat());
                Vector3 delta = curPos - lastPos;
                Vector3 prediction = lastPos + delta;
                DoSetVisualPosition(prediction);
            }
            if (this.SetVisualRotation)
            {
                this.DoSetVisualRotation(_rotation);
            }

        }

        Vector3 lastVisualPos;
        Quaternion lastVisualRot;
        Quaternion visualRot = Quaternion.identity;

        public void Visualize()
        {
            if (CanSetVisualPosition)
            {
                if (SetPositionBuffer)
                {
                    //Interpolates between the current position and the interpolation between the last lockstep position and the current lockstep position
                    //LerpTime = time passed since last simulation frame
                    //LerpDamping = special value calculated based on Time.deltaTime for the extra layer of interpolation
                    PositionalTransform.position = 
                        Vector3.Lerp(lastVisualPos, _visualPosition, PhysicsManager.LerpTime);
                
                }
            }
            const float rotationLerpDamping = 1f;
            if (CanSetVisualRotation && RotationalTransform != null)
            {
                if (SetRotationBuffer)
                {
                    RotationalTransform.rotation =

                            Quaternion.Lerp(lastVisualRot, visualRot, PhysicsManager.LerpTime);
                    SetRotationBuffer = PhysicsManager.LerpTime < 1f;

                }
            }
        }

        public void LerpOverReset()
        {
            
            if (CanSetVisualRotation)
            {
                if (SetRotationBuffer)
                {
                    RotationalTransform.rotation = visualRot;
                    SetRotationBuffer = false;
                }
            }
            if (this.CanSetVisualPosition)
            {
                if (this.SetPositionBuffer)
                {
                    PositionalTransform.position = this._visualPosition;
                    SetPositionBuffer = false;
                }
            }
        }

	
		
        public void Rotate(long cos, long sin)
        {

            _rotation.Rotate(cos, sin);
            RotationChanged = true;
        }

        public void SetRotation(long x, long y)
        {
            _rotation = new Vector2d(x, y);
            RotationChanged = true;
        }

        public override int GetHashCode()
        {
            return ID;
        }

        public void Deactivate()
        {
            Partition.UpdateObject(this, false);
            PhysicsManager.Dessimilate(this);
        }

        public bool HeightOverlaps(long heightPos)
        {
            return heightPos >= HeightMin && heightPos <= HeightMax;
        }

        public bool HeightOverlaps(long heightMin, long heightMax)
        {
            return heightMax >= HeightMin && heightMin <= HeightMax;
        }




        long GetCeiledSnap(long f, long snap)
        {
            return (f + snap - 1) / snap * snap;
        }

        long GetFlooredSnap(long f, long snap)
        {
            return (f / snap) * snap;
        }

        public void GetCoveredSnappedPositions(long snapSpacing, FastList<Vector2d> output)
        {
            long referenceX = 0,
            referenceY = 0;
            long xmin = GetFlooredSnap(this.XMin - FixedMath.Half, snapSpacing);
            long ymin = GetFlooredSnap(this.YMin - FixedMath.Half, snapSpacing);

            long xmax = GetCeiledSnap(this.XMax + FixedMath.Half - xmin, snapSpacing) + xmin;
            long ymax = GetCeiledSnap(this.YMax + FixedMath.Half - ymin, snapSpacing) + ymin;
            //Used for getting snapped positions this body covered
            for (long x = xmin; x < xmax; x += snapSpacing)
            {
                for (long y = ymin; y < ymax; y += snapSpacing)
                {
                    Vector2d checkPos = new Vector2d(x, y);
                    if (IsPositionCovered(checkPos))
                    {
                        output.Add(checkPos);
                    }
                }
            }
        }

        public bool IsPositionCovered(Vector2d position)
        {
            //Checks if this body covers a position

            //Different techniques for different shapes
            switch (this.Shape)
            {
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
                case ColliderType.Polygon:
                    for (int i = this.EdgeNorms.Length - 1; i >= 0; i--)
                    {
                        Vector2d norm = this.EdgeNorms [i];
                        long posProj = norm.Dot(position);
                        long polyMin, polyMax;
                        CollisionPair.ProjectPolygon(norm.x, norm.y, this, out polyMin, out polyMax);
                        if (posProj >= polyMin && posProj <= polyMax)
                        {

                        } else
                        {
                            return false;
                        }
                    }
                    return true;
                    break;
            }


            return false;
        }

        public void SetHeight(long newHeight)
        {
            Height = newHeight;
            this.HeightPosChanged = true;
        }

        void Reset()
        {
            this._positionalTransform = this.transform;
            this._rotationalTransform = this.transform;
        }

        void OnDrawGizmos()
        {
            //return;
            //Don't draw gizmos before initialization
            if (Application.isPlaying == false)
                return;
            switch (this.Shape)
            {
                case ColliderType.Circle:
                    Gizmos.DrawWireSphere(this._position.ToVector3(this.HeightPos.ToFloat()), this.Radius.ToFloat());
                    break;
                case ColliderType.AABox:
                    Gizmos.DrawWireCube(
                        this._position.ToVector3(this.HeightPos.ToFloat() + this.Height.ToFloat() / 2),
                        new Vector3(this.HalfWidth.ToFloat() * 2, this.Height.ToFloat(), this.HalfHeight.ToFloat() * 2));
                    break;
                case ColliderType.Polygon:
                    if (RealPoints.Length > 1)
                    {
                        for (int i = 0; i < this.RealPoints.Length; i++)
                        {
                            Gizmos.DrawLine(this.RealPoints [i].ToVector3(), this.RealPoints [i + 1 < RealPoints.Length ? i + 1 : 0].ToVector3());
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Returns 0 if not implemented or invalid.
        /// </summary>
        /// <value>The size of the grid square.</value>
        public long SquareSize
        {
            get
            {
                switch (this.Shape)
                {
                    case ColliderType.Circle:
                        return this.Radius;
                        break;
                    case ColliderType.AABox:
                        if (this.HalfWidth > this.HalfHeight)
                            return HalfWidth;
                        else
                            return HalfHeight;
                        break;
                }
                return 0;
            }
        }

        FastList<UnityEngine.Coroutine> flashRoutines = new FastList<UnityEngine.Coroutine>();

        public void TestFlash()
        {
            flashRoutines.Add(base.StartCoroutine(_TestFlash()));
        }

        private System.Collections.IEnumerator _TestFlash()
        {
            foreach (UnityEngine.Coroutine co in flashRoutines)
            {
                if (co != null)
                    base.StopCoroutine(co);
            }
            flashRoutines.Clear();


            Renderer ren = this.GetComponentInChildren<Renderer>();

            if (ren != null)
            {
                Color col = Color.white;
                ren.material.color = Color.red;
                yield return null;
                yield return new WaitForSeconds(.08f);
                ren.material.color = col;
            }
            yield break;
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