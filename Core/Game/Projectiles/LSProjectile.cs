using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Lockstep.Data;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using Lockstep.Integration;
#endif
namespace Lockstep
{
    public sealed class LSProjectile : CerealBehaviour
    {
        private const long Gravity = FixedMath.One * 98 / 10;
        //
        // Static Fields
        //
        private static Vector2d agentPos;
		
        private static Vector3 newPos;
		
        private static Vector2d tempDirection;
				
        private const int defaultMaxDuration = LockstepManager.FrameRate * 16;
		
        private static Vector2d difference;
				
        //
        // Fields
        //
        private GameObject cachedGameObject;
		
        private Transform cachedTransform;
		
        public Vector3d Position;
		
        [FixedNumber, SerializeField]
        public long _speed;
		
        private int CountDown;

        public Vector3d Velocity { get; private set; }

        private Vector3d Direction { get; set; }


        private Vector2d lastDirection;
		
        private long speedPerFrame;
				
        private long HeightSpeed;
		
        private long arcStartVerticalSpeed;

        private long arcStartHeight;
        private long linearHeightSpeed;

        [SerializeField]
        private bool _visualArc;
				
        [SerializeField]
        private int _delay;

        [SerializeField]
        private bool _attachEndEffectToTarget;

        public bool AttachEndEffectToTarget { get { return _attachEndEffectToTarget; } }

        [SerializeField,DataCode("Effects")]
        private string _endEffect;

        public string EndEffect { get { return _endEffect; } }

        public bool CanRotate = true;

        [SerializeField,DataCode("Effects")]
        private string _startEffect;

        public string StartEffect { get { return _startEffect; } }

        public bool IsActive;
		
        public bool UseEffects;
		
        [SerializeField]
        private bool _canVisualize = true;

        public bool CanVisualize { get { return _canVisualize; } }

        [SerializeField]
        private long _interpolationRate = FixedMath.One * 8;

        [SerializeField]
        private AgentTag _exclusiveTargetType;

        [SerializeField]
        private TargetingType _targetingBehavior;

        public TargetingType TargetingBehavior { get { return _targetingBehavior; } }

        [SerializeField]
        public PlatformType _targetPlatform;

        [SerializeField]
        private HitType _hitBehavior;

        public HitType HitBehavior { get { return _hitBehavior; } }

        [FixedNumberAngle, SerializeField]
        private long _angle = FixedMath.TenDegrees;
		
        [FixedNumber, SerializeField]
        private long _radius = FixedMath.Create(1);
		
        //
        // Properties
        //

        public uint SpawnVersion { get; private set; }

        public int AliveTime
        {
            get;
            private set;
        }

        public long Angle
        {
            get { return _angle; }
            set { _angle = value; }
        }

        public long Damage{ get; set; }

        public int Delay{ get; set; }

        public Vector3 EndPoint
        {
            get
            {
                return this.Target.CachedTransform.position;
            }
        }

        public long ExclusiveDamageModifier
        {
            get;
            set;
        }

        public AgentTag ExclusiveTargetType
        {
            get;
            set;
        }

        private bool HeightReached
        {
            get;
            set;
        }

        public int ID
        {
            get;
            private set;
        }

        public long InterpolationRate
        {
            get;
            set;
        }

        private int MaxDuration
        {
            get
            {
                return (256 <= this.Delay) ? this.Delay : 256;
            }
        }

        public string MyProjCode
        {
            get;
            private set;
        }

        public long Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        public long Speed
		{ get; set; }

        public LSAgent Target
        {
            get;
            set;
        }

        public long TargetHeight
        {
            get;
            set;
        }

        public PlatformType TargetPlatform
        {
            get;
            set;
        }

        public Vector2d TargetPosition
        {
            get;
            set;
        }

        private uint TargetVersion
        {
            get;
            set;
        }

        public Vector2d Forward { get; set; }

        private Action<LSAgent> HitEffect { get; set; }

        public int GetStateHash()
        {
            int hash = 13;
            hash ^= Position.StateHash;
            return hash;
        }
		
        //
        // Static Methods
        //
        private void ApplyArea(Vector2d center, long radius)
        {
            long num = radius * radius;
            foreach (LSAgent agent in Scan(center, radius))
            {
                if (agent.Body._position.FastDistance(center.x, center.y) < num)
                {
                    this.HitEffect(agent);
                }
            }
        }

