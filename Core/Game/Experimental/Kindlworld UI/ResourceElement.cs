using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lockstep;
namespace Lockstep.Experimental {
	public class ResourceElement : MonoBehaviour {

		[SerializeField]
		private Text _amountText;
		Text AmountText {get {return _amountText;}}

		[SerializeField]
		private Image _icon;
		Image Icon {get {return _icon;}}

		ResourceContainer Container {get; set;}

		public void LinkResource (ResourceContainer container) {
			Container = container;
			Initialize ();
		}
		void Initialize () {
			Icon.sprite = Container.MyResourceType.Visual;
			SetAmount (0);
			Container.onAddResource += HandleAddResource;
			Container.onChangeResource += HandleChangeResource;
		}
		void HandleChangeResource () {
			SetAmount (Container.Total);
		}
		void HandleAddResource (long obj)
		{
			
		}
		void SetAmount (long amount) {
			AmountText.text = amount.ToFormattedDouble ().ToString ();

		}

	}
}