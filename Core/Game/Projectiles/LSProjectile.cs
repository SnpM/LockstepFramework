using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Lockstep.Data;
using System.Collections.Generic;
using FastCollections;

#if UNITY_EDITOR
using UnityEditor;
using Lockstep.Integration;
#endif
namespace Lockstep
{
	public sealed class LSProjectile : CerealBehaviour
	{
		public const int DefaultTickRate = LockstepManager.FrameRate / 4;
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

		public Vector3d Direction { get; set; }


		private Vector2d lastDirection;

		private long speedPerFrame;

		private long HeightSpeed;

		private long arcStartVerticalSpeed;

		private long arcStartHeight;
		private long linearHeightSpeed;

		[SerializeField]
		private bool _visualArc;

		[SerializeField, FrameCount]
		private int _delay;

		[SerializeField]
		private bool _attachEndEffectToTarget;

		public bool AttachEndEffectToTarget { get { return _attachEndEffectToTarget; } }

		[SerializeField, DataCode("Effects"), UnityEngine.Serialization.FormerlySerializedAs("_endEffect")]
		private string _hitFX;

		public string HitFX { get { return _hitFX; } }

		public bool CanRotate = true;

		[SerializeField, DataCode("Effects"), UnityEngine.Serialization.FormerlySerializedAs("_startEffect")]
		private string _startFX;

		public string StartFX { get { return _startFX; } }

		public bool IsActive;

		public bool UseEffects;

		[SerializeField]
		private bool _canVisualize = true;

		public bool CanVisualize { get { return _canVisualize; } }


		[SerializeField]
		private AgentTag _exclusiveTargetType;

		[SerializeField]
		private TargetingType _targetingBehavior;

		public TargetingType TargetingBehavior { get { return _targetingBehavior; } }


		[SerializeField]
		private HitType _hitBehavior;

		public HitType HitBehavior { get { return _hitBehavior; } }

		[FixedNumberAngle, SerializeField]
		private long _angle = FixedMath.TenDegrees;

		[FixedNumber, SerializeField]
		private long _radius = FixedMath.Create(1);


		[SerializeField, FrameCount]
		private int _lastingDuration;

		[SerializeField, FrameCount]
		private int _tickRate = DefaultTickRate;

		//
		// Properties
		//

		public uint SpawnVersion { get; private set; }

		//PAPPS ADDED THIS:	it detaches the children particle effects inside the projectile, and siwtches em off.. REALLY NECESSARY
		public bool DoReleaseChildren = false;

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

		public long Damage { get; set; }

		public int Delay { get; set; }

		public int LastingDuration { get; set; }

		public int TickRate { get; set; }

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
				int minTime = this.Delay + this.LastingDuration;
				return (LockstepManager.FrameRate * 16 <= minTime) ? minTime : LockstepManager.FrameRate * 16;
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
			Scan(center, radius);
			for (int i = 0; i < ScanOutput.Count; i++)
			{
				LSAgent agent = ScanOutput[i];
				if (agent.Body._position.FastDistance(center.x, center.y) < num)
				{
					HitAgent(agent);
				}
			}
		}

		void HitAgent(LSAgent agent)
		{
			if (this.UseEffects && this.AttachEndEffectToTarget)
			{
				LSEffect lSEffect = EffectManager.CreateEffect(this.HitFX);
				lSEffect.CachedTransform.parent = agent.VisualCenter;
				lSEffect.CachedTransform.localPosition = Vector3.up;
				lSEffect.CachedTransform.rotation = this.cachedTransform.rotation;
				lSEffect.Initialize();
			}
			this.HitEffect(agent);
			if (onHitAgent != null)
			{
				onHitAgent(agent);
			}
		}

