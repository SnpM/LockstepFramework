using System.Collections; using FastCollections;
using System.Collections.Generic;
using UnityEngine;
namespace Lockstep
{
	public class LSNetworkSettingsSaver : EnvironmentSaver
	{
		public LSNetworkSettings SavedSettings = new LSNetworkSettings ();
		protected override void OnApply ()
		{
			LSNetworkSettings.Settings = SavedSettings;
		}
	}
}
