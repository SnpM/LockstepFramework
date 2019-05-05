namespace Lockstep
{
	public class LSNetworkSettingsSaver : EnvironmentSaver
	{
		public LSNetworkSettings SavedSettings = new LSNetworkSettings();
		protected override void OnEarlyApply()
		{
			LSNetworkSettings.Settings = SavedSettings;
		}
	}
}
