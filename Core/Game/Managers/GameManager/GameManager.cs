using Lockstep;
using UnityEngine;
using FastCollections;
using Lockstep.NetworkHelpers;
using System;

namespace Lockstep
{
	public class GameManager : MonoBehaviour
	{
		public static GameManager Instance { get; private set; }

		void Awake()
		{
			Instance = this;

			NetworkHelper networkHelper = gameObject.GetComponent<NetworkHelper>();
			if (networkHelper == null)
				networkHelper = gameObject.AddComponent<DefaultNetworkHelper>();
			

			//Currently deterministic but not guaranteed by Unity
			// may be add as serialized Array as property?  [SerializeField] private BehaviourHelper[] helpers; ?
 			BehaviourHelper[] helpers = this.gameObject.GetComponents<BehaviourHelper>();
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
			Instance = null;
			if (Quited)
				return;
			LockstepManager.Deactivate();
		}

		void OnApplicationQuit()
		{
			Instance = null;
			Quited = true;
			LockstepManager.Quit();
		}

	}
}