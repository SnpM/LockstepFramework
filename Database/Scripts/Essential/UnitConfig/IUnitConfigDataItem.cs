namespace Lockstep.Data
{
	public interface IUnitConfigDataItem : INamedData
	{
		string Target { get; }
		Stat[] Stats { get; }
	}
}