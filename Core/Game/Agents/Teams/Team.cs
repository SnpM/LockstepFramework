using UnityEngine;
using System.Collections;
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


		public void Initialize ()
		{

			Diplomacy.FastClear ();
			for (int i = 0; i < TeamManager.Teams.Count; i++) {
				Team team = TeamManager.Teams[i];
				if (team != this)
					Diplomacy.AddAt (AllegianceType.Neutral, team.ID);
			}
			TeamManager.UpdateDiplomacy (this);
			this.SetAllegiance (this, AllegianceType.Friendly);

			MainController = new AgentController();
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

		public readonly FastBucket<AllegianceType> Diplomacy = new FastBucket<AllegianceType>();
		public void SetAllegiance (Team other, AllegianceType allegiance) {
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