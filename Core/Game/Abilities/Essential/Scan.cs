using System;
using UnityEngine;
using Lockstep.Data;

namespace Lockstep
{
    public class Scan : ActiveAbility
    {
        private const int SearchRate = (int)(LockstepManager.FrameRate / 2);
        public const long MissModifier = FixedMath.One / 2;

        public virtual bool CanMove { get; private set; }

        protected bool CanTurn { get; private set; }

        public LSAgent Target { get; private set; }



        public bool HasTarget
        {
            get { return _hasTarget; }
            set
            {
                if (_hasTarget != value)
                {
                    _hasTarget = value;
                    if (CanMove)
                    {
                        if (_hasTarget)
                        {
                            cachedMove.CanCollisionStop = false;
                        } else
                        {
                            cachedMove.CanCollisionStop = true;
                        }
                    }
                }
            }
        }

        public virtual string ProjCode { get { return _projectileCode; } }

        public virtual long Range { get { return _range; } }
        //Range

        public virtual long Sight { get { return _sight; } }
        //Approximate radius that's scanned for targets

        public virtual long Damage { get { return _damage; } }
        //Damage of attack

        public virtual int AttackRate { get { return _attackRate; } }
        //Frames between each attack

        public virtual bool TrackAttackAngle { get { return _trackAttackAngle; } }
        //Whether or not to require the unit to face the target for attacking

        public long AttackAngle { get { return _attackAngle; } }
        //The angle in front of the unit that the target must be located in

        protected virtual AllegianceType TargetAllegiance //Allegiance to the target
        {
            get { return this._targetAllegiance; }
        }

        protected virtual PlatformType TargetPlatform //PlatformType of the target
        {
            get { return this._targetPlatform; }
        }

        public Vector3d ProjectileOffset { get { return _projectileOffset; } }
        //Offset of projectile

        #region Serialized Values (Further description in properties)

        [SerializeField,DataCode("Projectiles")]
        protected string _projectileCode;
        [FixedNumber, SerializeField]
        protected long _range = FixedMath.One * 6;
        [FixedNumber, SerializeField]
        protected long _sight = FixedMath.One * 10;
        [FixedNumber, SerializeField]
        protected long _damage = FixedMath.One;
        [FrameCount, SerializeField]
        protected int _attackRate = 1 * LockstepManager.FrameRate;
        [SerializeField, EnumMask]
        protected AllegianceType _targetAllegiance = AllegianceType.Enemy;
        [SerializeField, EnumMask]
        protected PlatformType _targetPlatform = PlatformType.Ground;
        [SerializeField]
        protected  bool _trackAttackAngle = true;
        [FixedNumberAngle, SerializeField]
        protected  long _attackAngle = FixedMath.TenDegrees;
        [SerializeField]
        protected  Vector3d _projectileOffset;
        [SerializeField, FixedNumber]
        protected long _energyCost;
        [SerializeField,FrameCount]
        protected int _windup;

        #endregion

        public int Windup {get {return _windup;}}

        public long EnergyCost { get { return _energyCost; } }

        //Stuff for the logic
        private bool inRange;
        private long fastRange;
        private long fastRangeToTarget;
        private Vector2d Destination;
        private int attackFrameCount;
        private Move cachedMove;
        private Turn cachedTurn;

        private LSBody cachedBody { get { return Agent.Body; } }

        private int rangeDeltaCount;
        private int baseDeltaCount;
        private int basePriority;
        private Health cachedTargetHealth;
        private uint targetVersion;
        private int deltaCount;
        private int searchCount;
        private int attackCount;
        private bool _hasTarget;
        private bool isAttackMoving;
        private bool isFocused;

        protected override void OnSetup()
        {
            cachedTurn = Agent.GetAbility<Turn>();
            cachedMove = Agent.GetAbility<Move>();
            if (Sight < Range)
                _sight = Range;
            baseDeltaCount = InfluenceManager.GenerateDeltaCount(Sight);
            rangeDeltaCount = InfluenceManager.GenerateDeltaCount(Range);

            fastRange = (Range * Range);
            attackFrameCount = AttackRate;
            basePriority = cachedBody.Priority;

            CanMove = cachedMove.IsNotNull();
            if (CanMove)
            {
                cachedMove.onArrive += HandleOnArrive;
                cachedMove.onGroupProcessed += _HandleMoveGroupProcessed;
            }

            CanTurn = cachedTurn.IsNotNull();

        }