        private void ApplyCone(Vector3d center3d, Vector2d forward, long radius, long angle, Action<LSAgent> apply, PlatformType targetPlatform)
        {
            Vector2d center = center3d.ToVector2d();
            long fastRange = radius * radius;
            foreach (LSAgent agent in Scan(center, radius))
            {
                Vector2d agentPos = agent.Body._position;
                Vector2d difference = agentPos - center;

                if (difference.FastMagnitude() > fastRange)
                {
                    continue;
                }
                if (forward.Dot(difference) < 0)
                {
                    continue;
				
                }
                difference.Normalize();

                long cross = forward.Cross(difference).Abs();
                if (cross > angle)
                {
                    continue;
                }
                apply(agent);

            }
        }

        private bool CheckCollision()
        {
            return CheckCollision(Target.Body);
        }

        private bool CheckCollision(LSBody target)
        {
            return target._position.FastDistance(Position.x, Position.y) <= target.FastRadius;
        }


        private IEnumerable<LSAgent> Scan(Vector2d center, long radius)
        {

            foreach (LSAgent agent in InfluenceManager.ScanAll (
                center,
                radius,
                this.AgentConditional,
                this.BucketConditional))
            {
                yield return agent;
            }
        }

        private void SetupCachedActions()
        {
        }

        internal void Deactivate()
        {
            SpawnVersion = 0;
            this.TargetVersion = 0u;
            this.IsActive = false;
            if (cachedGameObject.IsNotNull())
                this.cachedGameObject.SetActive(false);
            if (this.cachedTransform.IsNotNull())
            {
                this.cachedTransform.parent = null;
            }
            if (this.onDeactivate.IsNotNull())
            {
                this.onDeactivate.Invoke();
            }
        }

        public bool IsExclusiveTarget(AgentTag AgentTag)
        {
            return this.ExclusiveTargetType != AgentTag.None && AgentTag == this.ExclusiveTargetType;
        }

        public long CheckExclusiveDamage(AgentTag AgentTag)
        {
            return IsExclusiveTarget(AgentTag) ? Damage.Mul(this.ExclusiveDamageModifier) : Damage;
        }

        private void Hit()
        {
            if (this.TargetingBehavior == TargetingType.Homing && this.HitBehavior == HitType.Single && this.Target.SpawnVersion != this.TargetVersion)
            {
                ProjectileManager.EndProjectile(this);
                return;
            }
            this.OnHit();
            if (this.onHit.IsNotNull())
            {
                this.onHit.Invoke();
            }
            if (this.UseEffects)
            {
                if (this.AttachEndEffectToTarget)
                {
                    LSEffect lSEffect = EffectManager.CreateEffect(this.EndEffect);
                    lSEffect.CachedTransform.parent = this.Target.VisualCenter;
                    lSEffect.CachedTransform.localPosition = Vector3.up;
                    lSEffect.CachedTransform.rotation = this.cachedTransform.rotation;
                    lSEffect.Initialize();
                } else
                {

                    {
                        EffectManager.LazyCreateEffect(this.EndEffect, this.cachedTransform.position, this.cachedTransform.rotation);
                    }
                }
            }
            ProjectileManager.EndProjectile(this);
        }

        public Func<byte,bool> BucketConditional { get; private set; }

        public Func<LSAgent,bool> AgentConditional { get; private set; }

        public bool Deterministic { get; private set; }

        internal void Prepare(int id, Vector3d projectilePosition, Func<LSAgent,bool> agentConditional, Func<byte,bool> bucketConditional, Action<LSAgent> hitEffect, bool deterministic)
        {
            this.Deterministic = deterministic;

            this.IsActive = true;
            this.cachedGameObject.SetActiveIfNot(true);

            this.ResetVariables();

            this.Position = projectilePosition;
            this.HitEffect = hitEffect;
            this.ID = id;

            this.AliveTime = 0;

            this.BucketConditional = bucketConditional;
            this.AgentConditional = agentConditional;

        }

        Vector3 bindPositionShift = new Vector3(0, 2, 0);

