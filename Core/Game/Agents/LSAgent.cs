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

		private bool _isHighlighted;
		public uint BoxVersion;
		public int SelectedAgentsIndex;
		#endregion
		public Transform cachedTransform;
		public GameObject cachedGameObject;
		public Renderer cachedRenderer;
		static int i, j, iterator;
	}
}