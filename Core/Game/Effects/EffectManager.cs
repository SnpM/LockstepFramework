using UnityEngine;
using System.Collections;
using Lockstep.Data;
using System.Collections.Generic;
namespace Lockstep {


	public static class EffectManager {

		const int MaxEffects = ProjectileManager.MaxProjectiles * 2;
		private static Dictionary<string,FastStack<LSEffect>> EffectPool;
        private static Dictionary<string,EffectDataItem> CodeDataMap;
		public static void Setup ()
		{
            EffectDataItem[] effectData = (LSDatabaseManager.CurrentDatabase as DefaultLSDatabase).EffectData;
			EffectPool = new Dictionary<string,FastStack<LSEffect>>(effectData.Length);
            CodeDataMap = new Dictionary<string, EffectDataItem>(effectData.Length);
			for (int i = 0; i < effectData.Length; i++)
			{
                EffectDataItem dataItem = effectData[i];
                string code = (string)dataItem.Name;
				EffectPool.Add(code,new FastStack<LSEffect> ());
                CodeDataMap.Add(code,dataItem);
			}
		}

		public static void Visualize ()
		{
			for (int i = 0; i < PeakCount; i++)
			{
				if (EffectActive[i])
				{
					Effects[i].Visualize ();
				}
			}
		}
        public static bool IsValid (string effectCode) {
            return !string.IsNullOrEmpty (effectCode) && effectCode != "None";
        }
		public static void LazyCreateEffect (string effectCode, Vector3 position)
		{
            if (!IsValid (effectCode)) return;
			LSEffect effect = CreateEffect (effectCode);
			effect.CachedTransform.position = position;
			effect.Initialize ();
		}

		public static void LazyCreateEffect (string effectCode, Vector3 position, Quaternion rotation)
		{
            if (!IsValid (effectCode)) return;
			LSEffect effect = CreateEffect (effectCode);
			effect.CachedTransform.position = position;
			effect.Initialize ();
		}

		public static void LazyCreateEffect (string effectCode, Transform SpawnPoint)
		{
            if (!IsValid (effectCode)) return;
			LSEffect effect = CreateEffect (effectCode);
			effect.CachedTransform.position = SpawnPoint.position;
			effect.Initialize ();
		}

		public static LSEffect CreateEffect (string effectCode)
		{
            if (!IsValid (effectCode)) return null;

			LSEffect effect = GenEffect (effectCode, -1);
			EffectActive[effect.ID] = true;
			Effects[effect.ID] = effect;
			return effect;
		}


		#region Allocation

		private static bool[] EffectActive = new bool[MaxEffects];
		private static LSEffect[] Effects = new LSEffect[MaxEffects];

		private static FastStack<int> OpenSlots = new FastStack<int>();
		private static int PeakCount;

		private static int GenerateID ()
		{
			if (OpenSlots.Count > 0)
				return OpenSlots.Pop ();
			else return PeakCount++;
		}

		private static LSEffect GenEffect (string effectCode, int id = -1)
		{
			FastStack<LSEffect> pool = EffectPool[effectCode];
			LSEffect effect = null;
			if (pool.Count > 0)
			{
				effect = pool.Pop ();
			}
			else {
				Debug.Log (effectCode);
                EffectDataItem dataItem = CodeDataMap[effectCode];
				effect = GameObject.Instantiate<GameObject> (dataItem.Prefab).GetComponent<LSEffect> ();
				effect.Setup (effectCode);
			}

			if (id == -1)
				id = GenerateID ();
			if (effect.Create (id))
			{
				return effect;
			}
			else {
				return GenEffect (effectCode, id);
			}
		}
		public static void EndEffect (LSEffect effect)
		{
			if (EffectActive[effect.ID] == false) return;
			effect.Deactivate ();
			EffectPool[effect.MyEffectCode].Add (effect);
			EffectActive[effect.ID] = false;
			Effects[effect.ID] = null;
			OpenSlots.Add (effect.ID);

		}
		/// <summary>
		/// Eliminates an effect and allows the Garbage Collector to clean it up.
		/// </summary>
		/// <param name="effect">Effect.</param>
		public static void DestroyEffect (LSEffect effect)
		{
			if (EffectActive[effect.ID] == false) return;
			EffectActive[effect.ID] = false;
			OpenSlots.Add (effect.ID);
		}
		#endregion
	}
}