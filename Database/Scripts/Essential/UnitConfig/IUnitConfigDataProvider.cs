namespace Lockstep.Data
{
	public interface IUnitConfigDataProvider
	{
		IUnitConfigDataItem[] UnitConfigData { get; }
		UnitConfigElementDataItem[] UnitConfigElementData { get; }
	}
}