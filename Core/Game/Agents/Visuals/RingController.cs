using UnityEngine;
using System.Collections;
using Lockstep;
public class RingController : MonoBehaviour {
	private static Color SelectColor = new Color (0,1,0,.5f);
	private static Color HighlightColor = new Color (0,1,0,.25f);
	private static Color UnselectColor = new Color (0,1,0,.1f);
	public Renderer cachedRenderer;
	public Transform cachedTransform;
	public Material cachedMaterial;

	private LSAgent Agent;

	public void Initialize (LSAgent agent)
	{
		if (this.cachedRenderer == null)
			this.cachedRenderer = base.GetComponent<Renderer>();
		cachedMaterial = this.cachedRenderer.material;
		if (this.cachedTransform == null)
			this.cachedTransform = base.GetComponent<Transform> ();
		float scale = agent.SelectionRadius * 2;
		transform.localScale = new Vector3(scale,scale,scale);
		cachedMaterial.color = UnselectColor;
		Agent = agent;
	}

	public void Visualize ()
	{
		transform.position = Agent.transform.position;
	}

	public void Select ()
	{
		cachedMaterial.color = SelectColor;
	}

	public void Unselect ()
	{
		cachedMaterial.color = UnselectColor;
	}

	public void Highlight ()
	{
		cachedMaterial.color = HighlightColor;
	}
}
