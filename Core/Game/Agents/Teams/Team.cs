using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep.Data;
namespace Lockstep
{
	public class Team
	{

		public Vector2d BasePos {get; private set;}
		public Team (Vector2d basePos) {
			BasePos = basePos;
		}

		public int ID {get; private set;}

		public AgentController MainController {get; private set;}

		public void Setup (int id) {
			ID = id;

		}
        public readonly FastList<AllegianceType> Diplomacy = new FastList<AllegianceType>();


		public void Initialize ()
		{

			Diplomacy.FastClear ();
			for (int i = 0; i < TeamManager.Teams.Count; i++) {
				Team team = TeamManager.Teams[i];
				if (team != this)
                    this.SetAllegiance(team,AllegianceType.Neutral);
			}
			TeamManager.UpdateDiplomacy (this);

            TeamManager.Teams.Add(this);
			this.SetAllegiance (this, AllegianceType.Friendly);

			MainController = AgentController.Create();
			MainController.JoinTeam (this);

		}

		public void Simulate ()
		{

		}

		public void Visualize ()
		{

		}


		public void AddController (AgentController controller) {
			controller.JoinTeam (this);
		}

		public void SetAllegiance (Team other, AllegianceType allegiance) {
            while (other.ID >= Diplomacy.Count)
                Diplomacy.Add(AllegianceType.Neutral);
			Diplomacy[other.ID] = allegiance;
		}
		public AllegianceType GetAllegiance (AgentController controller) {
			return GetAllegiance (controller.MyTeam);
		}
		public AllegianceType GetAllegiance (Team team) {
			return Diplomacy[team.ID];
		}
	}
}