        public void InitializeHoming(LSAgent target)
        {
            this.HeightReached = false;
            this.Target = target;
            this.TargetVersion = this.Target.SpawnVersion;

            this.TargetPosition = this.Target.Body._position;
            this.TargetHeight = this.Target.Body.HeightPos + this.Target.Body.Height / 2;

            this.cachedTransform.rotation = Quaternion.LookRotation(target.CachedTransform.position - this.Position.ToVector3());
        
        
        }

        public void InitializeTimed()
        {
            
        }

        Func<LSBody,bool> BodyConditional;

        public void InitializeFree(Vector3d direction, Func<LSBody,bool> bodyConditional, bool useGravity = false)
        {
            this.BodyConditional = bodyConditional;
            this.Direction = direction;
            this.Forward = Direction.ToVector2d();

            this.cachedTransform.rotation = Quaternion.LookRotation(direction.ToVector3());
        }

        public void InitializePositional(Vector3d position)
        {
            this.TargetPosition = position.ToVector2d();
            this.TargetHeight = position.z;

        }

        public void UpdatePosition () {
            cachedTransform.rotation = Quaternion.LookRotation(Forward.ToVector3());
            cachedTransform.position = this.Position.ToVector3();
        }

        public void LateInit()
        {
            this.UpdatePosition();

            if (this.TargetingBehavior != TargetingType.Timed)
            {
                this.cachedTransform.position = this.Position.ToVector3();
                this.speedPerFrame = this.Speed / 32L;
            }



            switch (this.TargetingBehavior)
            {
                case TargetingType.Timed:
                    if (this.Delay == 0)
                    {
                        this.CountDown--;
                        this.Hit();
                    } else
                    {
                        this.CountDown = this.Delay;
                    }
                    break;
                case TargetingType.Positional:
                case TargetingType.Homing:
                    long f = this.Position.ToVector2d().Distance(this.TargetPosition);
                    long timeToHit = f.Div(this.Speed);
                    if (this._visualArc)
                    {
                        this.arcStartHeight = this.Position.z;
                        this.arcStartVerticalSpeed = (this.TargetHeight - this.Position.z).Div(timeToHit) + timeToHit.Mul(Gravity);
                    } else
                    {
                        this.linearHeightSpeed = (this.TargetHeight - Position.z).Div(timeToHit).Abs();
                    }
                    Forward = TargetPosition - this.Position;
                    Forward.Normalize();
                    break;
                case TargetingType.Free:

                    Vector3d vel = this.Direction;
                    vel.Mul(speedPerFrame);
                    this.Velocity = vel;
                    if (this.CanRotate)
                    {
                        this.cachedTransform.LookAt(this.Forward.ToVector3());
                    }

                    break;
            }
            if (this.onInitialize.IsNotNull())
            {
                this.onInitialize.Invoke();
            }

            if (UseEffects)
                EffectManager.LazyCreateEffect(this.StartEffect, this.Position.ToVector3(), this.cachedTransform.rotation);
        }

        private void OnHit()
        {
            if (this.TargetingBehavior == TargetingType.Free)
            {
                switch (this.HitBehavior)
                {
                    case HitType.Single:
                        this.HitBodies [0].TestFlash();
                        break;
                }
            } else
            {
                switch (this.HitBehavior)
                {
                    case HitType.Single:
                        if (Target == null)
                        {
                            throw new System.Exception("Cannot use single hit effect without target");
                        }
                        this.HitEffect(Target);
                        break;
                    case HitType.Area:
                        ApplyArea(this.TargetPosition, this.Radius);
                        break;
                    case HitType.Cone:
                        ApplyCone(this.Position, this.Forward, this.Radius, this.Angle, this.HitEffect, this.TargetPlatform);
                        break;
                }
            }
        }

        private void ResetHit()
        {
            this.ExclusiveTargetType = this._exclusiveTargetType;
            this.TargetPlatform = this._targetPlatform;
        }

        private void ResetEffects()
        {
        }

        private void ResetHelpers()
        {
            this.lastDirection = Vector2d.zero;
            this.Velocity = default(Vector3d);
        }

        private void ResetTargeting()
        {
            this.Delay = this._delay;
            this.Speed = this._speed;
        }

