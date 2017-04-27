using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
    public class SelectionRing : MonoBehaviour
    {
		[SerializeField]
		private Color _selectedColor = new Color(0f, .6f, 0f);
		Color SelectedColor { get { return _selectedColor; } }

		[SerializeField]
		private Color _highlightedColor = new Color(0f, .7f, 0f, .5f);
		Color HighlightedColor { get { return _highlightedColor; } }

		[SerializeField]
		private Color _noneColor = new Color(0, 0, 0, 0);
		Color NoneColor { get { return _noneColor; } }

		Renderer cachedRenderer;

		public void Setup(float size)
		{
			this.OnSetup();
			this.SetSize(size);
			this.SetState(SelectionRingState.None);
		}
		protected virtual void OnSetup()
		{
			cachedRenderer = GetComponent<Renderer>();
		}

		private Color _tint;
		public Color Tint
		{
			get
			{
				return _tint;
			}
			set
			{
				if (_tint != value)
				{
					_tint = value;
					SetState(lastState);
				}
			}
		}

		SelectionRingState lastState;
		public void SetState(SelectionRingState state)
		{
			Color setColor = default(Color);
			switch (state)
			{
				case SelectionRingState.Selected:
					setColor = SelectedColor;
					break;
				case SelectionRingState.Highlighted:
					setColor = HighlightedColor;
					break;
				case SelectionRingState.None:
					setColor = NoneColor;
					break;
			}
			if (Tint != Color.clear)
				setColor = setColor / 4 + Tint;
			SetColor(setColor);
			lastState = state;
		}

		public virtual void SetColor(Color color)
		{
			cachedRenderer.material.color = color;

		}

		public virtual void SetSize(float size)
		{
			if (size <= 0.0f)
			{
				size = 1.0f;
			}
			transform.localScale = Vector3.one * size;
		}


	}
}