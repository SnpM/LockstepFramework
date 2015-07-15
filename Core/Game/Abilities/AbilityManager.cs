using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Lockstep
{
	public static class AbilityManager
	{
		public static AbilCode[] AbilityCodes = (AbilCode[])Enum.GetValues (typeof(AbilCode));
		private static Dictionary<AbilCode,Type> CachedAbilTypes = new Dictionary<AbilCode, Type> (256);

		public static Ability CreateAbility (AbilCode abilCode)
		{
			Type abilType;
			if (!CachedAbilTypes.TryGetValue (abilCode, out abilType)) {
				abilType = Type.GetType (abilCode.ToString ());
				CachedAbilTypes.Add (abilCode, abilType);
			}
			return (Ability)Activator.CreateInstance (abilType);

		}
	}

}