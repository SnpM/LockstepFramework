using UnityEngine;
using System.Collections;

namespace Lockstep
{
	/// <summary>
	/// LSAgents manage abilities and interpret commands.
	/// </summary>
	public class LSAgent : MonoBehaviour
	{
		public AbilCode[] _Abilities;
		public Ability[] Abilities;
		public ActiveAbility[] ActiveAbilities;
		public uint AbilityExists;
		public int AbilitiesLength;
		public LSBody Body;
		public ushort GlobalID;
		public ushort LocalID;
		public AgentCode MyAgentCode;

		public void Initialize ()
		{
			AbilitiesLength = _Abilities.Length;
			Abilities = new Ability[AbilitiesLength];
			ActiveAbilities = new ActiveAbility[InputManager.InputCount];
			for (i = 0; i < _Abilities.Length; i++)
			{
				abilCode = _Abilities[i];


				Ability ability = (Ability) AbilityManager.CreateAbility (abilCode);
				AbilityManager.CreateAbility (AbilCode.Print);

				Abilities[(int)abilCode] = ability;
				ability.Initialize (this);

				ActiveAbility activeAbility = ability as ActiveAbility;
				if (activeAbility != null)
				{
					ActiveAbilities[(int)activeAbility.ListenInput] = activeAbility;
				}
			}
			if (Body != null)
			{
				Body.Initialize ();
			}
		}

		public void Simulate ()
		{
			for (i = 0; i < AbilitiesLength; i++)
			{
				Abilities[i].Simulate ();
			}
		}

		public void Activate (Command com)
		{
			ActiveAbility activeAbility = (ActiveAbility)Abilities[(int)com.KeyInput];
			if (activeAbility != null)
			{
				activeAbility.Execute (com);
			}
		}

		public void Deactivate ()
		{
			PhysicsManager.Dessimilate (Body);
		}

		static int i, j;
		static AbilCode abilCode;
	}
}