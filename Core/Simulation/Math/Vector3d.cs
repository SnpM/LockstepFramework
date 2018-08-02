using UnityEngine;

namespace Lockstep
{
	//THIS IS HAPPENING!!!!
	[System.Serializable]
	public struct Vector3d : ICommandData
	{
		[FixedNumber]
		/// <summary>
		/// Horizontal
		/// </summary>
		public long x;
		[FixedNumber]
		/// <summary>
		/// Forward/Backward
		/// </summary>
		public long y;
		[FixedNumber]
		/// <summary>
		/// Up/Down
		/// </summary>
		public long z; //Height
		public Vector3d(Vector3 vec3)
		{
			this.x = FixedMath.Create(vec3.x);
			this.y = FixedMath.Create(vec3.z);
			this.z = FixedMath.Create(vec3.y);
		}
		public Vector3d(long X, long Y, long Z)
		{
			x = X;
			y = Y;
			z = Z;
		}

		public void Normalize()
		{
			long magnitude = FixedMath.Sqrt(x.Mul(x) + y.Mul(y) + z.Mul(z));
			x = x.Div(magnitude);
			y = y.Div(magnitude);
			z = z.Div(magnitude);
		}

		public Vector2d ToVector2d()
		{
			return new Vector2d(x, y);
		}
		public Vector3 ToVector3()
		{
			return new Vector3(x.ToPreciseFloat(), z.ToPreciseFloat(), y.ToPreciseFloat());
		}
		public void SetVector2d(Vector2d vec2d)
		{
			x = vec2d.x;
			y = vec2d.y;
		}
		public void Add(ref Vector2d other)
		{
			x += other.x;
			y += other.y;
		}
		public void Add(ref Vector3d other)
		{
			x += other.x;
			y += other.y;
			z += other.z;
		}
		public void Add(Vector3d other)
		{
			x += other.x;
			y += other.y;
			z += other.z;
		}
		public void Mul(long f1)
		{
			x *= f1;
			x >>= FixedMath.SHIFT_AMOUNT;
			y *= f1;
			y >>= FixedMath.SHIFT_AMOUNT;
			z *= f1;
			z >>= FixedMath.SHIFT_AMOUNT;
		}
		public void Mul(int i)
		{
			x *= i;
			y *= i;
			z *= i;
		}
		public long Distance(Vector3d other)
		{
			long tX = other.x - x;
			tX *= tX;
			tX >>= FixedMath.SHIFT_AMOUNT;
			long tY = other.y - y;
			tY *= tY;
			tY >>= FixedMath.SHIFT_AMOUNT;
			long tZ = other.z - z;
			tZ *= tZ;
			tZ >>= FixedMath.SHIFT_AMOUNT;
			return FixedMath.Sqrt(tX + tY + tZ);
		}
		public long LongStateHash { get { return (x * 31 + y * 7 + z * 11); } }
		public int StateHash { get { return (int)(LongStateHash % int.MaxValue); } }

		public override string ToString()
		{
			return string.Format("({0}, {1}, {2})", x.ToFormattedDouble(), y.ToFormattedDouble(), z.ToFloat());
		}

		public void Write(Writer writer)
		{
			writer.Write(x);
			writer.Write(y);
			writer.Write(z);
		}

		public void Read(Reader reader)
		{
			x = reader.ReadLong();
			y = reader.ReadLong();
			z = reader.ReadLong();
		}

		#region Operators

		public static Vector3d operator +(Vector3d v1, Vector3d v2)
		{
			return new Vector3d(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
		}

		public static Vector3d operator -(Vector3d v1, Vector3d v2)
		{
			return new Vector3d(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
		}

		public static Vector3d operator *(Vector3d v1, long mag)
		{
			return new Vector3d((v1.x * mag) >> FixedMath.SHIFT_AMOUNT, (v1.y * mag) >> FixedMath.SHIFT_AMOUNT, (v1.z * mag) >> FixedMath.SHIFT_AMOUNT);
		}

		public static Vector3d operator *(Vector3d v1, int mag)
		{
			return new Vector3d((v1.x * mag), (v1.y * mag), (v1.z * mag));
		}

		public static Vector3d operator /(Vector3d v1, long div)
		{
			return new Vector3d(((v1.x << FixedMath.SHIFT_AMOUNT) / div), (v1.y << FixedMath.SHIFT_AMOUNT) / div, (v1.z << FixedMath.SHIFT_AMOUNT) / div);
		}

		public static Vector3d operator /(Vector3d v1, int div)
		{
			return new Vector3d((v1.x / div), v1.y / div, v1.z / div);
		}

		public static Vector3d operator >>(Vector3d v1, int shift)
		{
			return new Vector3d(v1.x >> shift, v1.y >> shift, v1.z >> shift);
		}

		public static bool operator ==(Vector3d v1, Vector3d v2)
		{
			return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
		}

		public static bool operator !=(Vector3d v1, Vector3d v2)
		{
			return v1.x != v2.x || v1.y != v2.y || v1.z != v2.z;
		}

		#endregion
	}
}