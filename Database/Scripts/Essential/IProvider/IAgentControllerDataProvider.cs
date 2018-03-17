using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep.Data
{
	public interface IAgentControllerDataProvider
	{
		AgentControllerDataItem[] AgentControllerData {get;}
	}
}