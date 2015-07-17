using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lockstep
{
	public static class SelectionManager
	{
		public static bool Changed;

		public const int MaximumSelection = 512;
		public static LSAgent MousedAgent;

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
		private static float BoxingTime;
		private const float MinimumClickTimeToBox = .1f;
		private const float MinimumBoxArea = 2;

		public static void Initialize ()
		{
			ClearSelection ();
		}

		public static void Simulate ()
		{
			Changed = false;
		}

		public static bool CanClearSelection;
		public static void Update ()
		{
			MousePosition = Input.mousePosition;
			MouseWorldPosition = GetWorldPos (MousePosition);
			CanClearSelection = !Input.GetKey (KeyCode.LeftShift);


			if (Boxing) {
				BoxingTime += Time.deltaTime;
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

				MousedAgent = null;
				ClearBox ();

				for (i = 0; i < PlayerManager.agentControllers.Count; i++) {
					AgentController agentController = PlayerManager.agentControllers [i];
					foreach (LSAgent agent in agentController.ActiveAgents.Values) {
						if (agent.renderer.isVisible) {

							if (MousedAgent == null)
							{
								dif = new Vector2(
									agent.transform.position.x - MouseWorldPosition.x,
									agent.transform.position.z - MouseWorldPosition.y);
								if ((dif.x * dif.x + dif.y * dif.y) <= (agent.SelectionRadius * agent.SelectionRadius))
								{
									MouseOver(agent);
									BoxAgent (agent);
									continue;
								}
							}

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
											BoxAgent (agent);
										}
									}
								}
							}
						}
					}
				}

				if (Input.GetMouseButtonUp (0)) {

					if (CanClearSelection)
						ClearSelection ();
					SelectBoxedAgents ();
					Boxing = false;
					ClearBox ();

				}

			} else if (InitializeBoxing) {
				GetMousedAgent ();
				InitializeTime += Time.deltaTime;
				if (InitializeTime >= MinimumClickTimeToBox) {
					if (Input.GetMouseButton (0)) {
						BoxEnd = MousePosition;
						float Area = BoxEnd.x > BoxStart.x ? BoxEnd.x - BoxStart.x : BoxStart.x - BoxEnd.x;
						float AreaMul = BoxEnd.y > BoxStart.y ? BoxEnd.y - BoxStart.y : BoxStart.y - BoxEnd.y;
						Area *= AreaMul;
						if (Area >= MinimumBoxArea) {
							Boxing = true;
							InitializeBoxing = false;
							InitializeTime = 0f;
						}
						else {
							return;
						}
					}
					else {
						QuickSelect ();
					}
					InitializeBoxing = false;
					InitializeTime = 0f;
				}
				else {
					GetMousedAgent ();
					if (Input.GetMouseButtonUp (0))
					{
						QuickSelect ();
					}
				}
			} else {
				GetMousedAgent ();
				if (Input.GetMouseButtonDown (0)) {
					InitializeBoxing = true;
					BoxStart = MousePosition;
				}
			}

		}

		public static void QuickSelect ()
		{
			if (CanClearSelection)
				ClearSelection ();
			SelectAgent (MousedAgent);
		}

		public static void SelectAgent (LSAgent agent)
		{
			if (agent == null) return;
			SelectedAgents.Add (agent);
			agent.IsSelected = true;
		}

		public static void UnselectAgent (LSAgent agent)
		{
			if (agent == null) return;
			SelectedAgents.Remove (agent);
			agent.IsSelected = false;
		}

		public static void BoxAgent (LSAgent agent)
		{
			if (agent == null) return;
			BoxedAgents.Add (agent);
			agent.IsHighlighted = true;
		}

		public static void UnboxAgent (LSAgent agent)
		{
			if (agent == null) return;
			BoxedAgents.Remove (agent);
			agent.IsHighlighted = false;
		}
		private static void SelectBoxedAgents ()
		{
			for (i = 0; i < BoxedAgents.PeakCount; i++)
			{
				if (BoxedAgents.arrayAllocated[i])
					SelectAgent(BoxedAgents.innerArray[i]);
			}
		}
		public static void ClearSelection ()
		{
			for (i = 0; i < SelectedAgents.PeakCount; i++)
			{
				if (SelectedAgents.arrayAllocated[i])
					SelectedAgents.innerArray[i].IsSelected = false;
			}
			SelectedAgents.FastClear ();
		}
		public static void ClearBox ()
		{
			for (i = 0; i < BoxedAgents.PeakCount; i++)
			{
				if (BoxedAgents.arrayAllocated[i])
					BoxedAgents.innerArray[i].IsHighlighted = false;
			}
			BoxedAgents.FastClear ();
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
			ray = PlayerManager.mainCamera.ScreenPointToRay (ScreenPos);
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
		static Vector2 dif;
		private static void GetMousedAgent ()
		{
			if (MousedAgent != null)
			{
				MousedAgent.IsHighlighted = false;
				MousedAgent = null;
			}
			for (i = 0; i < PlayerManager.agentControllers.Count; i++) {
				AgentController agentController = PlayerManager.agentControllers [i];
				foreach (LSAgent agent in agentController.ActiveAgents.Values) {
					if (agent.renderer.isVisible)
					{
						dif = new Vector2(
							agent.transform.position.x - MouseWorldPosition.x,
						    agent.transform.position.z - MouseWorldPosition.y);
						if ((dif.x * dif.x + dif.y * dif.y) <= (agent.SelectionRadius * agent.SelectionRadius))
						{
							MouseOver (agent);
							break;
						}
					}
				}
			}
		}
		private static void MouseOver (LSAgent agent)
		{
			MousedAgent = agent;
			agent.IsHighlighted = true;
		}
	}
}