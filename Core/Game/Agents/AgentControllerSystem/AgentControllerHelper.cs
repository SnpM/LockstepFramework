using UnityEngine;
using Lockstep.Data;
using FastCollections;

namespace Lockstep
{
	/// <summary>
	/// At the moment a simple script that automatically creates AgentControllers at the start of games
	/// </summary>
	public class AgentControllerHelper : BehaviourHelper
	{
		[SerializeField, DataCodeAttribute("AgentControllers")]
		private string _environmentController;
		public string EnvironmentController { get { return _environmentController; } }
		[SerializeField, DataCodeAttribute("AgentControllers")]
		private string _defaultController;
		public string DefaultController { get { return _defaultController; } }

		public static AgentControllerHelper Instance { get; private set; }
		BiDictionary<string, byte> CodeIDMap = new BiDictionary<string, byte>();
		protected override void OnInitialize()
		{
			Instance = this;

			IAgentControllerDataProvider database;
			if (!LSDatabaseManager.TryGetDatabase<IAgentControllerDataProvider>(out database))
			{
				Debug.LogError("IAgentControllerDataProvider unavailable.");
			}

			//TODO: Re-implement cammander system. Putting on hold for now.
			//Also think of other settings for AgentController to be set in database

			AgentControllerDataItem[] controllerItems = database.AgentControllerData;
			for (int i = 0; i < controllerItems.Length; i++)
			{
				var item = controllerItems[i];
				var controller = AgentController.Create(item.DefaultAllegiance);
				if (item.PlayerManaged)
				{
					PlayerManager.AddController(controller);
				}
				if (string.IsNullOrEmpty(item.CommanderCode) == false)
				{
					controller.CreateCommander(item.CommanderCode);
				}
				CodeIDMap.Add(item.Name, controller.ControllerID);
			}
		}

		public AgentController GetInstanceManager(string controllerCode)
		{
			if (string.IsNullOrEmpty(controllerCode))
			{
				Debug.Log("controllerCode is null or empty.");
				return null;
			}
			byte id;
			if (!CodeIDMap.TryGetValue(controllerCode, out id))
			{
				Debug.Log("Controller name " + controllerCode + " is not valid.");
			}

			return AgentController.GetInstanceManager(id);
		}

	}
}