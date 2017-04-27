using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;

namespace Lockstep.Data
{
	public interface IUnitConfigDataProvider
	{
		IUnitConfigDataItem [] UnitConfigData { get; }
		UnitConfigElementDataItem [] UnitConfigElementData { get; }
	}
}