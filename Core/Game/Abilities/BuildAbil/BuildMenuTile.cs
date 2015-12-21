using UnityEngine;
using System.Collections;
using Lockstep;
using UnityEngine.UI;
using Lockstep.Data;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Lockstep
{
	public class BuildMenuTile : CerealBehaviour
	{
		[SerializeField]
		private Image _image;
		Image Imager {get{return _image;}}
		[SerializeField]
		private Button _button;
		Button Buttoner {get {return _button;}}
		[SerializeField]
		private Text _text;
		Text Texter {get {return _text;}}

		public int ID {get; private set;}
		public void Setup (int id) {
			this.ID = id;
			if (Buttoner.IsNotNull ())
			_button.onClick.AddListener(Press);
		}


		public void Open (string agentCode) {
			AgentInterfacer agentInfo = AgentController.GetAgentInterfacer (agentCode);
			Imager.sprite = agentInfo.Icon;
			Texter.text = agentCode.ToString ();
			gameObject.SetActiveIfNot (true);
		}
		public void Close () {
			gameObject.SetActiveIfNot (false);
		}
		public void Press () {
			BuildMenu.Press(ID);
		}
#if UNITY_EDITOR
        protected override void OnAfterSerialize  () {
			_image = _image ?? this.GetComponentInChildrenOrderered<Image> ();
			_button = _button ?? this.GetComponentInChildrenOrderered<Button> ();
			_text = _text ?? this.GetComponentInChildrenOrderered<Text> ();
		}
#endif
	}
}