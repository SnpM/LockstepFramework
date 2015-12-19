//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================
using System;
using System.Collections.Generic;
using UnityEngine;
using Lockstep.UI;
using Lockstep.Data;
#if UNITY_EDITOR
using UnityEditor;
using Lockstep.Integration;
#endif
namespace Lockstep {
	[RequireComponent (typeof (LSBody))]
    /// <summary>
    /// LSAgents manage abilities and interpret commands.
    /// </summary>
    public class LSAgent : CerealBehaviour, IMousable {

        Vector3 IMousable.WorldPosition {
            get {return this.Body.visualPosition;}
        }
        float IMousable.MousableRadius {
            get {return this.SelectionRadius;}
        }

		static FastList<Ability> setupAbilitys = new FastList<Ability>();

		[SerializeField, FrameCount]
        private int _deathTime = LockstepManager.FrameRate * 2;

        public string MyAgentCode { get; private set; }

        [SerializeField, HideInInspector]
        private AgentType _agentType;
        public AgentType MyAgentType {
            get {
                return _agentType;
            }
        }

        private LSBusStop _busStop;
        public LSBusStop BusStop {
            get {return _busStop ?? (_busStop = new LSBusStop());}
        }

        public event Action<LSAgent> onDeactivation;
        public event Action<bool, bool> onInteraction;
		public event Action<LSAgent> onBuildChild;
        public ushort GlobalID { get; private set; }
        public ushort LocalID { get; private set; }
        public uint BoxVersion { get; set; }

		[SerializeField]
		private Vector3 _statsBarOffset = Vector3.up;
		public Vector3 StatsBarOffset {get {return _statsBarOffset;}}

		[SerializeField]
		private int _boxPriority = 0;
		public int BoxPriority {get {return _boxPriority;}}

		[SerializeField]
		private int _selectionPriority;
		public int SelectionPriority {get {return _selectionPriority;}}

		public bool Selectable {get; set;}
		public bool CanSelect {get {return Selectable && IsVisible;}}



		public Vector2 Position2 {get{return new Vector2(CachedTransform.position.x, CachedTransform.position.z);}}
		public FastList<AbilityInterfacer> Interfacers {get {return abilityManager.Interfacers;}}

		#region Pre-runtime generated
		[SerializeField]
		private Ability[] _attachedAbilities;
		public Ability[] AttachedAbilities {get {return _attachedAbilities;}}
		[SerializeField]
		private LSBody _body;
		public LSBody Body { get {return _body;} }
        [SerializeField]
        private LSTrigger[] _triggers;
        public LSTrigger[] Triggers {get {return _triggers;}}
		[SerializeField]
		private LSAnimator _animator;
		public LSAnimator Animator { get {return _animator;} }
		[SerializeField]
		private Transform _cachedTransform;
		public Transform CachedTransform {get{return _cachedTransform;}}
		[SerializeField]
		private GameObject _cachedGameObject;
		public GameObject CachedGameObject {get {return _cachedGameObject;}}
		#endregion
        public LSInfluencer Influencer { get; private set; }
		public Health Healther { get {return abilityManager.Healther;} }
		public Scan Scanner { get {return abilityManager.Scanner;} }
		public Move Mover { get {return abilityManager.Mover;} }
		public Turn Turner {get {return abilityManager.Turner;}}
		public StatsBar StatsBarer{get; private set;}
		public EnergyStore EnergyStorer {get {return abilityManager.EnergyStorer;}}
		public RingController Ringer {get; private set;}
		public bool IsActive { get; private set;}


		public AgentTag Tag;

        public bool CheckCasting { get; set; }
        public bool IsCasting {
            get {
                return abilityManager.CheckCasting();
            }
        }
		public bool UseEnergy (long energyCost) {
			return EnergyStorer == null || EnergyStorer.Use (energyCost);
		}

		public PlatformType Platform {
			get { return CachedGameObject.layer == LayerMask.NameToLayer("Air") ? PlatformType.Air : PlatformType.Ground; }
		}


        public uint SpawnVersion { get; private set; }
        public AgentController Controller { get; private set; }  

		public bool Controllable {
			get {
				return PlayerManager.ContainsController (Controller);
			}
		}

        public bool IsSelected {
            get { return isSelected; }
            set {
                if (isSelected != value) {
                    isSelected = value;
                    if (onInteraction .IsNotNull ()) {
                        onInteraction(isSelected, isHighlighted);
                    }
					if (Ringer .IsNotNull ())
					if (isSelected) {
						Ringer.Select ();

					}
					else {
						if (IsHighlighted)
							Ringer.Highlight ();
						else
							Ringer.Unselect ();
					}
                }
            }
        }

