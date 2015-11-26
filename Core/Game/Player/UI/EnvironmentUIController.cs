using UnityEngine;
using System.Collections;
using Lockstep.UI;
namespace Lockstep {
	public class EnvironmentUIController : MonoBehaviour {
		[SerializeField]
		private CommandCard _comCard;
		public CommandCard ComCard {get {return _comCard;}}
		[SerializeField]
		private BuildMenu _buildingMenu;
		public BuildMenu BuildingMenu {get {return _buildingMenu;}}
		void Awake () {
			gameObject.SetActive (false); //Prevents multiple MenuUIs from being active
		}
		public void Setup () {
			gameObject.SetActive (true);
			ComCard.Setup ();
			_buildingMenu.Setup ();
		}
		public void Initialize () {
			_buildingMenu.Initialize ();

		}
	}
}