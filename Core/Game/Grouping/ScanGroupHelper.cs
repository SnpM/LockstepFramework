using UnityEngine;
using System.Collections;
using Lockstep.Data;
namespace Lockstep
{
	public class ScanGroupHelper : BehaviourHelper
	{
		public override InputCode ListenInput {
			get {
                return AbilityInterfacer.FindInterfacer(typeof (Scan)).ListenInput;
			}
		}

		protected override void OnExecute (Lockstep.Command com)
		{
			MovementGroupHelper.Execute (com);
		}
	}
}