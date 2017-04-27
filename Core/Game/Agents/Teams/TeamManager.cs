namespace Lockstep
{
	using UnityEngine;
	using System.Collections; using FastCollections;
    using System.Collections.Generic;
	public static class TeamManager
	{
		const int mapSize = 48;
		private static readonly Team[] PredefinedTeams = new Team[]{
			new Team (new Vector2d (mapSize, mapSize)),
			new Team (new Vector2d (-mapSize, -mapSize))
			          };
		public static readonly FastList<Team> Teams = new FastList<Team> ();

		#region Routines
		public static void Setup ()
		{

		}

		public static void UpdateDiplomacy (Team newTeam) {
			for (int i = 0; i < Teams.Count; i++) {
                Teams[i].SetAllegiance (newTeam,AllegianceType.Neutral);
			}
		}
		public static void Initialize () {
			Teams.FastClear ();
			nextDistribute = 0;
		}
		public static void LateInitialize ()
		{
			for (int i = 0; i < PredefinedTeams.Length; i++) {
				Team team = PredefinedTeams[i];
				Teams.Add (team);
				team.Setup (i);
				team.Initialize ();
			}
			for (int i = 0; i < Teams.Count; i++) {
				for (int j = 0; j < Teams.Count; j++) {
					Teams[i].SetAllegiance (Teams[j], AllegianceType.Enemy);
				}
				Teams[i].SetAllegiance (Teams[i],AllegianceType.Friendly);
			}
		}

		public static void Simulate ()
		{
			for (int i = 0; i < Teams.Count; i++) {
				Teams [i].Simulate ();
			}
		}

		public static void Visualize ()
		{
			for (int i = 0; i < Teams.Count; i++) {
				Teams [i].Visualize ();
			}
		}

		public static void Deactivate ()
		{
			
		}

		#endregion
		static int nextDistribute;

		public static void JoinTeam (AgentController controller)
		{
			Teams [nextDistribute++].AddController (controller);
			if (nextDistribute >= Teams.Count)
			{
				nextDistribute = 0;
			}
		}
	}
}