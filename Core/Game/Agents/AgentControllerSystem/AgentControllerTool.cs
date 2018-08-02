namespace Lockstep
{
	public static class AgentControllerTool
	{
		public static void SetFullHostile(AgentController con)
		{
			//TODO: Make this hostile to new controllers
			for (int j = 0; j < AgentController.InstanceManagers.Count; j++)
			{
				if (j == con.ControllerID) continue;
				AgentController ac = AgentController.InstanceManagers[j];
				if (ac != con)
				{
					con.SetAllegiance(ac, AllegianceType.Enemy);
					ac.SetAllegiance(con, AllegianceType.Enemy);
				}
			}
		}
	}
}