	private void ApplyCone(Vector3d center3d, Vector2d forward, long radius, long angle)
		{
			Vector2d center = center3d.ToVector2d();
			long fastRange = radius * radius;
			Scan(center, radius);
			for (int i = 0; i < ScanOutput.Count; i++)
			{
				LSAgent agent = ScanOutput[i];
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
				HitAgent(agent);

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


		static FastList<LSAgent> ScanOutput = new FastList<LSAgent>();
		private void Scan(Vector2d center, long radius)
		{
			InfluenceManager.ScanAll(
			center,
			radius,
			this.AgentConditional,
			this.BucketConditional,
				ScanOutput
		);

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

		private void Hit(bool destroy = true)
		{
			if (this.TargetingBehavior == TargetingType.Homing && this.HitBehavior == HitType.Single && this.Target.SpawnVersion != this.TargetVersion)
			{
				ProjectileManager.EndProjectile(this);
				return;
			}
			this.OnHit();
			if (this.onHit.IsNotNull())
			{
				this.onHit();
			}
			if (this.UseEffects)
			{
				if (this.AttachEndEffectToTarget)
				{
					/*
                    LSEffect lSEffect = EffectManager.CreateEffect(this.HitFX);
                    lSEffect.CachedTransform.parent = this.Target.VisualCenter;
                    lSEffect.CachedTransform.localPosition = Vector3.up;
                    lSEffect.CachedTransform.rotation = this.cachedTransform.rotation;
                    lSEffect.Initialize();
                    */
				}
				else
				{

					{
						EffectManager.LazyCreateEffect(this.HitFX, this.cachedTransform.position, this.cachedTransform.rotation);
					}
				}
			}

			if (destroy)
				ProjectileManager.EndProjectile(this);
		}

		public Func<byte, bool> BucketConditional { get; private set; }

		public Func<LSAgent, bool> AgentConditional { get; private set; }

		public bool Deterministic { get; private set; }

		internal void Prepare(int id, Vector3d projectilePosition, Func<LSAgent, bool> agentConditional, Func<byte, bool> bucketConditional, Action<LSAgent> hitEffect, bool deterministic)
		{
			this.Deterministic = deterministic;

			this.IsActive = true;
			this.cachedGameObject.SetActiveIfNot(true);

			this.ResetVariables();

			this.Position = projectilePosition;
			this.HitEffect = hitEffect;
			this.ID = id;

			this.AliveTime = 0;
			this.IsLasting = false;


			this.BucketConditional = bucketConditional;

			this.AgentConditional = agentConditional;

			Forward = Vector2d.up;
		}

		public void InitializeHoming(LSAgent target)
		{
			this.HeightReached = false;
			this.Target = target;
			this.TargetVersion = this.Target.SpawnVersion;

			this.TargetPosition = this.Target.Body._position;
			this.TargetHeight = this.Target.Body.HeightPos + this.Target.Body.Height / 2;

			this.cachedTransform.rotation = Quaternion.LookRotation(target.CachedTransform.position - this.Position.ToVector3());


		}

		public void InitializeTimed(Vector2d forward)
		{
			Forward = forward;
			Direction = forward.ToVector3d();
		}

		Func<LSBody, bool> BodyConditional;

		public void InitializeFree(Vector3d direction, Func<LSBody, bool> bodyConditional, bool useGravity = false)
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

		public void UpdateVisuals()
		{
			cachedTransform.rotation = Quaternion.LookRotation(Forward.ToVector3());
			cachedTransform.position = this.Position.ToVector3();
		}

		private bool IsLasting;
		private int tickTimer;

		public void LateInit()
		{

			if (this.TargetingBehavior != TargetingType.Timed)
			{
				this.cachedTransform.position = this.Position.ToVector3();
				this.speedPerFrame = this.Speed / 32L;
			}



			switch (this.TargetingBehavior)
			{
				case TargetingType.Timed:
					this.CountDown = this.Delay;

					break;
				case TargetingType.Positional:
				case TargetingType.Homing:
					long f = this.Position.ToVector2d().Distance(this.TargetPosition);
					long timeToHit = f.Div(this.Speed);
					if (this._visualArc)
					{
						this.arcStartHeight = this.Position.z;
						if (timeToHit > 0)
						{


							this.arcStartVerticalSpeed = (this.TargetHeight - this.Position.z).Div(timeToHit) + timeToHit.Mul(Gravity);
						}
					}
					else
					{
						if (timeToHit > 0)
							this.linearHeightSpeed = (this.TargetHeight - Position.z).Div(timeToHit).Abs() / LockstepManager.FrameRate;

					}
					Forward = TargetPosition - this.Position.ToVector2d();
					Forward.Normalize();
					break;
				case TargetingType.Free:

					Vector3d vel = this.Direction;
					vel.Mul(speedPerFrame);
					this.Velocity = vel;


					break;
			}
			if (this.CanRotate)
			{
				this.cachedTransform.LookAt(this.Direction.ToVector3());
			}
			this.UpdateVisuals();

			if (this.onInitialize.IsNotNull())
			{
				this.onInitialize.Invoke();
			}

			if (UseEffects)
			{
				LSEffect effect = EffectManager.LazyCreateEffect(this.StartFX, this.Position.ToVector3(), this.cachedTransform.rotation);
				if (effect != null)
				{
					effect.StartPos = this.Position.ToVector3();
					effect.EndPos = this.TargetPosition.ToVector3(this.TargetHeight.ToFloat());
					if (this.Target != null)
						effect.Target = Target.transform;
				}
			}
		}

		private void OnHit()
		{
			if (this.TargetingBehavior == TargetingType.Free)
			{
				switch (this.HitBehavior)
				{
					case HitType.Single:
						//todo: Implement
						break;
				}
			}
			else
			{
				switch (this.HitBehavior)
				{
					case HitType.Single:
						if (Target == null)
						{
							//throw new System.Exception("Cannot use single hit effect without target");

							break;
						}
						this.HitAgent(Target);
						break;
					case HitType.Area:
						ApplyArea(this.Position.ToVector2d(), this.Radius);
						break;
					case HitType.Cone:
						Debug.Log(this.Forward);
						ApplyCone(this.Position, this.Forward, this.Radius, this.Angle);
						break;
				}
			}
		}

		private void ResetHit()
		{
			this.ExclusiveTargetType = this._exclusiveTargetType;
			this.onHit = null;
			this.onHitAgent = null;
		}

		private void ResetEffects()
		{
		}

		private void ResetHelpers()
		{
			this.lastDirection = Vector2d.zero;
			this.Velocity = default(Vector3d);
			this.Direction = Vector2d.up.ToVector3d();
		}

		private void ResetTargeting()
		{
			this.Delay = this._delay;
			this.Speed = this._speed;
			this.LastingDuration = this._lastingDuration;
			this.TickRate = this._tickRate;

		}

		private void ResetTrajectory()
		{
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

					if (!IsLasting)
					{
						if (this.CountDown <= 0)
						{
							IsLasting = true;
							tickTimer = 0;
						}
					}
					if (IsLasting)
					{
						tickTimer--;
						if (tickTimer <= 0)
						{
							tickTimer = TickRate;
							this.Hit((this.AliveTime + TickRate - this.Delay) >= this.LastingDuration);
						}
					}
					break;
				case TargetingType.Homing:
					if (this.CheckCollision())
					{
						this.TargetPosition = this.Target.Body._position;
						this.Hit();
					}
					else
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
					MoveToTargetPosition();

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
			}
			else
			{
				this.Position.z = FixedMath.MoveTowards(this.Position.z, TargetHeight, this.linearHeightSpeed);
			}

			LSProjectile.tempDirection = TargetPosition - this.Position.ToVector2d();
			if (LSProjectile.tempDirection.Dot(this.lastDirection.x, this.lastDirection.y) < 0L || tempDirection == Vector2d.zero)
			{
				this.Hit();
				//Debug.Log("asdf");
			}
			else
			{
				//Debug.Log("asdf2");
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
			foreach (LSBody body in Raycaster.RaycastAll(this.Position, nextPosition))
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
			}
			else
			{
				this.cachedGameObject.SetActiveIfNot(false);
			}
		}

		//
		// Events
		//
		public event Action onDeactivate;

		public event Action onHit;
		public event Action<LSAgent> onHitAgent;

		public event Action onInitialize;

		public event Action onSetup;
		public event Action onVisualize;

	}
}
