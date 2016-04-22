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

#endif
namespace Lockstep {
	[RequireComponent (typeof (LSBody))]
    /// <summary>
    /// LSAgents manage abilities and interpret commands.
    /// </summary>
    public class LSAgent : MonoBehaviour, IMousable {

        Vector3 IMousable.WorldPosition {
            get {return this.Body._visualPosition;}
        }
        float IMousable.MousableRadius {
            get {return this.SelectionRadius;}
        }

		static FastList<Ability> setupAbilitys = new FastList<Ability>();

		[SerializeField, FrameCount]
        private int _deathTime = LockstepManager.FrameRate * 2;

		//[SerializeField, DataCode ("Agents")]
		private string _myAgentCode;
		public string MyAgentCode
		{
			get {
				return _myAgentCode;
			}
			private set {
				_myAgentCode = value;
			}
		}

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
            

        [SerializeField]
        private int _globalID;
        public ushort GlobalID { get {return(ushort) _globalID;} private set {_globalID = (int)value;} }

        public ushort LocalID { get; private set;}
        public uint BoxVersion { get; set; }

		[SerializeField]
		private int _boxPriority = 0;
		public int BoxPriority {get {return _boxPriority;}}

		[SerializeField]
		private int _selectionPriority;
		public int SelectionPriority {get {return _selectionPriority;}}

		public bool Selectable {get; set;}
		public bool CanSelect {get {return Selectable && IsVisible;}}

        public ushort TypeIndex;

		public Vector2 Position2 {get{return new Vector2(CachedTransform.position.x, CachedTransform.position.z);}}
		public FastList<AbilityDataItem> Interfacers {get {return abilityManager.Interfacers;}}

        #region Pre-runtime generated (maybe not)
		//[SerializeField]
		private Ability[] _attachedAbilities;
		public Ability[] AttachedAbilities {get {return _attachedAbilities;}}
		//[SerializeField]
		private LSBody _body;
		public LSBody Body { get {return _body;} }
        //[SerializeField]
        private LSTrigger[] _triggers;
        public LSTrigger[] Triggers {get {return _triggers;}}
		//[SerializeField]
		private LSAnimatorBase _animator;
		public LSAnimatorBase Animator { get {return _animator;} }
		//[SerializeField]
		private Transform _cachedTransform;
		public Transform CachedTransform {get{return _cachedTransform;}}
		//[SerializeField]
		private GameObject _cachedGameObject;
		public GameObject CachedGameObject {get {return _cachedGameObject;}}
		#endregion

        //TODO: Put all this stuff in an extendible class
        public LSInfluencer Influencer { get; private set; }

		public bool IsActive { get; private set;}

        public event Action<LSAgent> onDeactivate;


		public AgentTag Tag;

        public bool CheckCasting { get; set; }
        public bool IsCasting {
            get {
                return abilityManager.CheckCasting();
            }
        }



        public uint SpawnVersion { get; private set; }
        public AgentController Controller { get; private set; }  

		public bool Controllable {
			get {
				return PlayerManager.ContainsController (Controller);
			}
		}

        public event Action onSelectedChange;
        public bool IsSelected {
            get { return isSelected; }
            set {
                if (isSelected != value) {
                    isSelected = value;
                    if (onSelectedChange != null)
                        onSelectedChange();
                }
            }
        }

        public event Action onHighlightedChange;
        public bool IsHighlighted {
            get { return isHighlighted; }
            set {
                if (IsHighlighted != value) {
                    isHighlighted = value;
                    if (onHighlightedChange != null)
                        onHighlightedChange ();
                }
            }
        }

        public bool IsVisible {
            //get { return cachedRenderer == null || (cachedRenderer.enabled && cachedRenderer.isVisible); }
			get { return true; } //TODO: Return true only if viable GladFox: seen for what kind of camera? :)
        }

        public AllegianceType GetAllegiance(LSAgent other) {
            return Controller.GetAllegiance(other.Controller);
        }


        private bool isHighlighted;
        private bool isSelected;
		public readonly AbilityManager abilityManager = new AbilityManager();

		[SerializeField]
        private float _selectionRadius = -1f;
        public float SelectionRadius {get {return _selectionRadius <= 0 ? this.Body.Radius.ToFloat() + 2f : _selectionRadius;}}
		[SerializeField]
		private Transform _visualCenter;
		public Transform VisualCenter {get {return _visualCenter;}}
		public float SelectionRadiusSquared {get; private set;}

        public FastBucket<Buff> Buffs = new FastBucket<Buff>();

        internal void AddBuff (Buff buff) {
            buff.ID = Buffs.Add(buff);
        }
        internal void RemoveBuff (Buff buff) {
            Buffs.RemoveAt(buff.ID);
        }