        private void ResetTrajectory()
        {
            this.InterpolationRate = this._interpolationRate;
        }

        private void ResetVariables()
        {
            this.ResetEffects();
            this.ResetTrajectory();
            this.ResetHit();
            this.ResetTargeting();
            this.ResetHelpers();
        }

        public IProjectileData MyData { get; private set; }

        public void Setup(IProjectileData dataItem)
        {
            this.SpawnVersion = 1u;
            this.MyData = dataItem;
            this.MyProjCode = dataItem.Name;
            this.cachedGameObject = base.gameObject;
            this.cachedTransform = base.transform;
            GameObject.DontDestroyOnLoad(this.cachedGameObject);
            if (this.onSetup.IsNotNull())
            {
                this.onSetup.Invoke();
            }
        }

        private FastList<LSBody> HitBodies = new FastList<LSBody>();

        public void Simulate()
        {
            this.AliveTime++;

            if (this.AliveTime > this.MaxDuration)
            {
                ProjectileManager.EndProjectile(this);
                return;
            }
            switch (this.TargetingBehavior)
            {
                case TargetingType.Timed:
                    this.CountDown--;
                    if (this.CountDown == 0)
                    {
                        this.Hit();
                    }
                    break;
                case TargetingType.Homing:
                    if (this.CheckCollision())
                    {
                        this.TargetPosition = this.Target.Body._position;
                        this.Hit();
                    } else
                    {
                        TargetPosition = Target.Body.Position;
                        this.TargetHeight = this.Target.Body.HeightPos + Target.Body.Height / 2;

                        MoveToTargetPosition();
                    }
                    break;
                case TargetingType.Free:
                    RaycastMove(this.Velocity);
                    break;
                case TargetingType.Positional:
                    MoveToTargetPosition ();
                    break;
            }
        }

        void MoveToTargetPosition()
        {
            if (this._visualArc)
            {
                long progress = FixedMath.Create(this.AliveTime) / 32;
                long height = this.arcStartHeight + this.arcStartVerticalSpeed.Mul(progress) - Gravity.Mul(progress.Mul(progress));
                this.Position.z = height;
            } else
            {
                this.Position.z = FixedMath.MoveTowards(this.Position.z, TargetHeight, this.linearHeightSpeed);
            }

            LSProjectile.tempDirection = TargetPosition - this.Position.ToVector2d();
            if (LSProjectile.tempDirection.Dot(this.lastDirection.x, this.lastDirection.y) < 0L)
            {
                this.Hit();
            } else
            {
                LSProjectile.tempDirection.Normalize();
                Forward = tempDirection;
                this.lastDirection = LSProjectile.tempDirection;
                LSProjectile.tempDirection *= this.speedPerFrame;
                this.Position.Add(LSProjectile.tempDirection.ToVector3d());
            }
        }

        public void RaycastMove(Vector3d delta)
        {
            #if true
            Vector3d nextPosition = this.Position;
            nextPosition.Add(ref delta);
            HitBodies.FastClear();
            foreach (LSBody body in Raycaster.RaycastAll(this.Position,nextPosition))
            {  
                if (this.BodyConditional(body))
                {
                    HitBodies.Add(body);
                }
            }
            if (HitBodies.Count > 0)
                Hit();
            this.Position = nextPosition;
            #endif
        }

        public void Visualize()
        {
            if (this.IsActive)
            {
                if (this.CanVisualize)
                {
                    
                    LSProjectile.newPos = this.Position.ToVector3();
                    Vector3 shiftVelocity = LSProjectile.newPos - this.cachedTransform.position;
                    this.cachedTransform.position = LSProjectile.newPos;
                    if (shiftVelocity.sqrMagnitude > 0)
                    {
                        this.cachedTransform.rotation = Quaternion.LookRotation(shiftVelocity);
                    }
                }
                if (this.onVisualize.IsNotNull())
                {
                    this.onVisualize.Invoke();
                }
            } else
            {
                this.cachedGameObject.SetActiveIfNot(false);
            }
        }
		
        //
        // Events
        //
        public event Action onDeactivate;
		
        public event Action onHit;
		
        public event Action onInitialize;
		
        public event Action onSetup;
        public event Action onVisualize;

    }
}