        public bool IsHighlighted {
            get { return isHighlighted; }
            set {
                if (IsHighlighted != value) {
                    isHighlighted = value;
                    if (onInteraction .IsNotNull ()) {
                        onInteraction(isSelected, isHighlighted);
                    }
					if (Ringer .IsNotNull ())
					if (IsSelected == false) {
						if (IsHighlighted) {
							Ringer.Highlight ();
						}
						else {
							Ringer.Unselect();
						}
					}
                }
            }
        }

        public bool IsVisible {
            //get { return cachedRenderer == null || (cachedRenderer.enabled && cachedRenderer.isVisible); }
            get { return true; } //TODO: Return true only if viable
        }

        public AllegianceType GetAllegiance(LSAgent other) {
            return Controller.GetAllegiance(other.Controller);
        }

        public bool IsInjured {
            get { return Healther.HealthAmount < Healther.MaxHealth; }
        }

        private bool isHighlighted;
        private bool isSelected;
		private readonly AbilityManager abilityManager = new AbilityManager();

		[SerializeField]
		private float _selectionRadius = 1f;
		public float SelectionRadius {get {return _selectionRadius;}}
		[SerializeField]
		private Transform _visualCenter;
		public Transform VisualCenter {get {return _visualCenter;}}
		public float SelectionRadiusSquared {get; private set;}

        public AgentInterfacer Interfacer {get; private set;}

        void Awake () {
            gameObject.SetActive(false);
        }

        public void Setup(AgentInterfacer interfacer) {
			
            LoadComponents ();


			GameObject.DontDestroyOnLoad(gameObject);

			setupAbilitys.FastClear();
            
            MyAgentCode = interfacer.Name;
            Interfacer = interfacer;
            SpawnVersion = 1;
            CheckCasting = true;

            Influencer = new LSInfluencer();
			if (_visualCenter == null)
				_visualCenter = CachedTransform;

            if (Animator .IsNotNull ()) {
                Animator.Setup();
            }


			abilityManager.Setup(this);
			Ringer = RingController.Create ();
			if (Ringer .IsNotNull ())
			Ringer.Setup (this);

            Influencer.Setup(this);
            Body.Setup(this);

			SelectionRadiusSquared = _selectionRadius * _selectionRadius;
            if (StatsBarer != null)
			StatsBarer.Setup (this);
        }

		public void SessionReset () {
			this.BoxVersion = 0;
			this.SpawnVersion = 0;
		}

        public void Initialize(
			AgentController controller,
		    ushort localID,
			ushort globalID,
			Vector2d position = default (Vector2d),
            Vector2d rotation = new Vector2d(0,1)) {

			LocalID = localID;
			GlobalID = globalID;
			Controller = controller;

			IsActive = true;
			CheckCasting = true;
			Selectable = true;


			CachedGameObject.SetActive (true);
            if (Body .IsNotNull ()) {
                Body.Initialize(position, rotation);
            }

            if (Triggers.IsNotNull()) {
                foreach (LSTrigger trigger in Triggers) {
                    trigger.Initialize();
                }
            }

            if (Influencer .IsNotNull ()) {
                Influencer.Initialize();
            }

            if (Animator .IsNotNull ()) {
                Animator.Initialize();
            }

            abilityManager.Initialize();
            if (StatsBarer != null)
			StatsBarer.Initialize ();
			if (Ringer .IsNotNull ()) {
				Ringer.Initialize ();
				IsSelected = false;
				IsHighlighted = false;
			}
        }

        public void Simulate() {

            if (Influencer .IsNotNull ()) {
                Influencer.Simulate();
            }

            abilityManager.Simulate();

			if (IsCasting == false) {
				SetState (AnimState.Idling);
			}

        }
		public void LateSimulate () {
			abilityManager.LateSimulate ();
		}
		[HideInInspector]
		public bool VisualPositionChanged;
		Vector3 lastVisualPosition;
        public void Visualize() {
			VisualPositionChanged = CachedTransform.hasChanged && lastVisualPosition != (lastVisualPosition = CachedTransform.position);
			if (VisualPositionChanged) {
				lastVisualPosition = CachedTransform.position;
			}

            abilityManager.Visualize();
            if (Animator .IsNotNull ()) {
                Animator.Visualize();
            }
            if (StatsBarer != null)
			StatsBarer.Visualize ();

        }

