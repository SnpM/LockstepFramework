using UnityEngine;

namespace Lockstep
{
	public class UnityInstance : MonoBehaviour
	{
		private static UnityInstance instance;

		public static UnityInstance Instance
		{
			get {
				if (instance == null)
				{
					instance = new GameObject("UnityInstance").AddComponent<UnityInstance>();
					DontDestroyOnLoad(instance.gameObject);
				}
				return instance;
			}
		}

	}
}

