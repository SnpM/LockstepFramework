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
		//
		// Static Fields
		//
		private static FastList<LSAgent> outputAgents = new FastList<LSAgent> ();
		
		private static Vector2d agentPos;
		
		private static Vector3 newPos;
		
		private static Vector2d tempDirection;
		
		private const float Gravity = -9.8f;
		
		private const int defaultMaxDuration = LockstepManager.FrameRate * 16;
		
		private static Vector2d difference;
		
		private const float heightSpeed = 4f;
		
		//
		// Fields
		//
		private GameObject cachedGameObject;
		
		private Transform cachedTransform;
		
		public Vector2d Position;
		
		[FixedNumber, SerializeField]
		public long _speed;
		
		private int CountDown;
		
		public float CurrentHeight;
		
		private Vector2d targetDirection;
		
		private Vector3 arcStartVelocity;
		
		private Vector2d lastDirection;
		
		private long speedPerFrame;
		
		private float timeToHit;
		
		private float HeightSpeed;
		
		private float arcStartVerticalSpeed;
		
		private float arcStartHeight;
		
		[SerializeField]
		private int _delay;

		[SerializeField]
		private bool _attachEndEffectToTarget;
		public bool AttachEndEffectToTarget {get {return _attachEndEffectToTarget;}}
		[SerializeField,DataCode ("Effects")]
		private string _endEffect;
		public string EndEffect {get {return _endEffect;}}
		
		public bool CanRotate = true;

		[SerializeField]
		private TrajectoryType _trajectoryBehavior;
		public TrajectoryType Trajectory {get {return _trajectoryBehavior;}}

        [SerializeField,DataCode ("Effects")]
        private string _startEffect;
        public string StartEffect {get {return _startEffect;}}
		
		[SerializeField]
		private bool _channeled;
		public bool Channeled {get {return _channeled;}}
		
		public bool IsActive;
		
		[Header ("Circumstantial Settings")]
		public bool UseEffects;
		
		[SerializeField]
		private bool _canVisualize = true;
		public bool CanVisualize {get {return _canVisualize;}}
		
		[SerializeField]
		private float _interpolationRate = 8f;

		[SerializeField]
		private long _exclusiveDamageModifier = FixedMath.One;

		[SerializeField]
		private AgentTag _exclusiveTargetType;

		[SerializeField]
		private TargetingType _targetingBehavior;
		public TargetingType TargetingBehavior {get {return _targetingBehavior;}}
		
		[SerializeField]
		public PlatformType _targetPlatform;
		
		[SerializeField]
		public long _damage = 65536L;

		[SerializeField]
		private DamageType _damageBehavior;
		public DamageType DamageBehavior {get {return _damageBehavior;}}
		
		[SerializeField]
		private float _arcHeight = 8f;
		
		[SerializeField]
		private long _angle = 32768L;
		
		[SerializeField]
		private long _radius = 131072L;
		
		//
		// Properties
		//

		public uint SpawnVersion {get; private set;}

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
		
		public float ArcHeight {get;set;}

		
		public long Damage{get;set;}
		
		public int Delay{get;set;}
		
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
		{get;set;}
		
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
		
		public float TargetHeight
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

        public int GetStateHash () {
            int hash = 13;
            hash ^= Position.GetStateHash ();
            return hash;
        }
		
		//
		// Static Methods
		//
		private void ApplyArea (Vector2d center, long radius, Action<LSAgent> apply)
		{
            LSProjectile.Scan (center, radius, this.Source, Source.GetAllegiance(this.Target));
			long num = radius * radius;
			for (int i = 0; i < LSProjectile.outputAgents.Count; i++)
			{
				LSAgent lSAgent = LSProjectile.outputAgents [i];
                if (lSAgent.Body._position.FastDistance (center.x, center.y) < num)
				{
					apply (lSAgent);
				}
			}
		}
		
		private void ApplyCone (Vector2d center, Vector2d rotation, long radius, long angle, Action<LSAgent> apply, PlatformType targetPlatform)
		{
            LSProjectile.Scan (center, radius, this.Source,this.Source.GetAllegiance(this.Target));
			long num = radius * radius;
			long num2 = angle * angle >> 16;
			for (int i = 0; i < LSProjectile.outputAgents.Count; i++)
			{
				LSAgent lSAgent = LSProjectile.outputAgents [i];
                LSProjectile.agentPos = lSAgent.Body._position;
				LSProjectile.difference = LSProjectile.agentPos - center;
				long num3 = LSProjectile.difference.FastMagnitude ();
				if (num3 <= num && LSProjectile.difference.Dot (rotation.x, rotation.y) > 0L)
				{
					num3 >>= 16;
					long num4 = rotation.Cross (LSProjectile.difference.x, LSProjectile.difference.y);
					num4 *= num4;
					num4 >>= 16;
					if (num4 < num2 * num3 >> 16)
					{
						apply (lSAgent);
					}
				}
			}
		}
		private bool CheckCollision () {
			if (Target.Healther.Protected)
			{
				return CheckCollision (Target.Healther.CoveringShield.Agent.Body);
			}
			return CheckCollision(Target.Body);
		}
		private bool CheckCollision (LSBody target)
		{
            return target._position.FastDistance (Position.x, Position.y) <= target.FastRadius;
		}
		

		private static IEnumerable<LSAgent> Scan (Vector2d center, long radius, LSAgent sourceAgent, AllegianceType targetAllegiance)
		{
			int gridX;
			int gridY;
			GridManager.GetScanCoordinates (center.x, center.y, out gridX, out gridY);
            foreach (LSAgent agent in InfluenceManager.ScanAll (gridX, gridY, InfluenceManager.GenerateDeltaCount (radius), sourceAgent, targetAllegiance)) {
                yield return agent;
            }
		}
		

		//
		// Methods
		//
		private void SetupCachedActions () {
			AllButFriendlyAction = AllButFriendly;
			DealDamageAction = DealDamage;
		}
		public Func<LSAgent,bool> AllButFriendlyAction {get; private set;}
		public Action<LSAgent> DealDamageAction {get; private set;}
		public bool AllButFriendly (LSAgent other) {

			return this.Source.GetAllegiance (other) != AllegianceType.Friendly;
		}
		
		public void Deactivate ()
		{
			SpawnVersion = 0;
			this.SourceVersion = 0u;
			this.TargetVersion = 0u;
			this.IsActive = false;
			if (cachedGameObject .IsNotNull ())
			this.cachedGameObject.SetActive (false);
			if (this.cachedTransform .IsNotNull ())
			{
				this.cachedTransform.parent = null;
			}
			if (this.onDeactivate .IsNotNull ())
			{
				this.onDeactivate.Invoke ();
			}
		}

		public bool IsExclusiveTarget (AgentTag AgentTag) {
			return this.ExclusiveTargetType != AgentTag.None && AgentTag == this.ExclusiveTargetType;
		}
		public long CheckExclusiveDamage (AgentTag AgentTag) {
			return IsExclusiveTarget (AgentTag) ? Damage.Mul (this.ExclusiveDamageModifier) : Damage;
		}
		
		private void DealDamage (LSAgent agent)
		{
			agent.Healther.TakeProjectile (this);
		}
		
		private void Hit ()
		{
			if (this.DamageBehavior == DamageType.Single && this.Target.SpawnVersion != this.TargetVersion)
			{
				ProjectileManager.EndProjectile (this);
				return;
			}
			this.OnHit ();
			if (this.onHit .IsNotNull ())
			{
				this.onHit.Invoke ();
			}
			if (this.UseEffects)
			{
				if (this.AttachEndEffectToTarget)
				{
					LSEffect lSEffect = EffectManager.CreateEffect (this.EndEffect);
					lSEffect.CachedTransform.parent = this.Target.VisualCenter;
					lSEffect.CachedTransform.localPosition = Vector3.up;
					lSEffect.Initialize ();
				}
				else
				{
					DamageType damageBehavior = this.DamageBehavior;
					if (damageBehavior != DamageType.Single)
					{
						if (damageBehavior == DamageType.Area)
						{
							EffectManager.LazyCreateEffect (this.EndEffect, base.transform.position, this.cachedTransform.rotation);
						}
					}
					else
					{
						EffectManager.LazyCreateEffect (this.EndEffect, this.Target.CachedTransform.position, this.cachedTransform.rotation);
					}
				}
			}
			ProjectileManager.EndProjectile (this);
		}
		
		public void Initialize (int id, LSAgent source, LSAgent target)
		{
			this.ResetVariables ();
			this.HeightReached = false;
			this.IsActive = true;
			this.ID = id;
			this.Source = source;
			this.Target = target;
			this.TargetVersion = this.Target.SpawnVersion;
			this.SourceVersion = this.Source.SpawnVersion;
			this.cachedGameObject.SetActiveIfNot (true);
			this.AliveTime = 0;
			if (Source.Scanner .IsNotNull ()) {
				this.Position = Source.Scanner.ProjectileOffset;
				this.Position.RotateInverse (source.Body.Rotation.x,source.Body.Rotation.y);
				this.Position += this.Source.Body._position;
				this.CurrentHeight = this.Source.CachedTransform.position.y + source.Scanner.ProjectileHeightOffset;
			}
			else {
				this.Position = this.Source.Body._position;
			}

			this.TargetPosition = this.Target.Body._position;
			this.TargetHeight = this.Target.CachedTransform.position.y;
			DamageType damageBehavior = this.DamageBehavior;
			if (damageBehavior == DamageType.Cone)
			{
				this.Radius = this.Source.Scanner.Range + 65536L;
			}
		}
		
		public void LateInit ()
		{
			long f = this.Position.Distance (this.TargetPosition);
			this.timeToHit = f.ToFloat () / this.Speed.ToFloat ();
			if (this.TargetingBehavior != TargetingType.Timed)
			{
				this.cachedTransform.position = this.Position.ToVector3 (this.CurrentHeight);
				this.speedPerFrame = this.Speed / 32L;
			}
			TrajectoryType trajectory = this.Trajectory;
			if (trajectory != TrajectoryType.Linear)
			{
				if (trajectory == TrajectoryType.Arc)
				{
					this.arcStartHeight = this.CurrentHeight;
					this.arcStartVerticalSpeed = (this.TargetHeight - this.CurrentHeight) / this.timeToHit - -9.8f * this.timeToHit;
					Vector2 vector = (this.TargetPosition.ToVector2 () - this.Position.ToVector2 ()) / this.timeToHit;
					this.arcStartVelocity = new Vector3 (vector.x, this.arcStartVerticalSpeed, vector.y);
				}
			}
			else
			{
				this.HeightSpeed = (this.TargetHeight - this.CurrentHeight) / this.timeToHit;
			}
			switch (this.TargetingBehavior)
			{
			case TargetingType.Timed:
				if (this.Delay == 0)
				{
					this.CountDown--;
					this.Hit ();
				}
				else
				{
					this.CountDown = this.Delay;
				}
				break;
			case TargetingType.Positional:
				this.targetDirection = this.TargetPosition - this.Position;
				this.targetDirection.Normalize ();
				this.targetDirection *= this.speedPerFrame;
				if (this.CanRotate)
				{
					this.cachedTransform.LookAt (this.TargetPosition.ToVector3 (this.TargetHeight));
				}
				break;
			}
			if (this.onInitialize .IsNotNull ())
			{
				this.onInitialize.Invoke ();
			}
			EffectManager.LazyCreateEffect (this.StartEffect, this.Source.CachedTransform.position);
		}
		
		private void OnHit ()
		{
			switch (this.DamageBehavior)
			{
			case DamageType.Single:
				this.DealDamage (Target);
				break;
			case DamageType.Area:
				ApplyArea (this.TargetPosition, this.Radius, this.DealDamageAction);
				break;
			case DamageType.Cone:
				ApplyCone (this.Position, this.Source.Body.Rotation, this.Radius, this.Angle, DealDamageAction, this.TargetPlatform);
				break;
			}
		}
		
		private void ResetDamage ()
		{
			this.Radius = this._radius;
			this.Angle = this._angle;
			this.Damage = this._damage;
			this.ExclusiveTargetType = this._exclusiveTargetType;
			this.ExclusiveDamageModifier = this._exclusiveDamageModifier;
			this.TargetPlatform = this._targetPlatform;
		}
		
		private void ResetEffects ()
		{
		}
		
		private void ResetHelpers ()
		{
			this.lastDirection = Vector2d.zero;
			this.targetDirection = Vector2d.zero;
		}
		
		private void ResetTargeting ()
		{
			this.Delay = this._delay;
			this.Speed = this._speed;
		}
		
		private void ResetTrajectory ()
		{
			this.ArcHeight = this._arcHeight;
			this.InterpolationRate = this._interpolationRate;
		}
		
		private void ResetVariables ()
		{
			this.ResetEffects ();
			this.ResetTrajectory ();
			this.ResetDamage ();
			this.ResetTargeting ();
			this.ResetHelpers ();
		}
        public ProjectileDataItem MyData {get; private set;}
		public void Setup (ProjectileDataItem dataItem)
		{
			SetupCachedActions ();
			this.SpawnVersion = 1u;
            this.MyData = dataItem;
			this.MyProjCode = dataItem.Name;
			this.cachedGameObject = base.gameObject;
			this.cachedTransform = base.transform;
			GameObject.DontDestroyOnLoad (this.cachedGameObject);
			if (this.onSetup .IsNotNull ())
			{
				this.onSetup.Invoke ();
			}
		}
		
		public void Simulate ()
		{
			this.AliveTime++;
			if (this.AliveTime > this.MaxDuration || (this.Channeled && (this.Source.SpawnVersion != this.SourceVersion || !this.Source.Scanner.IsCasting)))
			{
				ProjectileManager.EndProjectile (this);
				return;
			}
			switch (this.TargetingBehavior)
			{
			case TargetingType.Timed:
				this.CountDown--;
				if (this.CountDown == 0)
				{
					this.Hit ();
				}
				break;
			case TargetingType.Seeking:
				if (this.CheckCollision ())
				{
					this.TargetPosition = this.Target.Body._position;
					this.Hit ();
				}
				else
				{
					LSProjectile.tempDirection = this.Target.Body._position - this.Position;
					if (LSProjectile.tempDirection.Dot (this.lastDirection.x, this.lastDirection.y) < 0L)
					{
						this.TargetPosition = this.Target.Body._position;
						this.Hit ();
					}
					else
					{
						LSProjectile.tempDirection.Normalize ();
						this.lastDirection = LSProjectile.tempDirection;
						LSProjectile.tempDirection *= this.speedPerFrame;
						this.Position += LSProjectile.tempDirection;
					}
				}
				break;
			case TargetingType.Positional:
				this.Position += this.targetDirection;
				LSProjectile.tempDirection = this.TargetPosition - this.Position;
				if (this.targetDirection.Dot (LSProjectile.tempDirection.x, LSProjectile.tempDirection.y) < 0L)
				{
					this.Hit ();
				}
				break;
			}
		}
		
		public void Visualize ()
		{
			if (this.IsActive)
			{
				if (this.CanVisualize)
				{
					TargetingType targetingBehavior = this.TargetingBehavior;
					if (targetingBehavior != TargetingType.Timed)
					{
						if (targetingBehavior == TargetingType.Seeking)
						{
							if (this.Target.Body.SetPositionBuffer)
							{
								this.TargetHeight = this.Target.CachedTransform.position.y;
							}
						}
					}
					switch (this.Trajectory)
					{
					case TrajectoryType.Linear:
						if (!this.HeightReached)
						{
							this.CurrentHeight += this.HeightSpeed * Time.deltaTime;
							if (this.HeightSpeed < 0f)
							{
								if (this.CurrentHeight <= this.TargetHeight)
								{
									this.HeightReached = true;
									this.CurrentHeight = this.TargetHeight;
								}
							}
							else
							{
								if (this.CurrentHeight >= this.TargetHeight)
								{
									this.HeightReached = true;
									this.CurrentHeight = this.TargetHeight;
								}
							}
						}
						if (this.CanRotate)
						{
							this.cachedTransform.LookAt (this.Target.CachedTransform.position);
						}
						break;
					case TrajectoryType.Arc:
					{
						float num = (float)this.AliveTime / 32f;
						this.CurrentHeight = this.arcStartHeight + num * this.arcStartVerticalSpeed + -9.8f * num * num;
						if (this.CanRotate)
						{
							Vector3 vector = this.arcStartVelocity + -9.8f * num * Vector3.up;
						}
						break;
					}
					case TrajectoryType.Interpolated:
						if (!this.HeightReached)
						{
							this.CurrentHeight = Mathf.Lerp (this.CurrentHeight, this.TargetHeight, this.InterpolationRate * Time.deltaTime);
						}
						if (this.CanRotate)
						{
							this.cachedTransform.LookAt (this.Target.VisualCenter);
						}
						break;
					}
					LSProjectile.newPos = this.Position.ToVector3 (this.CurrentHeight);
					this.cachedTransform.position = LSProjectile.newPos;
				}
				if (this.onVisualize .IsNotNull ())
				{
					this.onVisualize.Invoke ();
				}
			}
			else
			{
				this.cachedGameObject.SetActiveIfNot (false);
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
