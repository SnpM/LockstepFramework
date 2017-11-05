using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lockstep {
	/// <summary>
	/// At the moment a simple script that automatically creates AgentControllers at the start of games
	/// </summary>
	public class AgentControllerCreator : BehaviourHelper {
		/// <summary>
		/// Settings for an AgentController to be created
		/// </summary>
		[System.Serializable]
		public class AgentControllerSettings {
			[SerializeField,DataCode("Agents")]
			private string _commanderCode;
			public string CommanderCode {get { return _commanderCode; }}


		}

		[SerializeField]
		private AgentControllerSettings[] _controllers;
		public AgentControllerSettings[] Controllers {get {return _controllers;}}

		protected override void OnInitialize ()
		{
			for (int i = 0; i < Controllers.Length; i++) {
				var settings = Controllers [i];
				var controller = AgentController.Create ();
				if (!string.IsNullOrEmpty (settings.CommanderCode)) {
					controller.CreateCommander (settings.CommanderCode);
				}
			}
		}
			
	}
}