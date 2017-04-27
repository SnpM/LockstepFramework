using UnityEngine;
using System.Collections; using FastCollections;
using UnityEngine.UI;
public class LSUIWindow : MonoBehaviour {
	private CanvasGroup canvasGroup;
    [SerializeField]
    bool _startHidden;
	[SerializeField]
	float _fadeTime = .5f;
	float FadeTime {get {return _fadeTime;}}
	void Awake () {
		canvasGroup = GetComponent<CanvasGroup> () ?? gameObject.AddComponent<CanvasGroup> ();
		isAdjustingAlpha = true;
        if (_startHidden)
            targetAlpha = 0f;
        else
		    targetAlpha = 1f;
	}

	bool isAdjustingAlpha;
	float targetAlpha;
	float TargetAlpha {
		set {
			if (value != targetAlpha)
			{
			targetAlpha = value;
			isAdjustingAlpha = true;
			}
		}
	}
	void LateUpdate () {
		if (isAdjustingAlpha) {
			if (canvasGroup.alpha > targetAlpha) {
				canvasGroup.alpha -= Time.unscaledDeltaTime * 1f / FadeTime;
				if (canvasGroup.alpha <= targetAlpha) isAdjustingAlpha = false;
			}
			else {
				canvasGroup.alpha += Time.unscaledDeltaTime * 1f / FadeTime;
				if (canvasGroup.alpha >= targetAlpha) isAdjustingAlpha = false;
			}
		}
	}
	public void Hide () {
		TargetAlpha = 0f;
		canvasGroup.blocksRaycasts = false;
	}
	public void Show () {
		TargetAlpha = 1f;
		canvasGroup.blocksRaycasts = true;
	}
}
