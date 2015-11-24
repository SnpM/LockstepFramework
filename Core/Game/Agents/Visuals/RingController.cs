using UnityEngine;

namespace Lockstep {
    public class RingController : MonoBehaviour {
		static RingController () {
			ringTemplate = LSUtility.ResourceLoadGO ("SelectionRing");
		}
		private static GameObject ringTemplate;
		public static RingController Create () {
			GameObject go = GameObject.Instantiate <GameObject> (ringTemplate);
			RingController ringer = go.GetComponent<RingController> ();
			return ringer;
		}

        [SerializeField]
        private readonly Color SelectColor = new Color(1, 1, 1, .35f);
        [SerializeField]
        private readonly Color HighlightColor = new Color(1, 1, 1, .2f);
        [SerializeField]
        private readonly Color UnselectColor = new Color(1, 1, 1, .1f);

        private Renderer cachedRenderer;
        private Material cachedMaterial;

		public void Setup(LSAgent agent) {
			cachedRenderer = GetComponent<Renderer>();
			cachedMaterial = cachedRenderer.material;
			transform.parent = agent.VisualCenter;
			transform.localPosition = Vector3.zero;
			float size = agent.SelectionRadius * 2;
			transform.localScale = new Vector3(size,size,1);
		}

        public void Initialize() {
			Unselect ();
        }

        public void Select() {
            cachedRenderer.enabled = true;
            cachedMaterial.color = SelectColor;
        }

        public void Highlight() {
            cachedRenderer.enabled = true;
            cachedMaterial.color = HighlightColor;
        }

        public void Unselect() {
            if (cachedRenderer == null) return;
            cachedRenderer.enabled = false;
            cachedMaterial.color = UnselectColor;
        }

        public void Deactivate() {
			Unselect ();
        }

        public bool IsEnabled {
            get { return cachedRenderer.enabled; }
        }
    }
}