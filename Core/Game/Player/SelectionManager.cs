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
		private static float BoxingTime;
		private static bool CheckBoxDistance;
		private const float MinBoxSqrDist = 4;

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
			GetMousedAgent ();



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

				ClearBox ();

				int lecount = 0;
				if ((BoxEnd - BoxStart).sqrMagnitude >= MinBoxSqrDist)
				for (i = 0; i < PlayerManager.agentControllers.Count; i++) {
					AgentController agentController = PlayerManager.agentControllers [i];
					for (j = 0; j < AgentController.MaxAgents; j++) {
						if (agentController.AgentActive [j]) {
							curAgent = agentController.Agents [j];
							if (curAgent.cachedRenderer.isVisible) {

								Vector2 agentPos = new Vector2 (curAgent.transform.position.x, curAgent.transform.position.z);
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
												BoxAgent (curAgent);
												continue;
											}
										}
									}
								}
							}
							if (curAgent.BoxVersion == BoxedAgents._BufferVersion)
							{
								curAgent.IsHighlighted = false;
							}
						}
					}
				}

				if (Input.GetMouseButtonUp (0)) {

					if (CanClearSelection) {
						ClearSelection ();
					}
					SelectBoxedAgents ();
					SelectAgent (MousedAgent);

					Boxing = false;
				}

			} else {
				if (Input.GetMouseButtonDown (0)) {
					CheckBoxDistance = true;
					Boxing = true;
					BoxingTime = 0f;
					BoxStart = MousePosition;
					BoxEnd = MousePosition;
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
			if (agent == null)
				return;
			SelectedAgents.Add (agent);
			agent.IsSelected = true;
		}

		public static void UnselectAgent (LSAgent agent)
		{
			if (agent == null)
				return;
			SelectedAgents.Remove (agent);
			agent.IsSelected = false;
		}

		public static void BoxAgent (LSAgent agent)
		{
			if (System.Object.ReferenceEquals (agent,null))
				return;
			BoxedAgents.Add (agent);
			agent.IsHighlighted = true;
		}

		private static void SelectBoxedAgents ()
		{
			for (i = 0; i < BoxedAgents.Count; i++) {
				SelectAgent (BoxedAgents.innerArray [i]);
			}
		}

		public static void ClearSelection ()
		{
			for (i = 0; i < SelectedAgents.PeakCount; i++) {
				if (SelectedAgents.arrayAllocated [i])
					SelectedAgents.innerArray [i].IsSelected = false;
			}
			SelectedAgents.FastClear ();
		}

		public static void ClearBox ()
		{
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
		static LSAgent curAgent;
		static RaycastHit hit;
		static Ray ray;

		public static Vector2 GetWorldPos (Vector2 ScreenPos)
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

			for (i = 0; i < AgentController.InstanceManagers.Count; i++) {
				AgentController agentController = AgentController.InstanceManagers [i];
				for (j = 0; j < AgentController.MaxAgents; j++) {
					if (agentController.AgentActive [j]) {
						curAgent = agentController.Agents [j];
						if (curAgent.cachedRenderer.isVisible) {
							dif = new Vector2 (
								curAgent.transform.position.x - MouseWorldPosition.x,
							    curAgent.transform.position.z - MouseWorldPosition.y);
							if ((dif.x * dif.x + dif.y * dif.y) <= (curAgent.SelectionRadius * curAgent.SelectionRadius)) {
								MouseOver (curAgent);
								return;
							}
						}
					}
				}
			}
			if (System.Object.ReferenceEquals (MousedAgent, null)==false) {
				MousedAgent.IsHighlighted = false;
				MousedAgent = null;
			}
		}

		private static void MouseOver (LSAgent agent)
		{
			if (System.Object.ReferenceEquals (MousedAgent,agent))
			{
				return;
			}
			if (System.Object.ReferenceEquals (MousedAgent, null)==false) {
				MousedAgent.IsHighlighted = false;
			}
			MousedAgent = agent;
			agent.IsHighlighted = true;
		}
	}
}