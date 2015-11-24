using UnityEngine;
using System.Collections;

namespace Lockstep
{
	public class Build : ActiveAbility
	{
		[SerializeField]
		private BuildNode[] _tiles;
		public BuildNode[] Tiles {get {return _tiles;}}

		public bool Disabled {get; set;}

		protected override void OnSetup ()
		{
			for (ushort i = 0; i < Tiles.Length; i++) {
				Tiles[i].Setup (i, this);
			}

		}

		protected override void OnInitialize ()
		{
			for (int i = 0; i < Tiles.Length;i ++) {
				Tiles[i].Initialize();
			}
			Disabled = false;
            /*foreach (BuildNode node in _tiles) {
                node.SendBuild(1);
            }*/
		}

		protected override void OnExecute (Command com)
		{
			if (com.HasCount && com.HasTarget)
			{
				Tiles[com.Target].Execute (com);
			}
		}
#if UNITY_EDITOR
        protected override void OnAfterSerialize  ()
		{
			BuildNode[] tempTiles = Agent.GetComponentsInChildrenOrderered<BuildNode> ();
			if (tempTiles .IsNotNull ()) _tiles = tempTiles;
		}
#endif
	}
}