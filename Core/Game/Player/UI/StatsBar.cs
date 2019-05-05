﻿using UnityEngine;
using System;

namespace Lockstep.UI
{
	public class StatsBar : MonoBehaviour
	{
		[SerializeField] private BarElement _shield;
		[SerializeField] private BarElement _health;
		[SerializeField] private BarElement _energy;

		Vector3 _offset;
		Vector3 Offset { get { return _offset; } }

		public LSAgent TrackedAgent { get; private set; }

		void Awake()
		{
			_offset = Vector3.zero;
			gameObject.SetActive(false);
		}

		public void Setup(LSAgent agent)
		{
			TrackedAgent = agent;
			GameObject.DontDestroyOnLoad(gameObject);
			this.gameObject.name = agent.ToString();
		}

		private static StatBarType[] statTypes = (StatBarType[])Enum.GetValues(typeof(StatBarType));

		public void Initialize()
		{
			gameObject.SetActive(true);
			foreach (StatBarType statType in statTypes)
			{
				SetFill(statType, 1f);
			}
			UpdatePos();
			UpdateScale();
		}

		public void Visualize()
		{
			if (RTSInterfacingHelper.GUIManager.CameraChanged && TrackedAgent.IsVisible)
			{
				this.UpdatePos();
				this.UpdateScale();
			}
			else if (TrackedAgent.VisualPositionChanged)
			{
				this.UpdatePos();
			}
		}

		static GUIManager GUIManager { get { return RTSInterfacingHelper.GUIManager; } }
		static Camera MainCam { get { return GUIManager.MainCam; } }

		static Vector3 screenPos;

		private void UpdatePos()
		{
			if (GUIManager.CanHUD == false) return;
			screenPos = MainCam.WorldToScreenPoint(TrackedAgent.VisualCenter.position + Offset);
			transform.position = screenPos;
		}

		static Vector3 tempScale;

		private void UpdateScale()
		{
			if (GUIManager.CanHUD == false) return;
			float scale = Mathf.Max(TrackedAgent.SelectionRadius / 1f, .1f);
			tempScale.x = GUIManager.CameraScale * scale;
			tempScale.y = GUIManager.CameraScale;
			transform.localScale = tempScale;
		}

		public void SetFill(StatBarType statType, float amount)
		{
			BarElement element = null;
			switch (statType)
			{
				case StatBarType.Shield:
					element = _shield;
					break;
				case StatBarType.Health:
					element = _health;
					break;
				case StatBarType.Energy:
					element = _energy;
					break;
			}
			if (TrackedAgent.IsVisible == false || (amount >= 1f && !GUIManager.ShowHealthWhenFull))
			{

				element.gameObject.SetActive(false);
				return;
			}
			{
				element.SetFill(amount);
				element.gameObject.SetActive(true);
			}
		}

		public void Deactivate()
		{
			if (gameObject == null) return;
			gameObject.SetActive(false);
		}
	}

	public enum StatBarType
	{
		Shield,
		Health,
		Energy
	}
}