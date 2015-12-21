using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Lockstep.Data;
namespace Lockstep {
	public class CommandTile : CerealBehaviour {
		[SerializeField]
		private Button _button;
		public Button LeButton{get {return _button;}}
		[SerializeField]
		private Image _image;
		public Image LeImage {get {return _image;}}
		public int ID {get; private set;}

		private AbilityInterfacer _interfacer;
		public AbilityInterfacer Interfacer {get {return _interfacer;}
			set {
				if (value != _interfacer)
				{
					gameObject.SetActiveIfNot (true);
					LeImage.sprite = value.Icon;
					_interfacer = value;
				}
			}
		}

		public void Setup (int id) {
			ID = id;
			_button.onClick.AddListener (Activate);
			ResetTile ();
		}

		public void Visualize () {
			if (Interfacer .IsNotNull ())
			{
				if (InputManager.GetInputDown (Interfacer.ListenInput))
				{
					Activate ();
				}
			}
		}

		public void ResetTile ()
		{
			if (this == null) return;
			gameObject.SetActiveIfNot (false);
			_interfacer = null;
		}

		private void Activate () {
			OnActivate (ID);
			InterfaceManager.CurrentInterfacer = Interfacer;
		}

		public event Action<int> OnActivate;
#if UNITY_EDITOR
        protected override void OnAfterSerialize  ()
		{
			_image = this.GetComponentInChildrenOrderered<Image> ();
			_button = GetComponent<Button> ();
		}
#endif
	}
}