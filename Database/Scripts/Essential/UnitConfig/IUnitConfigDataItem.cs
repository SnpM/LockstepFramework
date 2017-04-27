using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;

namespace Lockstep.Data
{
	public interface IUnitConfigDataItem : INamedData
	{
		string Target { get; }
		Stat [] Stats { get; }
	}
}