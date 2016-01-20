using UnityEngine;
using System.Collections;
using Lockstep.Data;
namespace Lockstep
{
	public class ScanGroupHelper : BehaviourHelper
	{
		public override ushort ListenInput {
			get {
                return AbilityInterfacer.FindInterfacer(typeof (Scan)).ListenInputID;
			}
		}

		protected override void OnExecute (Command com)
		{
			MovementGroupHelper.Instance.Execute (com);
		}
	}
}