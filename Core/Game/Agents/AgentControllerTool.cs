using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lockstep
{
	public static class AgentControllerTool
	{
		public static void SetFullHostile (AgentController con) {
			for (int j = 0; j < AgentController.InstanceManagers.Count; j++) {
				AgentController ac = AgentController.InstanceManagers [j];
				if (ac != con) {
					con.SetAllegiance (ac, AllegianceType.Enemy);
					ac.SetAllegiance (con, AllegianceType.Enemy);
				}
			}
		}
	}
}