        private void HandleOnArrive()
        {
            if (this.isAttackMoving)
            {
                if (this.HasTarget == false)
                    isAttackMoving = false;
            }
        }

        protected override void OnInitialize()
        {
            deltaCount = baseDeltaCount;
            cachedBody.Priority = basePriority;
            searchCount = LSUtility.GetRandom(SearchRate) + 1;
            attackCount = 0;
            HasTarget = false;
            Target = null;
            isAttackMoving = false;
            inRange = false;
            isFocused = false;

            this.Destination = Vector2d.zero;
        }

        protected override void OnSimulate()
        {
            attackCount--;
            if (HasTarget)
            {
                BehaveWithTarget();
            } else
            {
                BehaveWithNoTarget();
            }
        }
        [Lockstep (true)]
        bool IsWindingUp {get; set;}
        int windupCount;
        void StartWindup () {
            windupCount = this.Windup;
            IsWindingUp = true;
            Agent.ApplyImpulse(AnimImpulse.Fire);
        }

        void BehaveWithTarget()
        {
            if (Target.IsActive == false || Target.SpawnVersion != targetVersion)
            {
                StopEngage();
                BehaveWithNoTarget();
                return;
            }
            if (IsWindingUp) {
                windupCount--;
                if (windupCount < 0) {
                    Fire ();
                    this.attackCount = this.attackFrameCount - this.Windup;
                    IsWindingUp = false;
                }
            }
            else {
            Vector2d targetDirection = Target.Body._position - cachedBody._position;
            long fastMag = targetDirection.FastMagnitude();

            if (fastMag <= fastRangeToTarget)
            {
                if (!inRange)
                {
                    if (CanMove)
                        cachedMove.StopMove();
                }
                Agent.SetState(AnimState.Engaging);
                long mag;
                targetDirection.Normalize (out mag);
                bool withinTurn = TrackAttackAngle == false ||
                                  (fastMag != 0 &&
                        cachedBody.Forward.Dot(targetDirection.x, targetDirection.y) > 0
                        && cachedBody.Forward.Cross(targetDirection.x, targetDirection.y).Abs() <= AttackAngle);
                bool needTurn = mag != 0 && !withinTurn;
                if (needTurn)
                {
                    if (CanTurn)
                    {
                        cachedTurn.StartTurnDirection(targetDirection);
                    }
                    else {

                    }
                } else
                {
                    if (attackCount <= 0)
                    {
                        StartWindup ();
                    }
                }

                if (inRange == false)
                {
                    inRange = true;
                }
            } else
            {
                if (CanMove)
                {
                    if (cachedMove.IsMoving == false)
                    {
                        cachedMove.StartMove(Target.Body._position);
                        cachedBody.Priority = basePriority;
                    } else
                    {
                        if (Target.Body.PositionChanged || inRange)
                        {
                            cachedMove.Destination = Target.Body._position;
                        }
                    }
                }
                
                if (isAttackMoving || isFocused == false)
                {
                    searchCount -= 1;
                    if (searchCount <= 0)
                    {
                        searchCount = SearchRate;
                        if (ScanAndEngage())
                        {
                        } else
                        {
                        }
                    }
                }
                if (inRange == true)
                {
                    inRange = false;
                }
                
            }
            }
        }

        void BehaveWithNoTarget()
        {
            if (isAttackMoving || Agent.IsCasting == false)
            {
                if (isAttackMoving)
                {
                    {
                        searchCount -= 8;
                    }
                } else
                {
                    searchCount -= 2;
                }
                if (searchCount <= 0)
                {
                    searchCount = SearchRate;
                    if (ScanAndEngage())
                    {
                    }
                }
            }
        }

        public void Fire()
        {

                if (CanMove)
                {
                    cachedMove.StopMove();
                }
                cachedBody.Priority = basePriority + 1;
                OnFire();

        }

