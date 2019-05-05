namespace Lockstep
{
	/// <summary>
	/// The AgentCommander stores information and runs behaviors for one AgentController. They're like BehaviourHelpers for each AgentController.
	/// AgentCommanders are spawned as singletons and will execute all commands received by its AgentController
	/// </summary>
	/// Atm it piggybacks on the LSAgent component system
	public class AgentCommander : Ability
	{
		public AgentController Controller { get { return Agent.Controller; } }
		protected override void OnInitialize()
		{
		}
		public T GetAbility<T>() where T : Ability
		{
			return Agent.GetAbility<T>();
		}
	}
}