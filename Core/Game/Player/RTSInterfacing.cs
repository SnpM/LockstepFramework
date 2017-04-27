using UnityEngine;
using System.Collections; using FastCollections;
using System;
using UnityEngine.EventSystems;
namespace Lockstep
{
	public static class RTSInterfacing
	{
		public static Camera mainCamera { get { return Camera.main; } }

		public static void Initialize ()
		{
			CachedDidHit = false;
		}

		static bool agentFound;
		static float heightDif;
		static float closestDistance;
		static LSAgent closestAgent;

		public static LSAgent GetScreenAgent (Vector2 screenPos, Func<LSAgent, bool> conditional = null)
		{
			if (conditional == null)
				conditional = (agent) => {
					return true;};
			agentFound = false;
			Ray ray = Camera.main.ScreenPointToRay (screenPos);
			checkDir = ray.direction;
			checkOrigin = ray.origin;
			for (int i = 0; i < AgentController.PeakGlobalID; i++) {
				if (AgentController.GlobalAgentActive [i]) {
					LSAgent agent = AgentController.GlobalAgents [i];
					if (agent.IsVisible) {
						if (conditional (agent)) {
							if (AgentIntersects (agent)) {
								if (agentFound) {
									if (heightDif < closestDistance) {
										closestDistance = heightDif;
										closestAgent = agent;
									}
								} else {
									agentFound = true;
									closestAgent = agent;
									closestDistance = heightDif;
								}
							}
						}
					}
				}
			}
			if (agentFound)
				return closestAgent;
			return null;
		}

		public static void Visualize ()
		{

			if (mainCamera .IsNotNull ()) {
				CachedRay = mainCamera.ScreenPointToRay (Input.mousePosition);
				CachedDidHit = NDRaycast.Raycast (CachedRay, out CachedHit);

				MousedAgent = GetScreenAgent (Input.mousePosition, (agent) => {return true;});
			}
		}

		public static LSAgent MousedAgent { get; private set; }

		public static Ray CachedRay { get; private set; }

		public static Transform MousedObject { get { return CachedHit.transform; } }

		public static RaycastHit CachedHit;

		public static bool CachedDidHit { get; private set; }

		public static Vector2d GetWorldPosHeight (Vector2 screenPos, float height = 0) {
			if (Camera.main == null) return Vector2d.zero;
			Ray ray = Camera.main.ScreenPointToRay (screenPos);
			//RaycastHit hit;

			Vector3 hitPoint = ray.origin - ray.direction * ((ray.origin.y - height) / ray.direction.y);
			//return new Vector2d(hitPoint.x * LockstepManager.InverseWorldScale, hitPoint.z * LockstepManager.InverseWorldScale);
			return new Vector2d (hitPoint.x, hitPoint.z);
		}
		public static Vector2d GetWorldPosD (Vector2 screenPos)
		{
            Ray ray = Camera.main.ScreenPointToRay (screenPos);
			RaycastHit hit;
			if (NDRaycast.Raycast (ray, out hit)) {
				//return new Vector2d(hit.point.x * LockstepManager.InverseWorldScale, hit.point.z * LockstepManager.InverseWorldScale);
				return new Vector2d (hit.point.x, hit.point.z);
			}
			Vector3 hitPoint = ray.origin - ray.direction * (ray.origin.y / ray.direction.y);
			//return new Vector2d(hitPoint.x * LockstepManager.InverseWorldScale, hitPoint.z * LockstepManager.InverseWorldScale);
			return new Vector2d (hitPoint.x, hitPoint.z);
		}
		
		public static Vector2 GetWorldPos (Vector2 screenPos)
		{
			if (mainCamera == null)
				return default (Vector2);
			Ray ray = mainCamera.ScreenPointToRay (screenPos);
			RaycastHit hit;
			if (NDRaycast.Raycast (ray, out hit)) {
				//return new Vector2(hit.point.x * LockstepManager.InverseWorldScale, hit.point.z * LockstepManager.InverseWorldScale);
				return new Vector2 (hit.point.x, hit.point.z);
			}
			Vector3 hitPoint = ray.origin - ray.direction * (ray.origin.y / ray.direction.y);
			//return new Vector2(hitPoint.x * LockstepManager.InverseWorldScale, hitPoint.z * LockstepManager.InverseWorldScale);
			return new Vector2 (hitPoint.x, hitPoint.z);
		}

		public static Vector3 GetWorldPos3 (Vector2 screenPos)
		{
			Ray ray = mainCamera.ScreenPointToRay (screenPos);
			RaycastHit hit;
			if (NDRaycast.Raycast (ray, out hit)) {
				//return new Vector2(hit.point.x * LockstepManager.InverseWorldScale, hit.point.z * LockstepManager.InverseWorldScale);
				return hit.point;
			}
			Vector3 hitPoint = ray.origin - ray.direction * (ray.origin.y / ray.direction.y);
			//return new Vector2(hitPoint.x * LockstepManager.InverseWorldScale, hitPoint.z * LockstepManager.InverseWorldScale);
			return hitPoint;
		}

		public static void ActivateMarkerOnMouse (MarkerType markerType)
		{
            #if false
            if (CachedDidHit) {
                PlayerManager.OrderMarker.PlayOneShot (CachedHit.point, CachedHit.normal, markerType);
                return;
            }
            Vector3 hitPoint = CachedRay.origin - CachedRay.direction * (CachedRay.origin.y / CachedRay .direction.y);
                     if (PlayerManager.OrderMarker != null)
            PlayerManager.OrderMarker.PlayOneShot (hitPoint, Vector3.up, markerType);
            #endif
		}

		private static Vector3 checkDir;
		private static Vector3 checkOrigin;

		private static bool AgentIntersects (LSAgent agent)
		{
			if (agent.IsVisible) {
				Vector3 agentPos = agent.VisualCenter.position;
				heightDif = checkOrigin.y - agentPos.y;
				float scaler = heightDif / checkDir.y;
				Vector2 levelPos;
				levelPos.x = (checkOrigin.x - (checkDir.x * scaler)) - agentPos.x;
				levelPos.y = (checkOrigin.z - (checkDir.z * scaler)) - agentPos.z;

				if (levelPos.sqrMagnitude <= agent.SelectionRadiusSquared) {
					return true;
				}
			}
			return false;
		}
	}
}