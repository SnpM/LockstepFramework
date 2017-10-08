using UnityEngine;

namespace Lockstep
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MovementGroupHelper))]
	[RequireComponent(typeof(ScanGroupHelper))]
	[RequireComponent(typeof(EnvironmentHelper))]
	public class DefaultHelperSetup : MonoBehaviour
	{
		void Awake()
		{
			DestroyImmediate(this);
		}
	}
}