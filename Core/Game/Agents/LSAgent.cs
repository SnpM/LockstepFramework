using UnityEngine;
using System.Collections;

namespace Lockstep
{
	/// <summary>
	/// LSAgents manage abilities and interpret commands.
	/// </summary>
	public class LSAgent : MonoBehaviour
	{
		public Ability[] Abilities;
		public ActiveAbility[] ActiveAbilities;
		public int AbilitiesLength;
		public LSBody Body;
		public ushort GlobalID;
		public ushort LocalID;
		public AgentCode MyAgentCode;
		public float SelectionRadius = 1f;
		public RingController ringController;

		public void Initialize ()
		{
			this.transform = GetComponent<Transform> ();
			this.gameObject = GetComponent<GameObject> ();
			this.renderer = GetComponent<Renderer> ();

			Abilities = this.GetComponents<Ability> ();
			AbilitiesLength = Abilities.Length;
			ActiveAbilities = new ActiveAbility[InputManager.InputCount];
			for (i = 0; i < Abilities.Length; i++) {
				Ability ability = Abilities [i];
				ability.Initialize (this);

				ActiveAbility activeAbility = ability as ActiveAbility;
				if (activeAbility != null) {
					ActiveAbilities [(int)activeAbility.ListenInput] = activeAbility;
				}
			}
			if (Body != null) {
				Body.Initialize ();
			}
		}

		public void Simulate ()
		{
			for (i = 0; i < AbilitiesLength; i++) {
				Abilities [i].Simulate ();
			}
		}
		public void Visualize ()
		{
			ringController.Visualize ();
		}

		public void Activate (Command com)
		{
			ActiveAbility activeAbility = (ActiveAbility)Abilities [(int)com.KeyInput];
			if (activeAbility != null) {
				activeAbility.Execute (com);
			}
		}

		public void Deactivate ()
		{
			PhysicsManager.Dessimilate (Body);
		}


		
		#region Utility Variables
		public bool IsSelected {
			get {
				return _isSelected;
			}
			set {
				if (_isSelected != value) {
					_isSelected = value;
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
		public int BoxedAgentsIndex;
		public int SelectedAgentsIndex;
		#endregion
		public Transform transform;
		public GameObject gameObject;
		public Renderer renderer;
		static int i, j;
		static AbilCode abilCode;
	}
}