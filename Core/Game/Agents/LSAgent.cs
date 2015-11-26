using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
	/// <summary>
	/// LSAgents manage abilities and interpret commands.
	/// </summary>
	public class LSAgent : MonoBehaviour
	{
		public Ability[] Abilities;
		public int AbilityCount;
		public ActiveAbility[] ActiveAbilities;
		public LSBody Body;
		public ushort GlobalID;
		public ushort LocalID;
		public AgentCode MyAgentCode;
		public float SelectionRadius = 1f;
		public RingController ringController;
		public LSInfluencer Influencer;
		public AgentController MyAgentController;

		public void Initialize (AgentController controller)
		{
			MyAgentController = controller;

			cachedTransform = base.transform;
			cachedGameObject = base.gameObject;
			cachedRenderer = GetComponent<Renderer> ();

			if (Body == null) {
				Body = GetComponent<LSBody>();
			}
			Body.Initialize ();
			if (Influencer == null)
			{
				Influencer = new LSInfluencer();
			}
			Influencer.Initialize (this);

			Abilities = this.GetComponents<Ability> ();
			AbilityCount = Abilities.Length;
			ActiveAbilities = new ActiveAbility[InputManager.InputCount];
			for (iterator = 0; iterator < AbilityCount; iterator++) {
				Ability ability = Abilities [iterator];
				ability.Initialize (this);
				ActiveAbility activeAbility = ability as ActiveAbility;
				if (activeAbility != null) {
					ActiveAbilities [(int)activeAbility.ListenInput] = activeAbility;
				}
			}
			delta = InfluenceManager.GenerateDeltaCount(5);
			Debug.Log (delta);
		}

		int delta;
		public void Simulate ()
		{
			Influencer.Simulate ();
			for (iterator = 0; iterator < AbilityCount; iterator++) {
				Abilities [iterator].Simulate ();
			}

			if (LocalID == 0)
			
<<<<<<< HEAD
			{
				FastList<LSAgent> agents = new FastList<LSAgent>();
				 Influencer.ScanAll (delta,agents);
				string s = "";
				for (i = 0; i < agents.Count;i++)
				{
					s += agents[i].LocalID + ", ";
				}
				Debug.Log (s);
			}
		}
		public void Visualize ()
		{
			if (ringController != null)
			ringController.Visualize ();
		}

		static byte leIndex;
		public void Execute (Command com)
		{
			leIndex = (byte)com.LeInput;
			ActiveAbility activeAbility = (ActiveAbility)ActiveAbilities [leIndex];
			if (activeAbility != null) {
				activeAbility.Execute (com);
=======
            LoadComponents ();


			GameObject.DontDestroyOnLoad(gameObject);

			setupAbilitys.FastClear();
            
            MyAgentCode = interfacer.GetAgentCode();
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
			Vector2d position = default (Vector2d)) {

			LocalID = localID;
			GlobalID = globalID;
			Controller = controller;

			IsActive = true;
			CheckCasting = true;
			Selectable = true;


			CachedGameObject.SetActive (true);
            if (Body .IsNotNull ()) {
                Body.Initialize(position, Vector2d.up);
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
>>>>>>> origin/develop
			}
		}

		public void Deactivate ()
		{
			for (iterator = 0; iterator < AbilityCount; iterator++)
			{
				Abilities[iterator].Deactivate();
			}
			PhysicsManager.Dessimilate (Body);
			Influencer.Deactivate ();

		}
<<<<<<< HEAD
=======
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
>>>>>>> origin/develop

		public T GetAbility<T> () where T : Ability
		{
			T ret;
			for (i = 0; i < AbilityCount; i++)
			{
				ret = Abilities[i] as T;
				if (System.Object.ReferenceEquals (ret,null) == false) return ret;
			}
			return null;
		}

		/*public System.Object GetAbility (Type AbilityType)
		{
			for (i = 0; i < AbilityCount; i++)
			{
				if (Abilities[i].GetType() == AbilityType)
				{
					return (System.Object)Abilities[i];
				}
			}
			return null;
		}*/
		
		#region Utility Variables
		public bool IsSelected {
			get {
				return _isSelected;
			}
			set {
				if (_isSelected != value) {
					_isSelected = value;
					if (ringController != null)
					if (_isSelected) {
						ringController.Select ();
					} else {
						if (IsHighlighted) {
							ringController.Highlight ();
						} else {
							ringController.Unselect ();
						}
					}
				}
			}
<<<<<<< HEAD
=======

            if (StatsBarer != null)
			StatsBarer.Deactivate ();
			if (Ringer .IsNotNull ()) Ringer.Deactivate ();
>>>>>>> origin/develop
		}

		private bool _isSelected;
			
		public bool IsHighlighted {
			get {
				return _isHighlighted;
			}
			set {
				if (IsHighlighted != value) {
					_isHighlighted = value;
					if (ringController != null)
					if (!_isSelected) {
						if (_isHighlighted) {
							ringController.Highlight ();
						} else {
							ringController.Unselect ();
						}
					}
				}
			}
		}

<<<<<<< HEAD
		private bool _isHighlighted;
		public uint BoxVersion;
		public int SelectedAgentsIndex;
		#endregion
		public Transform cachedTransform;
		public GameObject cachedGameObject;
		public Renderer cachedRenderer;
		static int i, j, iterator;
	}
=======
        private void LoadComponents () {
            _cachedTransform = base.transform;
            _cachedGameObject = base.gameObject;
            _body = GetComponent<LSBody> ();
            _animator = GetComponent<LSAnimator> ();
            _attachedAbilities = GetComponents<Ability> ();
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
>>>>>>> origin/develop
}