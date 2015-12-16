using System;
using UnityEngine;
using Lockstep.Data;

namespace Lockstep
{
    public class Scan : ActiveAbility
    {
        private const int SearchRate =  (int)(LockstepManager.FrameRate / 2);
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

        public virtual long Range { get { return _range; } } //Range

        public virtual long Sight { get { return _sight; } } //Approximate radius that's scanned for targets

        public virtual long Damage { get { return _damage; } } //Damage of attack

        public virtual int AttackRate { get { return _attackRate; } } //Frames between each attack

        public virtual bool TrackAttackAngle { get { return _trackAttackAngle; } } //Whether or not to require the unit to face the target for attacking

        public long AttackAngle { get { return _attackAngle; } } //The angle in front of the unit that the target must be located in

        protected virtual AllegianceType TargetAllegiance //Allegiance to the target
        {
            get { return this._targetAllegiance; }
        }

        protected virtual PlatformType TargetPlatform //PlatformType of the target
        {
            get {return this._targetPlatform;}
        }

        public Vector2d ProjectileOffset { get { return _projectileOffset.ToOrientedVector2d(); } } //Offset of projectile

        public float ProjectileHeightOffset { get { return _projectileOffset.Height; } } //Offset of projectile on the Y axis

        #region Serialized Values (Further description in properties)
        [SerializeField,DataCode ("Projectiles")]
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
        protected  Vector2dHeight _projectileOffset;
        [SerializeField, FixedNumber]
        protected long _energyCost;

        #endregion

        public long EnergyCost { get { return _energyCost; } }

        //Stuff for the logic
        private bool inRange;
        private long fastRange;
        private long fastRangeToTarget;
        private Vector2d Destination;
        private int attackFrameCount;
        private Move cachedMove;
        private Turn cachedTurn;
        private LSBody cachedBody {get {return Agent.Body;}}
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
        private Vector2d projectileOffset;

        protected override void OnSetup()
        {
            cachedTurn = Agent.Turner;
            cachedMove = Agent.Mover;
            if (Sight < Range)
                _sight = Range;
            baseDeltaCount = InfluenceManager.GenerateDeltaCount(Sight);
            rangeDeltaCount = InfluenceManager.GenerateDeltaCount(Range);

            fastRange = (Range * Range);
            attackFrameCount = AttackRate;
            basePriority = cachedBody.Priority;

            CanMove = cachedMove .IsNotNull();
            if (CanMove)
            {
                cachedMove.OnArrive += HandleOnArrive;
                cachedMove.onGroupProcessed += _HandleMoveGroupProcessed;
            }

            CanTurn = cachedTurn .IsNotNull();

        }

        private void HandleOnArrive()
        {
            if (this.isAttackMoving) {
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

        void BehaveWithTarget()
        {
            if (Target.IsActive == false || Target.SpawnVersion != targetVersion)
            {
                StopEngage();
                BehaveWithNoTarget();
                return;
            }
            Vector2d targetDirection = Target.Body.Position - cachedBody.Position;
            long fastMag = targetDirection.FastMagnitude();

            if (fastMag <= fastRangeToTarget)
            {
                if (!inRange)
                {
                    if (CanMove)
                    cachedMove.StopMove();
                }
                Agent.SetState(AnimState.Engaging);
                long mag = FixedMath.Sqrt(fastMag >> FixedMath.SHIFT_AMOUNT);
                //cachedTurn.StartTurn(targetDirection / mag);
                bool withinTurn = TrackAttackAngle == false ||
                    (fastMag != 0 &&
                     cachedBody.Rotation.Dot(targetDirection.x, targetDirection.y) > 0
                     && cachedBody.Rotation.Cross(targetDirection.x, targetDirection.y).Abs() <= AttackAngle);
                bool needTurn = mag != 0 && !withinTurn;
                if (needTurn)
                {
                    if (CanTurn)
                    {
                        targetDirection /= mag;
                        cachedTurn.StartTurn(targetDirection);
                    }
                } 
                else
                {
                    if (attackCount <= 0)
                    {
                        attackCount = attackFrameCount;
                        Fire();
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
                        cachedMove.StartMove(Target.Body.Position);
                        cachedBody.Priority = basePriority;
                    } else
                    {
                        if (Target.Body.PositionChanged || inRange)
                        {
                            cachedMove.Destination = Target.Body.Position;
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
            if (Agent.UseEnergy(this.EnergyCost))
            {
                if (CanMove)
                {
                    cachedMove.StopMove();
                }
                cachedBody.Priority = basePriority + 1;
                Agent.ApplyImpulse(AnimImpulse.Fire);

                OnFire();
            }
        }

        protected virtual void OnFire()
        {
            long appliedDamage = Damage;
            //appliedDamage = 0;
            LSProjectile projectile = ProjectileManager.Create(ProjCode, Agent, Target, appliedDamage);
            projectile.TargetPlatform = TargetPlatform;
            ProjectileManager.Fire(projectile);
        }

        public void Engage(LSAgent other)
        {
            if (other != Agent)
            {
                cachedTargetHealth = other.Healther;
                if (cachedTargetHealth .IsNotNull())
                {
                    Target = other;

                    HasTarget = true;
                    targetVersion = Target.SpawnVersion;
                    IsCasting = true;
                    fastRangeToTarget = Range + (Target.Body .IsNotNull() ? Target.Body.Radius : 0);
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

            if (com.HasPosition || com.HasTarget)
            {
                if (com.HasPosition && CanMove)
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

                } else if (com.HasTarget)
                {

                    isFocused = true;
                    isAttackMoving = false;
                    LSAgent tempTarget;
                    AgentController.TryGetAgentInstance(com.Target, out tempTarget);
                    Engage(tempTarget);
                }
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
            LSAgent agent = Agent.Influencer.Scan(deltaCount, TargetAllegiance, TargetPlatform);
            if (agent == null || HasTarget && agent == Target)
            {
                return false;
            } else
            {
                Engage(agent);
                return true;
            }
        }

        public bool ScanWithinRangeAndEngage()
        {
            LSAgent agent = Agent.Influencer.Scan(rangeDeltaCount, TargetAllegiance, TargetPlatform);
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
        private Vector3 _projectileOrigin;
        protected override void OnAfterSerialize  ()
        {
            if (transform.position != Vector3.zero)
            {
                Debug.LogWarning ("Visual editting can only be used when transform is at origin.");
                return;
            }
            _projectileOffset = new Vector2dHeight(base.transform.InverseTransformPoint (_projectileOrigin));
        }
#endif
    }
}