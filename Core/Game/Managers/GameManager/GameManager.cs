using UnityEngine;
using Lockstep.NetworkHelpers;

namespace Lockstep
{
	public class GameManager : MonoBehaviour
	{
		void Awake()
		{
			NetworkHelper networkHelper = gameObject.GetComponent<NetworkHelper>();
			if (networkHelper == null)
				networkHelper = gameObject.AddComponent<DefaultNetworkHelper>();

			//Currently deterministic but not guaranteed by Unity
			// may be add as serialized Array as property?  [SerializeField] private BehaviourHelper[] helpers; ?
			BehaviourHelper[] helpers = this.gameObject.GetComponentsInChildren<BehaviourHelper>();
			LockstepManager.Initialize(helpers, networkHelper);
		}

		void FixedUpdate()
		{
			LockstepManager.Simulate();
		}

		void Update()
		{
			LockstepManager.Visualize();
		}

		void LateUpdate()
		{
			LockstepManager.LateVisualize();
		}

		bool Quited = false;

		void OnDisable()
		{
			if (Quited)
				return;
			LockstepManager.Deactivate();
		}

		void OnApplicationQuit()
		{
			Quited = true;
			LockstepManager.Quit();
		}

	}
}