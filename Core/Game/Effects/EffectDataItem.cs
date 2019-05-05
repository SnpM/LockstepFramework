namespace Lockstep.Data
{
	[System.Serializable]
	public class EffectDataItem : ObjectDataItem, IEffectData
	{
		public LSEffect GetEffect()
		{
			return base.Prefab.GetComponent<LSEffect>();
		}
	}
}