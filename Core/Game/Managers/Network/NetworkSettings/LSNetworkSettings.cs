using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Lockstep
{
	[System.Serializable]
	public class LSNetworkSettings
	{
		public static LSNetworkSettings Settings { get; set; }
		public JitterSettings JitterSettings = new JitterSettings (2f, .1f, .004f);
				

	}
	[System.Serializable]
	public struct JitterSettings
	{
		

		public JitterSettings (float compensation, float sensitivity, float degrade)
		{
			JitterCompensation = compensation;
			JitterSensitivity = sensitivity;
			JitterDegrade = degrade;
		}
		//Note: Higher jitter factor = more lag bufering
		/// <summary>
		/// The jitter compensation. Higher value = higher effect of jitter factor on lag buffering.
		/// </summary>
		public float JitterCompensation;
		/// <summary> 
		/// The jitter sensitivity. Higher value = bigger effect on jitter factor of each jitter.
		/// </summary>
		public float JitterSensitivity;
		/// <summary>
		/// The jitter degrade. Higher value = faster reduction of jitter factor.
		/// </summary>
		public float JitterDegrade;
	}
}