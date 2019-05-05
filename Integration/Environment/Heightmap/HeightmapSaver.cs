using UnityEngine;

namespace Lockstep
{
	public class HeightmapSaver : EnvironmentSaver
	{
		public static HeightmapSaver Instance { get; private set; }

		[SerializeField]
		private Vector2d _size = new Vector2d(100, 100);

		public Vector2d Size { get { return _size; } }

		[SerializeField]
		private Vector2d _heightBounds = new Vector2d(-20, 50);

		public Vector2d HeightBounds { get { return _heightBounds; } }

		[SerializeField]
		private Vector2d _bottomLeft = new Vector2d(-50, -50);

		public Vector2d BottomLeft { get { return _bottomLeft; } }

		[SerializeField, FixedNumber]
		private long _bonusHeight;

		public long BonusHeight
		{
			get
			{
				return _bonusHeight;
			}
		}

		/// <summary>
		/// Interval distance between each consecutive scan
		/// </summary>
		[SerializeField, FixedNumber]
		private long _interval = FixedMath.Half;

		public long Interval { get { return _interval; } }

		const int CompressionShift = FixedMath.SHIFT_AMOUNT / 2;

		[SerializeField]
		private HeightMap[] _maps = new HeightMap[1];

		public HeightMap[] Maps { get { return _maps; } }

		public short[,] Scan(int scanLayers)
		{

			int widthPeriods = Size.x.Div(Interval).CeilToInt();

			int heightPeriods = Size.y.Div(Interval).CeilToInt();
			short[,] heightMap = new short[widthPeriods, heightPeriods];


			Vector3 startPos = _bottomLeft.ToVector3(HeightBounds.y.ToFloat());
			Vector3 scanPos = startPos;
			float dist = (HeightBounds.y - HeightBounds.x).ToFloat();

			float fRes = Interval.ToFloat();
			for (int x = 0; x < widthPeriods; x++)
			{
				scanPos.z = startPos.z;
				for (int y = 0; y < heightPeriods; y++)
				{
					RaycastHit hit;
					long height;
					if (Physics.Raycast(scanPos, Vector3.down, out hit, dist, scanLayers, QueryTriggerInteraction.UseGlobal))
					{
						height = FixedMath.Create(hit.point.y);
					}
					else
					{
						height = HeightBounds.x;
					}

					heightMap[x, y] = Compress(height);
					scanPos.z += fRes;
				}
				scanPos.x += fRes;
			}

			return heightMap;
		}

		protected override void OnEarlyApply()
		{
			Instance = this;
		}


		public long GetHeight(int mapIndex, Vector2d position)
		{
			if (mapIndex >= Maps.Length)
			{
				return this.HeightBounds.x;
			}
			HeightMap map = Maps[mapIndex];
			long normX = (position.x - this._bottomLeft.x).Div(Interval);
			long normY = (position.y - this._bottomLeft.y).Div(Interval);
			int gridX = FixedMath.ToInt(normX);
			int gridY = FixedMath.ToInt(normY);
			long fractionX = (normX - FixedMath.Create(gridX));
			long fractionY = (normY - FixedMath.Create(gridY));
			long baseHeight = map.GetHeight(gridX, gridY);

			int nextX = Mathf.Clamp(gridX + 1, 0, map.Map.Width);

			int nextY = Mathf.Clamp(gridY + 1, 0, map.Map.Height);

			//bilinear lerp
			long h1 = FixedMath.Lerp(baseHeight, map.GetHeight(nextX, gridY), fractionX);
			long h2 = FixedMath.Lerp(map.GetHeight(gridX, nextY), map.GetHeight(nextX, nextY), fractionX);
			return FixedMath.Lerp(h1, h2, fractionY) + this.BonusHeight;

		}

		[SerializeField]
		private bool _show;

		void OnDrawGizmos()
		{
			if (_show == false)
				return;
			float fRes = Interval.ToFloat();
			Vector3 size = Vector3.one * (fRes * .95f);
			size.y = .1f;
			Color color = Color.grey;
			for (int i = 0; i < Maps.Length; i++)
			{
				color *= 1.1f;
				HeightMap map = Maps[i];
				Vector3 startPos = this.BottomLeft.ToVector3(0);
				Vector3 drawPos = startPos;
				for (int x = 0; x < map.Map.Width; x++)
				{
					drawPos.z = startPos.z;
					for (int y = 0; y < map.Map.Height; y++)
					{
						drawPos.y = map.GetHeight(x, y).ToFloat();
						Gizmos.DrawCube(drawPos, size);
						drawPos.z += fRes;
					}
					drawPos.x += fRes;
				}
			}

		}

		/*
         [SerializeField]
         Terrain[] _visualizeTerrains;
         
         void SetTerrain(Terrain terrain, float[,] heights)
         {
         terrain.terrainData.SetHeights(0, 0, heights);
         terrain.transform.position = _bottomLeft.ToVector3(0);
         }
         */
		public static short Compress(long value)
		{
			long compressed = value >> CompressionShift;
			if (compressed > short.MaxValue)
				compressed = short.MaxValue;
			else if (compressed < short.MinValue)
				compressed = short.MinValue;
			return (short)compressed;
		}

		public static long Uncompress(short compressed)
		{
			return (long)(compressed << CompressionShift);
		}

		protected override void OnSave()
		{
			var hh = this;
			for (int i = 0; i < hh.Maps.Length; i++)
			{
				HeightMap hm = hh.Maps[i];

				short[,] scan = hh.Scan(hh.Maps[i].ScanLayers.value);
				hm.Map.LocalClone(scan);

			}
		}
	}
}