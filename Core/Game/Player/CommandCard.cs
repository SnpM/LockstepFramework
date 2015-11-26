using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using Lockstep.Data;
namespace Lockstep {
	[ExecuteInEditMode]
	public class CommandCard : MonoBehaviour {
		void Start () {

		}

		public const int Width = 4;
		public const int Height = 3;


		static CommandTile[] Tiles;
		public void Setup () {
			Tiles = GetComponentsInChildren<CommandTile> ();
		
			for (int i = 0; i < Tiles.Length; i++)
			{
				CommandTile tile = Tiles[i];
				if (tile .IsNotNull ())
				{
					tile.Setup (i);
					tile.OnActivate += HandleTileActivation;
				}
			}
		}

		private static void HandleTileActivation (int tileID) {

		}

		public static void Inject (FastList<AbilityInterfacer> interfacers)
		{
			for (int i = 0; i < interfacers.Count; i++)
			{

				AbilityInterfacer facer = interfacers[i];
				int tileIndex = facer.TileIndex;
                if (Tiles == null || tileIndex >= 12 || tileIndex >= Tiles.Length) continue;

				Tiles[tileIndex].Interfacer = (facer);
			}
		}

		public static void Visualize () {
			if (Tiles .IsNotNull ()) {
				for (int i = 0; i < Tiles.Length; i++)
				{
					if (Tiles[i] .IsNotNull ())
					Tiles[i].Visualize ();
				}
			}
		}

		public static void Reset ()
		{
			if (Tiles .IsNotNull ())
			for (int i = 0; i < Tiles.Length; i++)
			{
				Tiles[i].ResetTile ();
			}
		}


	}
}