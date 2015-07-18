using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using Lockstep.Integration;
#endif

namespace Lockstep
{
	public class LSBody : MonoBehaviour
	{
		#region User-defined Variables

		[SerializeField]
		public bool
			IsTrigger = false;
		[SerializeField]
		public bool
			Immovable = false;
		[SerializeField]
		public ColliderType
			Shape;
		[SerializeField]
		public long
			Radius = FixedMath.Half;
		[SerializeField]
		public long
			HalfWidth = FixedMath.Half;
		[SerializeField]
		public long
			HalfHeight = FixedMath.Half;
		[SerializeField]
		public Vector2d[]
			Vertices;

		#endregion

		#region Meta Variables
		
		public int
			ID;
		public Transform
			transform;
		public GameObject
			gameObject;
		private bool FirstInitialize = false;
		public delegate void CollisionFunction (LSBody other);

		public CollisionFunction OnContactEnter;
		public CollisionFunction OnContact;
		public CollisionFunction OnContactExit;
		public Move Mover;
		#endregion

		#region State Information
		[SerializeField]
		public Vector2d
			Position;
		[SerializeField]
		public Vector2d
			Rotation;
		public Vector2d
			Velocity;
		public long VelocityMagnitude;
		public bool VelocityChanged;
		public bool PositionChanged;
		public bool RotationChanged;
		public bool PositionChangedBuffer;
		public bool RotationChangedBuffer;
		public int[] LocatedPartitions = new int[4];
		public bool[] LocatedPartitionExists = new bool[4];
		private Vector3 curPos;
		private Vector3 lastPos;
		private Vector3 lastRot = Vector3.forward;
		private Vector3 curRot = Vector3.forward;
		#endregion

		public void Initialize ()
		{
			Initialize (Vector2d.zero, Vector2d.up);
		}

		public void Initialize (Vector2d StartPosition)
		{
			Initialize (StartPosition, Vector2d.up);
		}

		public void Initialize (Vector2d StartPosition, Vector2d StartRotation)
		{
			if (!FirstInitialize) {
				FirstInitialize = true;
				this.gameObject = base.gameObject;
				this.transform = base.transform;
			}

			PositionChanged = true;
			RotationChanged = true;
			VelocityChanged = true;
			Position = StartPosition;
			Rotation = StartRotation;

			if (Shape != ColliderType.None) {
				GeneratePoints ();
				GenerateBounds ();
				BuildPoints ();
				BuildBounds ();
			}
			for (int i = 0; i < 4; i++) {
				LocatedPartitionExists [i] = false;
			}
			PhysicsManager.Assimilate (this);

			GetInterpolateValues ();
			transform.position = curPos;
			lastPos = curPos;
		}

		public void GetInterpolateValues ()
		{
			if (PositionChanged) {
				lastPos = curPos;
				curPos = Position.ToVector3 (transform.position.y);
				PositionChangedBuffer = true;
			}

			if (RotationChanged) {
				lastRot = curRot;
				curRot = Rotation.ToVector3 (0f);
				RotationChangedBuffer = true;
			}
			
		}

		public void EarlySimulate ()
		{
			if (VelocityChanged) {
				VelocityMagnitude = Velocity.Magnitude ();
				VelocityChanged = false;
			}

			if (VelocityMagnitude != 0) {
				Position.x += Velocity.x;
				Position.y += Velocity.y;
				PositionChanged = true;
			}


		}

		public void Simulate ()
		{


			if (PositionChanged || RotationChanged) {

				if (PositionChanged) {
					FuturePosition.x = Position.x + Velocity.x * PhysicsManager.ColSpreadMul;
					FuturePosition.y = Position.y + Velocity.y * PhysicsManager.ColSpreadMul;
					this.transform.position = Position.ToVector3 (this.transform.position.y);
				} else {
				}

				if (RotationChanged) {
					this.transform.rotation = Quaternion.LookRotation (new Vector3 (FixedMath.ToFloat (Rotation.x), 0, FixedMath.ToFloat (Rotation.y)));
				} else {
				}

				BuildPoints ();
				BuildBounds ();

				PositionChanged = false;
				RotationChanged = false;
			} else {

			}

			
		}

		public void Visualize ()
		{
			if (PositionChangedBuffer) {
				this.transform.position = Vector3.Lerp (lastPos, curPos, PhysicsManager.LerpTime);
				PositionChangedBuffer = false;
			}

			if (RotationChangedBuffer) {
				this.transform.rotation = Quaternion.LookRotation (Vector3.Lerp (lastRot, curRot, PhysicsManager.LerpTime));
				RotationChangedBuffer = false;
			}

		}

		public void SetPosition (long x, long y)
		{
			Position.x = x;
			Position.y = y;
			PositionChanged = true;
		}

		public void SetRotation (long x, long y)
		{
			Rotation.x = x;
			Rotation.y = y;
			RotationChanged = true;
		}

		public void SetVelocity (long x, long y)
		{
			Velocity.x = x;
			Velocity.y = y;
			VelocityChanged = true;
		}

		#region Collider

		public uint RaycastVersion;
		
		public Vector2d[]
			RotatedPoints;
		public Vector2d[]
			RealPoints;
		public Vector2d[]
			EdgeNorms;
		public Vector2d FuturePosition;

		public long XMin;
		public long XMax;
		public long YMin;
		public long YMax;
		public long FutureXMin;
		public long FutureXMax;
		public long FutureYMin;
		public long FutureYMax;
		public long PastGridXMin;
		public long PastGridXMax;
		public long PastGridYMin;
		public long PastGridYMax;

