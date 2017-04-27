using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
	public class LSParticler : MonoBehaviour
	{
		LSAnimatorBase animator;

		void Awake ()
		{
			animator = GetComponent<LSAnimatorBase> ();
			animator.OnStatePlay += HandleOnStatePlay;
			animator.OnImpulsePlay += HandleOnImpulsePlay;
		}

		void HandleOnImpulsePlay (AnimImpulse obj, int rate)
		{
		}

		void HandleOnStatePlay (AnimState obj)
		{

		}
	}
}