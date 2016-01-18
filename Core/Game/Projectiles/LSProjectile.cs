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
        private static FastList<LSAgent> outputAgents = new FastList<LSAgent>();
		
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
		
        public Vector2d Position;
		
        [FixedNumber, SerializeField]
        public long _speed;
		
        private int CountDown;
		
        public long CurrentHeight;

        private Vector2d Velocity { get; set; }

        private Vector2d Direction { get; set; }

        private long Slope { get; set; }

        private Vector2d lastDirection;
		
        private long speedPerFrame;
				
        private long HeightSpeed;
		
		private float arcStartVerticalSpeed;

		private float arcStartHeight;

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
		
        [Header("Circumstantial Settings")]
        public bool UseEffects;
		
        [SerializeField]
        private bool _canVisualize = true;

        public bool CanVisualize { get { return _canVisualize; } }

        [SerializeField]
        private float _interpolationRate = 8f;

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

        [SerializeField]
        private long _angle = 32768L;
		
        [SerializeField]
        private long _radius = 131072L;
		
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
            get;
            set;
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

        public float InterpolationRate
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
            get;
            set;
        }

        public LSAgent Source
        {
            get;
            set;
        }

        private uint SourceVersion
        {
            get;
            set;
        }

        public long Speed
		{ get; set; }

        public Vector3 StartPoint
        {
            get
            {
                return this.Source.CachedTransform.position;
            }
        }

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

        private Action<LSAgent> HitEffect { get; set; }

        public int GetStateHash()
        {
            int hash = 13;
            hash ^= Position.GetStateHash();
            return hash;
        }
		
        //
        // Static Methods
        //
        private void ApplyArea(Vector2d center, long radius, Action<LSAgent> apply)
        {
            LSProjectile.Scan(center, radius, this.Source, Source.GetAllegiance(this.Target));
            long num = radius * radius;
            for (int i = 0; i < LSProjectile.outputAgents.Count; i++)
            {
                LSAgent lSAgent = LSProjectile.outputAgents [i];
                if (lSAgent.Body._position.FastDistance(center.x, center.y) < num)
                {
                    apply(lSAgent);
                }
            }
        }

        private void ApplyCone(Vector2d center, Vector2d rotation, long radius, long angle, Action<LSAgent> apply, PlatformType targetPlatform)
        {
            LSProjectile.Scan(center, radius, this.Source, this.Source.GetAllegiance(this.Target));
            long num = radius * radius;
            long num2 = angle * angle >> 16;
            for (int i = 0; i < LSProjectile.outputAgents.Count; i++)
            {
                LSAgent lSAgent = LSProjectile.outputAgents [i];
                LSProjectile.agentPos = lSAgent.Body._position;
                LSProjectile.difference = LSProjectile.agentPos - center;
                long num3 = LSProjectile.difference.FastMagnitude();
                if (num3 <= num && LSProjectile.difference.Dot(rotation.x, rotation.y) > 0L)
                {
                    num3 >>= 16;
                    long num4 = rotation.Cross(LSProjectile.difference.x, LSProjectile.difference.y);
                    num4 *= num4;
                    num4 >>= 16;
                    if (num4 < num2 * num3 >> 16)
                    {
                        apply(lSAgent);
                    }
                }
            }
        }

        private bool CheckCollision()
        {
            if (Target.Healther.Protected)
            {
                return CheckCollision(Target.Healther.CoveringShield.Agent.Body);
            }
            return CheckCollision(Target.Body);
        }

        private bool CheckCollision(LSBody target)
        {
            return target._position.FastDistance(Position.x, Position.y) <= target.FastRadius;
        }


        private static IEnumerable<LSAgent> Scan(Vector2d center, long radius, LSAgent sourceAgent, AllegianceType targetAllegiance)
        {
            int gridX;
            int gridY;
            GridManager.GetScanCoordinates(center.x, center.y, out gridX, out gridY);
            foreach (LSAgent agent in InfluenceManager.ScanAll (gridX, gridY, InfluenceManager.GenerateDeltaCount (radius), sourceAgent, targetAllegiance))
            {
                yield return agent;
            }
        }
		

        //
        // Methods
        //
        private void SetupCachedActions()
        {
            AllButFriendlyAction = AllButFriendly;
        }

        public Func<LSAgent,bool> AllButFriendlyAction { get; private set; }


        public bool AllButFriendly(LSAgent other)
        {

            return this.Source.GetAllegiance(other) != AllegianceType.Friendly;
        }

        public void Deactivate()
        {
            SpawnVersion = 0;
            this.SourceVersion = 0u;
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
                    lSEffect.Initialize();
                } else
                {
                    if (this.HitBehavior != HitType.Single)
                    {
                        if (this.HitBehavior == HitType.Area)
                        {
                            EffectManager.LazyCreateEffect(this.EndEffect, base.transform.position, this.cachedTransform.rotation);
                        }
                    } else
                    {
                        EffectManager.LazyCreateEffect(this.EndEffect, this.Target.CachedTransform.position, this.cachedTransform.rotation);
                    }
                }
            }
            ProjectileManager.EndProjectile(this);
        }

        internal void Prepare(int id, LSAgent source, Vector2dHeight projectileOffset, Action<LSAgent> hitEffect)
        {
            this.IsActive = true;
            this.cachedGameObject.SetActiveIfNot(true);

            this.Source = source;
            this.ResetVariables();

            this.Position = projectileOffset.ToVector2d();
            this.Position.RotateInverse(source.Body._rotation.x, source.Body._rotation.y);
            this.Position += this.Source.Body._position;
            this.CurrentHeight = this.Source.Body.HeightPos + projectileOffset.Height;

            this.HitEffect = hitEffect;
            this.ID = id;
            this.Source = source;

            this.AliveTime = 0;
        }

        public void InitializeHoming(LSAgent target)
        {
            this.HeightReached = false;
            this.Target = target;
            this.TargetVersion = this.Target.SpawnVersion;
            this.SourceVersion = this.Source.SpawnVersion;

            this.TargetPosition = this.Target.Body._position;
            this.TargetHeight = this.Target.Body.HeightPos;
        }

        public void InitializeTimed(int frameTime)
        {
            this.Delay = frameTime;
        }

        public void InitializeFree(Vector2d direction, long slope)
        {
            this.Direction = direction;
            this.Slope = slope;
        }

        public void LateInit()
        {
            long f = this.Position.Distance(this.TargetPosition);
            long timeToHit = f.Div(this.Speed);
            if (this.TargetingBehavior != TargetingType.Timed)
            {
                this.cachedTransform.position = this.Position.ToVector3(this.CurrentHeight);
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
                case TargetingType.Homing:
					this.arcStartHeight = this.CurrentHeight;
					float visualTimeToHit = f.ToFloat () / this.Speed.ToFloat ();
					this.arcStartVerticalSpeed = (this.TargetHeight - this.CurrentHeight) / visualTimeToHit - -9.8f * visualTimeToHit;
                    break;
                case TargetingType.Free:
                    this.Velocity = this.Direction * this.speedPerFrame;
                    this.HeightSpeed = this.Slope.Mul(this.speedPerFrame);
                    if (this.CanRotate)
                    {
                        //this.cachedTransform.LookAt();
                    }
                    break;
            }
            if (this.onInitialize.IsNotNull())
            {
                this.onInitialize.Invoke();
            }
            EffectManager.LazyCreateEffect(this.StartEffect, this.Source.CachedTransform.position);
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
                        this.HitEffect(Target);
                        break;
                    case HitType.Area:
                        ApplyArea(this.TargetPosition, this.Radius, this.HitEffect);
                        break;
                    case HitType.Cone:
                        ApplyCone(this.Position, this.Source.Body._rotation, this.Radius, this.Angle, this.HitEffect, this.TargetPlatform);
                        break;
                }
            }
        }

        private void ResetHit()
        {
            this.Radius = this._radius;
            this.Angle = this._angle;
            this.ExclusiveTargetType = this._exclusiveTargetType;
            this.TargetPlatform = this._targetPlatform;
        }

        private void ResetEffects()
        {
        }

        private void ResetHelpers()
        {
            this.lastDirection = Vector2d.zero;
            this.Velocity = Vector2d.zero;
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

        public ProjectileDataItem MyData { get; private set; }

        public void Setup(ProjectileDataItem dataItem)
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
                        LSProjectile.tempDirection = this.Target.Body._position - this.Position;
                        if (LSProjectile.tempDirection.Dot(this.lastDirection.x, this.lastDirection.y) < 0L)
                        {
                            this.TargetPosition = this.Target.Body._position;
                            this.Hit();
                        } else
                        {
                            LSProjectile.tempDirection.Normalize();
                            this.lastDirection = LSProjectile.tempDirection;
                            LSProjectile.tempDirection *= this.speedPerFrame;
                            this.Position += LSProjectile.tempDirection;
                        }
                    }
                    break;
                case TargetingType.Free:
                    
                    Vector2d nextPosition = this.Position + this.Velocity;
                    HitBodies.FastClear();
                    foreach (LSBody body in Raycaster.RaycastAll(this.Position,nextPosition,CurrentHeight,this.Slope))
                    {
                        if (body.ID != Source.Body.ID)
                            HitBodies.Add(body);
                    }
                    if (HitBodies.Count > 0)
                        Hit();
                    this.Position = nextPosition;
                    this.CurrentHeight += this.HeightSpeed;

                    break;
            }
        }


        public void Visualize()
        {
            if (this.IsActive)
            {
                if (this.CanVisualize)
                {
					if (this.TargetingBehavior == TargetingType.Homing && _visualArc)
					{
						float num = (float)this.AliveTime / 32f;
						float height = this.arcStartHeight + num * this.arcStartVerticalSpeed + -9.8f * num * num;
						LSProjectile.newPos = this.Position.ToVector3(height);
						this.cachedTransform.position = LSProjectile.newPos;
					} else
					{
                    	LSProjectile.newPos = this.Position.ToVector3(this.CurrentHeight.ToFloat());
                    	this.cachedTransform.position = LSProjectile.newPos;
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
