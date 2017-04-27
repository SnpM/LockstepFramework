using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep.Data;
using System.Collections.Generic;

namespace Lockstep
{


    public static class EffectManager
    {

        const int MaxEffects = ProjectileManager.MaxProjectiles * 2;
        private static Dictionary<string,FastStack<LSEffect>> EffectPool;
        private static Dictionary<string,IEffectData> CodeDataMap;

        public static void Setup()
        {
            IEffectDataProvider database;
            if (LSDatabaseManager.TryGetDatabase<IEffectDataProvider>(out database))
            {
                IEffectData[] effectData = database.EffectData;
                EffectPool = new Dictionary<string,FastStack<LSEffect>>(effectData.Length);
                CodeDataMap = new Dictionary<string, IEffectData>(effectData.Length);
                for (int i = 0; i < effectData.Length; i++)
                {
                    IEffectData dataItem = effectData [i];
                    string code = (string)dataItem.Name;
                    EffectPool.Add(code, new FastStack<LSEffect>());
                    CodeDataMap.Add(code, dataItem);
                }
            }
        }

        public static void Visualize()
        {
            for (int i = 0; i < PeakCount; i++)
            {
                if (EffectActive [i])
                {
                    Effects [i].Visualize();
                }
            }
        }

		public static void Deactivate()
		{
			for (int i = 0; i < PeakCount; i++)
			{
				if (EffectActive [i])
				{
					EndEffect(Effects [i]);
				}
			}
		}

        public static bool IsValid(string effectCode)
        {
			return !string.IsNullOrEmpty (effectCode) && effectCode != "None" && CodeDataMap.ContainsKey (effectCode);
        }

        public static LSEffect LazyCreateEffect(string effectCode, Vector3 position)
        {
            if (!IsValid(effectCode))
                return null;
            LSEffect effect = CreateEffect(effectCode);
            effect.CachedTransform.position = position;
            Fire (effect);
            return effect;
        }

        public static LSEffect LazyCreateEffect(string effectCode, Vector3 position, Quaternion rotation)
        {
            if (!IsValid(effectCode))
                return null;
            LSEffect effect = CreateEffect(effectCode);
            effect.CachedTransform.position = position;
            effect.CachedTransform.rotation = rotation;
            Fire (effect);
            return effect;

        }

        public static LSEffect LazyCreateEffect(string effectCode, Transform spawnParent)
        {
            if (!IsValid(effectCode))
                return null;
            LSEffect effect = CreateEffect(effectCode);
            effect.CachedTransform.parent = spawnParent;
            Fire (effect);
            return effect;

        }

        public static LSEffect CreateEffect(string effectCode)
        {
            if (!IsValid(effectCode))
                return null;

            LSEffect effect = GenEffect(effectCode, -1);
            EffectActive [effect.ID] = true;
            Effects [effect.ID] = effect;
            return effect;
        }

        public static void Fire (LSEffect effect) {
            effect.Initialize();
        }
        #region Allocation

        private static bool[] EffectActive = new bool[MaxEffects];
        private static LSEffect[] Effects = new LSEffect[MaxEffects];

        private static FastStack<int> OpenSlots = new FastStack<int>();
        private static int PeakCount;

        private static int GenerateID()
        {
            if (OpenSlots.Count > 0)
                return OpenSlots.Pop();
            else
                return PeakCount++;
        }

        private static LSEffect GenEffect(string effectCode, int id = -1)
        {
            FastStack<LSEffect> pool;

            if (!EffectPool.TryGetValue(effectCode, out pool))  {
                return null;
            }
            LSEffect effect = null;
            if (pool.Count > 0)
            {
                effect = pool.Pop();
            } else
            {
                IEffectData dataItem = CodeDataMap [effectCode];
                effect = GameObject.Instantiate<GameObject>(dataItem.GetEffect().gameObject).GetComponent<LSEffect>();
                effect.Setup(effectCode);
            }

            if (id == -1)
                id = GenerateID();
            if (effect.Create(id))
            {
                return effect;
            } else
            {
                return GenEffect(effectCode, id);
            }
        }

        public static void EndEffect(LSEffect effect)
        {
            if (EffectActive [effect.ID] == false)
                return;
            effect.Deactivate();
            EffectPool [effect.MyEffectCode].Add(effect);
            EffectActive [effect.ID] = false;
            Effects [effect.ID] = null;
            OpenSlots.Add(effect.ID);

        }

        /// <summary>
        /// Eliminates an effect and allows the Garbage Collector to clean it up.
        /// </summary>
        /// <param name="effect">Effect.</param>
        public static void DestroyEffect(LSEffect effect)
        {
            if (EffectActive [effect.ID] == false)
                return;
            EffectActive [effect.ID] = false;
            OpenSlots.Add(effect.ID);
        }

        #endregion
    }
}