		public void GeneratePoints ()
		{
			if (Shape != ColliderType.Polygon)
				return;
			RotatedPoints = new Vector2d[Vertices.Length];
			for (int i = 0; i < Vertices.Length; i++) {
				RotatedPoints [i] = Vertices [i];
				RotatedPoints [i].Rotate (Rotation.x, Rotation.y);
			}
			RealPoints = new Vector2d[Vertices.Length];
			EdgeNorms = new Vector2d[Vertices.Length];
		}

		public void BuildPoints ()
		{
			if (Shape != ColliderType.Polygon)
				return;
			int VertLength = Vertices.Length;

			if (RotationChanged) {
				for (int i = 0; i < VertLength; i++) {
					RotatedPoints [i] = Vertices [i];
					RotatedPoints [i].Rotate (Rotation.x, Rotation.y);

					EdgeNorms [i] = RotatedPoints [i];
					if (i == 0) {
						EdgeNorms [i].Subtract (ref RotatedPoints [VertLength - 1]);
					} else {
						EdgeNorms [i].Subtract (ref RotatedPoints [i - 1]);
					}
					EdgeNorms [i].Normalize ();
					EdgeNorms [i].RotateRight ();
				}
			}
			for (int i = 0; i < Vertices.Length; i++) {
				RealPoints [i].x = RotatedPoints [i].x + Position.x;
				RealPoints [i].y = RotatedPoints [i].y + Position.y;
			}
		}

		public void GenerateBounds ()
		{
			if (Shape == ColliderType.Circle) {
				Radius = Radius;
			} else if (Shape == ColliderType.AABox) {
				if (HalfHeight == HalfWidth)
					Radius = FixedMath.Sqrt ((HalfHeight * HalfHeight * 2) >> FixedMath.SHIFT_AMOUNT);
				else
					Radius = FixedMath.Sqrt ((HalfHeight * HalfHeight + HalfWidth * HalfWidth) >> FixedMath.SHIFT_AMOUNT);
			} else if (Shape == ColliderType.Polygon) {
				long BiggestSqrRadius = Vertices [0].SqrMagnitude ();
				for (int i = 1; i < Vertices.Length; i++) {
					long sqrRadius = Vertices [i].SqrMagnitude ();
					if (sqrRadius > BiggestSqrRadius) {
						BiggestSqrRadius = sqrRadius;
					}
				}
				Radius = FixedMath.Sqrt (BiggestSqrRadius);
			}
		}
		
		public void BuildBounds ()
		{
			if (Shape == ColliderType.Circle) {
				XMin = -Radius + Position.x;
				XMax = Radius + Position.x;
				YMin = -Radius + Position.y;
				YMax = Radius + Position.y;
			} else if (Shape == ColliderType.AABox) {
				XMin = -HalfWidth + Position.x;
				XMax = HalfWidth + Position.x;
				YMin = -HalfHeight + Position.y;
				YMax = HalfHeight + Position.y;
			} else if (Shape == ColliderType.Polygon) {
				XMin = Position.x;
				XMax = Position.x;
				YMin = Position.y;
				YMax = Position.y; 
				for (int i = 0; i < Vertices.Length; i++) {

					Vector2d vec = RealPoints [i];
					if (vec.x < XMin) {
						XMin = vec.x;
					} else if (vec.x > XMax) {
						XMax = vec.x;
					}

					if (vec.y < YMin) {
						YMin = vec.y;
					} else if (vec.y > YMax) {
						YMax = vec.y;
					}
				}

			}
			FutureXMin = XMin + Velocity.x * PhysicsManager.ColSpreadMul;
			FutureXMax = XMax + Velocity.x * PhysicsManager.ColSpreadMul;
			FutureYMin = YMin + Velocity.y * PhysicsManager.ColSpreadMul;
			FutureYMax = YMax + Velocity.y * PhysicsManager.ColSpreadMul;
		}
#if UNITY_EDITOR
		void OnDrawGizmos ()
		{
			if (!Application.isPlaying) return;
			Gizmos.color = Color.white;
			Vector3[] PolyLine;
			LSBody body = this;
			Vector3 TargetPosition = ((MonoBehaviour)body).transform.position;
			//TargetPosition.y += .55f;
			switch (body.Shape) {
			case ColliderType.Circle:
				LSEditorUtility.GizmoCircle (TargetPosition, FixedMath.ToFloat (body.Radius));
				break;
			case ColliderType.AABox:
				PolyLine = new Vector3[] {
					TargetPosition,
					TargetPosition,
					TargetPosition,
					TargetPosition
				};
				float halfWidth = FixedMath.ToFloat (body.HalfWidth);
				float halfHeight = FixedMath.ToFloat (body.HalfHeight);
				PolyLine [0].x += halfWidth;
				PolyLine [0].z += halfHeight;
				PolyLine [1].x += halfWidth;
				PolyLine [1].z -= halfHeight;
				PolyLine [2].x -= halfWidth;
				PolyLine [2].z -= halfHeight;
				PolyLine [3].x -= halfWidth;
				PolyLine [3].z += halfHeight;
				LSEditorUtility.GizmoPolyLine (PolyLine);
				break;
			case ColliderType.Polygon:
				int VertLength = body.Vertices.Length;
				PolyLine = new Vector3[VertLength];
				for (int i = 0; i < VertLength; i++)
				{
					PolyLine[i] = body.RealPoints[i].ToVector3 (TargetPosition.y);
				}
				LSEditorUtility.GizmoPolyLine (PolyLine);
				break;
			}
		}
#endif
		#endregion
		public override int GetHashCode ()
		{
			return ID;
		}
	}

	public enum ColliderType : byte
	{
		None,
		Circle,
		AABox,
		Polygon
	}
}