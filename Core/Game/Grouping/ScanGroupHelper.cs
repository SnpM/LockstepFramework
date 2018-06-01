using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep.Data;
namespace Lockstep
{
	public class ScanGroupHelper : BehaviourHelper
	{
		public override ushort GetListenInput () {
            return AbilityDataItem.FindInterfacer(typeof (Scan)).ListenInputID;
		}
			
		protected override void OnExecute (Lockstep.Command com)
		{
            MovementGroupHelper.StaticExecute (com);
		}
	}
}