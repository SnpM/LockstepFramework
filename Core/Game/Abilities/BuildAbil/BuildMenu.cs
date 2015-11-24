using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Lockstep;
public class BuildMenu : CerealBehaviour {
	private LSUIWindow window;
	public BuildNode CurrentBuildNode {get; private set;}
	[SerializeField]
	private BuildMenuTile[] _tiles;
	BuildMenuTile[] Tiles {get {return _tiles;}}
	private static BuildMenu instance;
	public void Setup () {
		window = GetComponent<LSUIWindow> ();
		instance = this;
		for (int i = 0; i < _tiles.Length; i++) {
			_tiles[i].Setup (i);
		}
	}
	public void Initialize () {
		StartCoroutine (StartClose ());
    }
	IEnumerator StartClose () {
		yield return null;
		Close ();
	}
	public void Open (BuildNode node)
	{

		CurrentBuildNode = node;
		window.Show ();

		for (int i = 0; i < node.SpawnableAgents.Length; i++) {
			Tiles[i].Open (node.SpawnableAgents[i]);
		}
	}
	public void Close () {
		window.Hide ();
		foreach (BuildMenuTile tile in Tiles) {
			tile.Close ();
		}

	}
	public static void Press (int index) {
		instance.LocalPress (index);
	}
	private void LocalPress (int index) {
		CurrentBuildNode.SendBuild (index);
		Close ();
	}

#if UNITY_EDITOR
    protected override void OnAfterSerialize  ()
	{
		_tiles = this.GetComponentsInChildrenOrderered<BuildMenuTile> () ?? _tiles;
	} 
#endif
}
