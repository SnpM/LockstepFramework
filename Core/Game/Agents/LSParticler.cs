using UnityEngine;

namespace Lockstep
{
	public class LSParticler : MonoBehaviour
	{
		LSAnimatorBase animator;

		void Awake()
		{
			animator = GetComponent<LSAnimatorBase>();
			animator.onStatePlay += HandleOnStatePlay;
			animator.onImpulsePlay += HandleOnImpulsePlay;
		}

		void HandleOnImpulsePlay(AnimImpulse obj)
		{
		}

		void HandleOnStatePlay(AnimState obj)
		{

		}
	}
}