        protected virtual void OnFire()
        {
            long appliedDamage = Damage;
            Health healther = Agent.GetAbility<Health>();
            LSProjectile projectile = ProjectileManager.Create(
                ProjCode,
                this.Agent,
                this.ProjectileOffset,
                this.TargetAllegiance,
                (other) => healther.IsNotNull() && healther.HealthAmount > 0,
                (agent) => healther.TakeRawDamage(appliedDamage));
            projectile.InitializeHoming(this.Target);
            projectile.TargetPlatform = TargetPlatform;
            ProjectileManager.Fire(projectile);
        }

        public void Engage(LSAgent other)
        {
            if (other != Agent)
            {
                cachedTargetHealth = other.GetAbility<Health>();
                if (cachedTargetHealth.IsNotNull())
                {
                    Target = other;

                    HasTarget = true;
                    targetVersion = Target.SpawnVersion;
                    IsCasting = true;
                    fastRangeToTarget = Range + (Target.Body.IsNotNull() ? Target.Body.Radius : 0);
                    fastRangeToTarget *= fastRangeToTarget;
                }
            }
        }

        public void StopEngage(bool complete = false)
        {
            isFocused = false;
            if (complete)
            {
                isAttackMoving = false;
            } else
            {
                if (isAttackMoving)
                {
                    if (ScanAndEngage() == false)
                    {
                        cachedMove.StartMove(this.Destination);
                    } else
                    {
                    }
                } else
                {
                    if (CanMove)
                    {
                        if (HasTarget && inRange == false)
                        {
                            cachedMove.StopMove();
                        }
                    }
                }
            }

            HasTarget = false;
            Target = null;
            cachedBody.Priority = basePriority;

            IsCasting = false;
        }

        protected override void OnDeactivate()
        {
            StopEngage(true);
        }

        protected override void OnExecute(Command com)
        {
            Agent.StopCast(this.ID);
            Vector2d pos;
            DefaultData target;
            if (com.TryGetData<Vector2d>(out pos) && CanMove)
            {

                if (HasTarget)
                {
                    cachedMove.RegisterGroup(false);
                } else
                {
                    cachedMove.RegisterGroup();
                }

                isAttackMoving = true;
                isFocused = false;

            } else if (com.TryGetData<DefaultData> (out target) && target.Is(DataType.UShort))
            {
                isFocused = true;
                isAttackMoving = false;
                LSAgent tempTarget;
                DefaultData data;
                ushort targetValue = (ushort)target.Value;
                AgentController.TryGetAgentInstance(targetValue, out tempTarget);
                Engage(tempTarget);
            }

        
        }

        protected sealed override void OnStopCast()
        {
            StopEngage(true);
        }

        Action _handleMoveGroupProcessed;

        Action _HandleMoveGroupProcessed { get { return _handleMoveGroupProcessed ?? (_handleMoveGroupProcessed = HandleMoveGroupProcessed); } }

        void HandleMoveGroupProcessed()
        {
            this.Destination = cachedMove.Destination;
        }

        private bool ScanAndEngage()
        {
            LSAgent agent = DoScan ();
            if (agent == null || HasTarget && agent == Target)
            {
                return false;
            } else
            {
                Engage(agent);
                return true;
            }
        }

        private LSAgent DoScan () {
            return InfluenceManager.Scan(
                this.cachedBody.Position,
                this.rangeDeltaCount,
                (other) => other.GetAbility<Health>().IsNotNull(),
                (bite) => ((this.Agent.Controller.GetAllegiance(bite) & this.TargetAllegiance) != 0)
            );
        }

        public bool ScanWithinRangeAndEngage()
        {
            LSAgent agent = this.DoScan();
            if (agent == null)
            {
                return false;
            } else
            {
                Engage(agent);
                return true;
            }
        }
        #if UNITY_EDITOR
        [SerializeField, Visualize]
        private Vector3 _projectileOrigin = Vector3.forward;

        protected override void OnAfterSerialize()
        {
            if (transform.position != Vector3.zero)
            {
                Debug.LogWarning("Visual editting can only be used when transform is at origin.");
                return;
            }
            Vector3 temp = (base.transform.InverseTransformPoint(_projectileOrigin));
            temp *= base.transform.localScale.x;
            _projectileOffset = new Vector3d(temp);
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(Application.isPlaying ? Agent.Body._visualPosition : this.transform.position, this.Range.ToFloat()); 
        }
        #endif
    }
}