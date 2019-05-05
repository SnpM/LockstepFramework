using UnityEngine;

namespace Lockstep.Data
{
	[System.Serializable]
	public class InputDataItem : DataItem
	{
		//TODO: Keycode/input linking
		public InputDataItem(string name)
		{
			_name = name;
		}
		public InputDataItem(string name, KeyCode keyCode)
		{
			_name = name;
		}
	}
}