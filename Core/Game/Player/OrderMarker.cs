﻿using UnityEngine;

public class OrderMarker : MonoBehaviour
{
	private Material material;

	private static Color NeutralColor = Color.green;
	private static Color FriendlyColor = Color.green;
	private static Color AggresiveColor = Color.red;
	//private static Vector3 InactivePosition = new Vector3(0,10000,0);
	bool hasRenderer { get; set; }
	void Awake()
	{
		var renderer = GetComponent<Renderer>();
		hasRenderer = renderer != null;
		if (hasRenderer)
		{
			material = renderer.material;
		}
	}

	public void PlayOneShot(Vector3 pos, Vector3 norm, MarkerType markerType)
	{
		if (markerType == MarkerType.None) return;

		var renderer = GetComponent<Renderer>();
		renderer.gameObject.SetActive(true);

		transform.position = pos;
		if (!hasRenderer) return;
		switch (markerType)
		{
			case MarkerType.Neutral:
				material.color = NeutralColor;
				break;
			case MarkerType.Friendly:
				material.color = FriendlyColor;
				break;
			case MarkerType.Aggresive:
				material.color = AggresiveColor;
				break;
		}

		var animation = GetComponent<Animation>();

		animation.Stop();
		animation.Play();
		transform.up = norm;
		//UnityEditor.EditorApplication.isPaused = true;
	}
}

public enum MarkerType
{
	None,
	Friendly,
	Neutral,
	Aggresive
}