        public void Execute(Command com) {
            abilityManager.Execute(com);
        }

        public void StopCast(int exceptionID = -1) {
            abilityManager.StopCast(exceptionID);
        }

		
		public void Die (bool immediate = false) {
			AgentController.DestroyAgent(this, immediate);
			if (Animator .IsNotNull ())
			{
				SetState (AnimState.Dying);
				Animator.Visualize ();
			}
		}

        public void Deactivate(bool Immediate = false) {
			_Deactivate ();
            if (Immediate == false) {
                CoroutineManager.StartCoroutine(PoolDelayer());
            } else {
                Pool();
            }
        }
		private void _Deactivate () {
            IsActive = false;

            IsSelected = false;
			SpawnVersion++;
			if (onDeactivation .IsNotNull ()) {
				onDeactivation(this);
			}
			
			abilityManager.Deactivate();
			
            Body.Deactivate();
			if (Influencer .IsNotNull ()) {
				Influencer.Deactivate();
			}

            if (StatsBarer != null)
			StatsBarer.Deactivate ();
			if (Ringer .IsNotNull ()) Ringer.Deactivate ();
		}
        private IEnumerator<int> PoolDelayer() {
            yield return _deathTime;
            Pool();
        }

        private void Pool() {
            AgentController.CacheAgent(this);
			if (CachedGameObject .IsNotNull ())
            CachedGameObject.SetActive (false);
        }

        public void SetState (AnimState animState) {
            if (Animator .IsNotNull ()) {
                Animator.CurrentAnimState = animState;
            }
        }

        public void ApplyImpulse(AnimImpulse animImpulse) {
            if (Animator .IsNotNull ()) {
                Animator.ApplyImpulse(animImpulse);
            }
        }

        public T GetAbility<T>() where T : Ability {
            return abilityManager.GetAbility<T>();
        }
        public long GetStateHash () {
            long hash = 3;
            hash ^= this.GlobalID;
            hash ^= this.LocalID;
            hash ^= this.Body.Position.GetStateHash ();
            hash ^= this.Body.Rotation.GetStateHash ();
            hash ^= this.Body.Velocity.GetStateHash ();
            return hash;
        }

		public LSAgent BuildChild (string agentCode, Vector2d localPos, float localHeight) {
			LSAgent agent = this.Controller.CreateAgent (agentCode);
			agent.Body.Parent = this.Body;
			agent.Body.LocalPosition = localPos;
			agent.Body.LocalRotation = Vector2d.up.rotatedRight;
			agent.Body.visualPosition.y = localHeight + this.Body.LocalPosition.y;
			agent.Body.UpdatePosition ();
			agent.Body.UpdateRotation();
			agent.Body.BuildChangedValues ();
			if (onBuildChild .IsNotNull ()) onBuildChild (agent);
			return agent;
		}

        private void LoadComponents () {
            _cachedTransform = base.transform;
            _cachedGameObject = base.gameObject;
            _body = GetComponent<LSBody> ();
            _animator = GetComponent<LSAnimator> ();
            _attachedAbilities = GetComponents<Ability> ();
            _triggers = GetComponents<LSTrigger> ();
        }
#if UNITY_EDITOR
		public void RefreshComponents () {
            LoadComponents ();
			SerializedObject so = new SerializedObject (this);
            so.Update ();
            so.ApplyModifiedProperties ();
		}
        public override bool GetSerializedFieldNames(List<string> output)
        {
            base.GetSerializedFieldNames(output);
            output.Add("_deathTime");
            output.Add("_boxPriority");
            output.Add("_selectionPriority");
            output.Add("_selectionRadius");
            output.Add("_statsBarOffset");
            output.Add("_visualCenter");
            return true;
        }
		/*protected override bool OnSerialize ()
		{
			LSEditorUtility.FrameCountField ("Death Time", ref _deathTime);
			_boxPriority = EditorGUILayout.IntField ("Box Priority", _boxPriority);
			this._selectionPriority = EditorGUILayout.IntField ("Selection Priority", _selectionPriority);
			_selectionRadius = EditorGUILayout.FloatField ("Selection Radius", _selectionRadius);
			this._statsBarOffset = EditorGUILayout.Vector3Field ("Stats Offset", this._statsBarOffset);
			_visualCenter = (Transform)EditorGUILayout.ObjectField ("Visual Center", _visualCenter, typeof (Transform), true);
			RefreshComponents ();
			return true;
		}*/
		void Reset () {
			_selectionRadius = 1f;
			_visualCenter = transform;
		}
#endif
    }
}