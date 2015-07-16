using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lockstep
{
	public static class SelectionManager
	{

		public static FastList<LSAgent> SelectedUnits = new FastList<LSAgent> ();
		public static Vector2 MousePosition;
		public static Vector2 MouseWorldPosition;
		public static Vector2 BoxStart;
		public static Vector2 BoxEnd;
		public static Vector2 Box_TopLeft;
		public static Vector2 Box_TopRight;
		public static Vector2 Box_BottomLeft;
		public static Vector2 Box_BottomRight;
		public static bool Boxing;
		private static bool InitializeBoxing;
		private static float InitializeTime;
		private const float MinimumClickTimeToBox = .1f;
		private const float MinimumBoxArea = 16;

		public static void Initialize ()
		{
			SelectedUnits.FastClear ();
		}

		public static void Simulate ()
		{

		}

		public static void Visualize ()
		{
			MousePosition = Input.mousePosition;

			if (Boxing) {
				if (MousePosition != BoxEnd) {
					Vector2 RaycastTopLeft;
					Vector2 RaycastTopRight;
					Vector2 RaycastBotLeft;
					Vector2 RaycastBotRight;

					BoxEnd = MousePosition;
					if (BoxStart.x < BoxEnd.x) {
						RaycastTopLeft.x = BoxStart.x;
						RaycastBotLeft.x = BoxStart.x;
						RaycastTopRight.x = BoxEnd.x;
						RaycastBotRight.x = BoxEnd.x;
					} else {
						RaycastTopLeft.x = BoxEnd.x;
						RaycastBotLeft.x = BoxEnd.x;
						RaycastTopRight.x = BoxStart.x;
						RaycastBotRight.x = BoxStart.x;
					}
					if (BoxStart.y < BoxEnd.y) {
						RaycastBotLeft.y = BoxStart.y;
						RaycastBotRight.y = BoxStart.y;
						RaycastTopLeft.y = BoxEnd.y;
						RaycastTopRight.y = BoxEnd.y;
					} else {
						RaycastBotLeft.y = BoxEnd.y;
						RaycastBotRight.y = BoxEnd.y;
						RaycastTopLeft.y = BoxStart.y;
						RaycastTopRight.y = BoxStart.y;
					}

					Box_TopLeft = GetWorldPos (RaycastTopLeft);
					Box_TopRight = GetWorldPos (RaycastTopRight);
					Box_BottomLeft = GetWorldPos (RaycastBotLeft);
					Box_BottomRight = GetWorldPos (RaycastBotRight);
				}

				SelectedUnits.FastClear ();
				for (i = 0; i < PlayerController.agentControllers.Count; i++) {
					AgentController agentController = PlayerController.agentControllers [i];
					foreach (LSAgent agent in agentController.ActiveAgents.Values) {
						if (agent.renderer.isVisible) {
							Vector2 agentPos = new Vector2 (agent.transform.position.x, agent.transform.position.z);
							Edge = Box_TopRight - Box_TopLeft;
							Point = agentPos - Box_TopLeft;
							if (DotEdge () < 0) {
								Edge = Box_BottomRight - Box_TopRight;
								Point = agentPos - Box_TopRight;
								if (DotEdge () < 0) {
									Edge = Box_BottomLeft - Box_BottomRight;
									Point = agentPos - Box_BottomRight;
									if (DotEdge () < 0) {
										Edge = Box_TopLeft - Box_BottomLeft;
										Point = agentPos - Box_BottomLeft;
										if (DotEdge () < 0) {
											SelectedUnits.Add (agent);
											Debug.Log (agent);
											continue;
										}
									}
								}
							}
						}
					}
				}
				if (Input.GetMouseButtonUp (0)) {
					Boxing = false;
				}
			} else if (InitializeBoxing) {
				InitializeTime += Time.deltaTime;
				if (InitializeTime >= MinimumClickTimeToBox) {
					if (Input.GetMouseButton (0)) {
						BoxEnd = MousePosition;
						float Area = BoxEnd.x > BoxStart.x ? BoxEnd.x - BoxStart.x : BoxStart.x - BoxEnd.x;
						if (BoxEnd.y > BoxStart.y)
							Area *= (BoxEnd.y - BoxStart.y);
						else
							Area *= (BoxStart.y - BoxEnd.y);
						if (Area >= MinimumBoxArea) {
							Boxing = true;
							InitializeBoxing = false;
						}
					} else {
						InitializeBoxing = false;
						SelectMouseOverUnit ();
					}
				}
			} else {
				if (Input.GetMouseButtonDown (0)) {
					InitializeBoxing = true;
					BoxStart = MousePosition;
				}
			}
		}

		public static void SelectMouseOverUnit ()
		{

		}

		public static void DrawRealWorldBox ()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawCube (new Vector3 (Box_TopLeft.x, 0, Box_TopLeft.y), Vector3.one);
			Gizmos.color = Color.green;
			Gizmos.DrawCube (new Vector3 (Box_TopRight.x, 0, Box_TopRight.y), Vector3.one);
			Gizmos.color = Color.blue;
			Gizmos.DrawCube (new Vector3 (Box_BottomRight.x, 0, Box_BottomRight.y), Vector3.one);
			Gizmos.color = Color.white;
			Gizmos.DrawCube (new Vector3 (Box_BottomLeft.x, 0, Box_BottomLeft.y), Vector3.one);
		}

		public static void DrawBox (GUIStyle style)
		{
			if (Boxing) {
				Vector2 Size = BoxEnd - BoxStart;
				GUI.Box (new Rect (BoxStart.x, Screen.height - BoxStart.y, Size.x, -Size.y), "", style);
			}
		}

		static int i, j;
		static RaycastHit hit;
		static Ray ray;

		private static Vector2 GetWorldPos (Vector2 ScreenPos)
		{
			ray = PlayerController.mainCamera.ScreenPointToRay (ScreenPos);
			if (Physics.Raycast (
				ray,
				out hit)) {
				return new Vector2 (hit.point.x, hit.point.z);
			} else {
				Vector3 hitPoint = ray.origin - ray.direction * (ray.origin.y / ray.direction.y);
				return new Vector2 (hitPoint.x, hitPoint.z);
			}
		}

		static Vector2 Point;
		static Vector2 Edge;

		static float DotEdge ()
		{
			return Point.x * -Edge.y + Point.y * Edge.x;
		}
	}
}