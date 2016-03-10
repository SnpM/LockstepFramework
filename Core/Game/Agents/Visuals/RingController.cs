using UnityEngine;
namespace Lockstep {
	public class RingController : MonoBehaviour {
		public bool IsBuilding = false;
		static RingController () {
		}
		private static GameObject ringTemplateBuilding;
		private static GameObject ringTemplateUnit;
		public static RingController Create (int type) {
			GameObject go;
			// CREATE RINGCONTROLLER EITHER BUILDING OR UNIT
			if (type == 0) {
				ringTemplateUnit = LSUtility.ResourceLoadGO("UnitSelectionRing");
				go = GameObject.Instantiate<GameObject>(ringTemplateUnit);
			}
			else {
				ringTemplateBuilding = LSUtility.ResourceLoadGO("BuildingSelectionRing");
				go = GameObject.Instantiate<GameObject>(ringTemplateBuilding);
			}
			//=========
			RingController ringer = go.GetComponent<RingController>();
			return ringer;
		}
		public Color[] colors = new Color[3];
		private Projector cachedProjector;
		private Material cachedMaterial;
		public Transform ProjectorComponent;
		public void Setup (LSAgent agent) {
			cachedProjector = ProjectorComponent.GetComponent<Projector>();
			cachedMaterial = cachedProjector.material;
			transform.parent = agent.VisualCenter;
			transform.localPosition = Vector3.zero;
			//          float size = agent.SelectionRadius * 2;
			float size = agent.Body.HalfWidth.ToFloat() * 2f;
			cachedProjector.orthographicSize = size + 1;
			transform.localScale = new Vector3(size, size, 1);
		}
		public void Initialize () {
			Unselect();
		}
		public void Select () {
			if (cachedProjector == null) return;
			cachedProjector.enabled = true;
			cachedMaterial.color = colors[0];
		}
		public void Highlight () {
			if (cachedProjector == null) return;
			cachedProjector.enabled = true;
			cachedMaterial.color = colors[1];
		}
		public void Unselect () {
			if (cachedProjector == null) return;
			cachedProjector.enabled = false;
			cachedMaterial.color = colors[2];
		}
		public void Deactivate () {
			Unselect();
		}
		public bool IsEnabled {
			get { return cachedProjector.enabled; }
		}
	}
}