        public IAgentData Data {get; private set;}
        private readonly FastList<int> TrackedLockstepTickets = new FastList<int>();
        void Awake () {
            gameObject.SetActive(false);

        }
        public void Setup(IAgentData interfacer) {
            gameObject.SetActive(true);
            LoadComponents ();

			GameObject.DontDestroyOnLoad(gameObject);

			setupAbilitys.FastClear();
            
            MyAgentCode = interfacer.Name;
            Data = interfacer;
            SpawnVersion = 1;
            CheckCasting = true;

            Influencer = new LSInfluencer();
			if (_visualCenter == null)
				_visualCenter = CachedTransform;

            if (Animator .IsNotNull ()) {
                Animator.Setup();
            }


			abilityManager.Setup(this);
			

            Influencer.Setup(this);
            Body.Setup(this);

			SelectionRadiusSquared = SelectionRadius * SelectionRadius;

            this.RegisterLockstep();


        }
        private void RegisterLockstep () {
            TrackedLockstepTickets.Add (LSVariableManager.Register(this.Body));
            foreach (Ability abil in this.abilityManager.Abilitys) {
                TrackedLockstepTickets.Add(LSVariableManager.Register(abil));
            }
        }
        public IEnumerable<LSVariable> GetDesyncs (int[] compare) {
            int position = 0;
            foreach (int ticket in this.TrackedLockstepTickets) {
                LSVariableContainer container = LSVariableManager.GetContainer(ticket);
                int[] hashes = container.GetCompareHashes();
                for (int i = 0; i < hashes.Length; i++) {
                    if (compare[i] != hashes[position]) {
                        yield return container.Variables[i];
                    }
                    position++;
                }


            }
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
            Vector2d rotation = default (Vector2d)) {

			LocalID = localID;
			GlobalID = globalID;
			Controller = controller;

			IsActive = true;
			CheckCasting = true;
			Selectable = true;


            CachedGameObject.SetActiveIfNot (true);
            if (Body .IsNotNull ()) {
                Body.Initialize(new Vector2dHeight(position), rotation);
            }

            if (Triggers.IsNotNull()) {
                foreach (LSTrigger trigger in Triggers) {
                    trigger.Initialize();
                }
            }

            if (Influencer .IsNotNull ()) {
                Influencer.Initialize();
            }



            abilityManager.Initialize();
            if (Animator .IsNotNull ()) {
                Animator.Initialize();
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
            for (int i = 0; i < this.Buffs.PeakCount; i++) {
                if (this.Buffs.arrayAllocation[i]) {
                    this.Buffs[i].Simulate();
                }
            }
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
            if (onDeactivate != null)
                this.onDeactivate(this);
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
			
			abilityManager.Deactivate();
			
            Body.Deactivate();
			if (Influencer .IsNotNull ()) {
				Influencer.Deactivate();
			}

		}
        private IEnumerator<int> PoolDelayer() {
            yield return _deathTime;
            Pool();
        }

        private void Pool() {
            AgentController.CacheAgent(this);
			if (CachedGameObject != null)
            CachedGameObject.SetActive (false);
        }

        public void SetState (AnimState animState) {
            if (Animator .IsNotNull ()) {
                Animator.CurrentAnimState = animState;
            }
        }

		public void ApplyImpulse(AnimImpulse animImpulse, int rate = 0) {
            if (Animator .IsNotNull ()) {
                Animator.ApplyImpulse(animImpulse, rate);
            }
        }

        public T GetAbility<T>() where T : Ability{
            return abilityManager.GetAbility<T>();
        }
        public Ability GetAbility (string name) {
            return abilityManager.GetAbility (name);
        }

        public long GetStateHash () {
            long hash = 3;
            hash ^= this.GlobalID;
            hash ^= this.LocalID;
            hash ^= this.Body._position.GetStateHash ();
            hash ^= this.Body._rotation.GetStateHash ();
            hash ^= this.Body.Velocity.GetStateHash ();
            return hash;
        }
            

        private void LoadComponents () {
            _cachedTransform = base.transform;
            _cachedGameObject = base.gameObject;
            _body = GetComponent<LSBody> ();
			_animator = GetComponent<LSAnimatorBase> ();
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

        /*public override bool GetSerializedFieldNames(List<string> output)
        {
            return false;
            base.GetSerializedFieldNames(output);
            output.Add("_deathTime");
            output.Add("_boxPriority");
            output.Add("_selectionPriority");
            output.Add("_selectionRadius");
            output.Add("_statsBarOffset");
            output.Add("_visualCenter");
            output.Add("_globalID");
			//output.Add("_myAgentCode");
            return true;
        }*/
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
			_selectionRadius = -1f;
			_visualCenter = transform;
		}


#endif
    }
}