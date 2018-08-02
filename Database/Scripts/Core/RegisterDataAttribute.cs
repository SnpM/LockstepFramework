using System;

namespace Lockstep.Data
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
	public sealed class RegisterDataAttribute : Attribute
	{
		public RegisterDataAttribute(string displayName)
		{
			this._dataName = displayName;
		}

		private string _dataName;
		public string DataName { get { return _dataName; } }
	}
}