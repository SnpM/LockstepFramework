using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


namespace Lockstep
{
	public static class AbilityManager
	{
		public static AbilCode[] AbilityCodes = (AbilCode[])Enum.GetValues (typeof(AbilCode));
		public static FastStack<Ability>[] CachedAbilities = new FastStack<Ability>[AbilityCodes.Length];
		private static Dictionary<AbilCode,Type> CachedAbilTypes = new Dictionary<AbilCode, Type> (256);


		public static Ability CreateAbility (AbilCode abilCode)
		{
			FastStack<Ability> abilCache = CachedAbilities[(int)abilCode];
			if (abilCache == null || abilCache.Count == 0) {
				Type abilType;
				if (!CachedAbilTypes.TryGetValue (abilCode, out abilType)) {
					abilType = Type.GetType (abilCode.ToString ());
					CachedAbilTypes.Add (abilCode, abilType);
				}
				return (Ability)Activator.CreateInstance (abilType);
			}
			return abilCache.Pop ();
		}

		public static void CacheAbility (Ability ability)
		{
			AbilCode abilCode = ability.Code;

			FastStack<Ability> abilCache = CachedAbilities[(int)abilCode];
			if (abilCache == null)
			{
				abilCache = new FastStack<Ability> ();

				CachedAbilities[(int)abilCode] = abilCache;
			}
			abilCache.Add (ability);
		}
	}

}