using UnityEngine;
using System.Collections;
using Lockstep;
public class RingController : MonoBehaviour {
	private static Color SelectColor = new Color (0,1,0,.5f);
	private static Color HighlightColor = new Color (0,1,0,.25f);
	private static Color UnselectColor = new Color (0,1,0,.1f);
	public Renderer renderer;
	public Transform transform;
	public Material material;

	private LSAgent Agent;

	public void Initialize (LSAgent agent)
	{
		if (this.renderer == null)
			this.renderer = base.GetComponent<Renderer>();
		material = this.renderer.material;
		if (this.transform == null)
			this.transform = base.GetComponent<Transform> ();
		float scale = agent.SelectionRadius * 2;
		transform.localScale = new Vector3(scale,scale,scale);
		material.color = UnselectColor;
		Agent = agent;
	}

	public void Visualize ()
	{

			transform.position = Agent.transform.position;
	}

	public void Select ()
	{
		material.color = SelectColor;
	}

	public void Unselect ()
	{
		material.color = UnselectColor;
	}

	public void Highlight ()
	{
		material.color = HighlightColor;
	}
}
