using UnityEngine;
using System.Collections;

namespace Lockstep
{
	public class LSParticler : MonoBehaviour
	{
		LSAnimator animator;

		void Awake ()
		{
			animator = GetComponent<LSAnimator> ();
			animator.OnStatePlay += HandleOnStatePlay;
			animator.OnImpulsePlay += HandleOnImpulsePlay;
		}

		void HandleOnImpulsePlay (AnimImpulse obj)
		{
			switch (obj)
			{

			}
		}

		void HandleOnStatePlay (AnimState obj)
		{

		}
	}
}