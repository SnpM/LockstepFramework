using UnityEngine;
using System.Collections;

namespace Lockstep
{
	public class ScanGroupHandler : BehaviourHelper
	{
		public override InputCode ListenInput {
			get {
				return InputCode.C1;
			}
		}

		protected override void OnExecute (Lockstep.Command com)
		{
			MovementGroupHandler.Execute (com);
		}
	}
}