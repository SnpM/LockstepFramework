using UnityEngine;
using System.Collections; using FastCollections;
using System;


namespace Lockstep
{
	[Serializable]
	public struct Vector2d : ICommandData
	{
		[FixedNumber]
		public long x;
		[FixedNumber]
		public long y;


		#region Constructors

		public Vector2d(long xFixed, long yFixed)
		{
			this.x = xFixed;
			this.y = yFixed;
		}

		public Vector2d(int xInt, int yInt)
		{
			this.x = xInt << FixedMath.SHIFT_AMOUNT;
			this.y = yInt << FixedMath.SHIFT_AMOUNT;
		}

		public Vector2d(Vector2 vec2)
		{
			this.x = FixedMath.Create(vec2.x);
			this.y = FixedMath.Create(vec2.y);
		}

		public Vector2d(float xFloat, float yFloat)
		{
			this.x = FixedMath.Create(xFloat);
			this.y = FixedMath.Create(yFloat);
		}

		public Vector2d(double xDoub, double yDoub)
		{
			this.x = FixedMath.Create(xDoub);
			this.y = FixedMath.Create(yDoub);
		}

		public Vector2d(Vector3 vec)
		{
			this.x = FixedMath.Create(vec.x);
			this.y = FixedMath.Create(vec.z);
		}

		#endregion


		#region Local Math

		public void Subtract(ref Vector2d other)
		{
			this.x -= other.x;
			this.y -= other.y;
		}

		public void Add(ref Vector2d other)
		{
			this.x += other.x;
			this.y += other.y;
		}


		/// <summary>
		/// This vector's square magnitude.
		/// </summary>
		/// <returns>The magnitude.</returns>
		public long SqrMagnitude()
		{
			return (this.x * this.x + this.y * this.y) >> FixedMath.SHIFT_AMOUNT;
		}

		/// <summary>
		/// This vector's magnitude.
		/// </summary>
		public long Magnitude()
		{
			temp1 = (this.x * this.x + this.y * this.y);
			if (temp1 == 0)
				return 0;
			temp1 >>= FixedMath.SHIFT_AMOUNT;
			return FixedMath.Sqrt(temp1);
		}

		public long FastMagnitude()
		{
			return this.x * this.x + this.y * this.y;
		}

		/// <summary>
		/// Normalize this vector.
		/// </summary>
		public void Normalize()
		{
			tempMag = this.Magnitude();
			if (tempMag == 0)
			{
				return;
			}
			else if (tempMag == FixedMath.One)
			{
				return;
			}
			this.x = (this.x << FixedMath.SHIFT_AMOUNT) / tempMag;
			this.y = (this.y << FixedMath.SHIFT_AMOUNT) / tempMag;
		}

		public void Normalize(out long mag)
		{
			mag = this.Magnitude();
			if (mag == 0)
			{
				return;
			}
			else if (mag == FixedMath.One)
			{
				return;
			}
			this.x = (this.x << FixedMath.SHIFT_AMOUNT) / mag;
			this.y = (this.y << FixedMath.SHIFT_AMOUNT) / mag;
		}

		public void FastNormalize()
		{
			//Blazing fast normalization when accuracy isn't needed
			tempMag = x > y ? x + y / 2 : x / 2 + y;
			const long errorFactor = 1000 * FixedMath.One / 1118;

			this.x = ((this.x << FixedMath.SHIFT_AMOUNT) / tempMag) * errorFactor >> FixedMath.SHIFT_AMOUNT;
			this.y = ((this.y << FixedMath.SHIFT_AMOUNT) / tempMag) * errorFactor >> FixedMath.SHIFT_AMOUNT;
		}

		public void Lerp(Vector2d target, long amount)
		{
			Lerp(target.x, target.y, amount);
		}

		/// <summary>
		/// Lerp this vector to target by amount.
		/// </summary>
		/// <param name="target">target.</param>
		/// <param name="amount">amount.</param>
		public void Lerp(long targetx, long targety, long amount)
		{
			if (amount >= FixedMath.One)
			{
				this.x = targetx;
				this.y = targety;
				return;
			}
			else if (amount <= 0)
			{
				return;
			}
			this.x = (targetx * amount + this.x * (FixedMath.One - amount)) >> FixedMath.SHIFT_AMOUNT;
			this.y = (targety * amount + this.y * (FixedMath.One - amount)) >> FixedMath.SHIFT_AMOUNT;
		}

		public Vector2d Lerped(Vector2d target, long amount)
		{
			Vector2d vec = this;
			vec.Lerp(target.x, target.y, amount);
			return vec;
		}

		public void Rotate(long cos, long sin)
		{
			temp1 = (this.x * cos + this.y * sin) >> FixedMath.SHIFT_AMOUNT;
			this.y = (this.x * -sin + this.y * cos) >> FixedMath.SHIFT_AMOUNT;
			this.x = temp1;
		}

		public Vector2d Rotated(long cos, long sin)
		{
			Vector2d vec = this;
			vec.Rotate(cos, sin);
			return vec;
		}

		public Vector2d Rotated(Vector2d rotation)
		{
			return Rotated(rotation.x, rotation.y);
		}

		public void RotateInverse(long cos, long sin)
		{
			Rotate(cos, -sin);
		}

		public void RotateRight()
		{
			temp1 = this.x;
			this.x = this.y;
			this.y = -temp1;
		}

		static Vector2d retVec = Vector2d.zero;

		public Vector2d rotatedRight
		{
			get
			{
				retVec.x = y;
				retVec.y = -x;
				return retVec;
			}
		}

		public Vector2d rotatedLeft
		{
			get
			{
				retVec.x = -y;
				retVec.y = x;
				return retVec;
			}
		}

		public void Reflect(long axisX, long axisY)
		{
			temp3 = this.Dot(axisX, axisY);
			temp1 = (axisX * temp3) >> FixedMath.SHIFT_AMOUNT;
			temp2 = (axisY * temp3) >> FixedMath.SHIFT_AMOUNT;
			this.x = temp1 + temp1 - this.x;
			this.y = temp2 + temp2 - this.y;
		}

		public void Reflect(long axisX, long axisY, long projection)
		{
			temp1 = (axisX * projection) >> FixedMath.SHIFT_AMOUNT;
			temp2 = (axisY * projection) >> FixedMath.SHIFT_AMOUNT;
			this.x = temp1 + temp1 - this.x;
			this.y = temp2 + temp2 - this.y;
		}

		public Vector2d Reflected(long axisX, long axisY)
		{
			Vector2d vec = this;
			vec.Reflect(axisX, axisY);
			return vec;
		}

		public long Dot(long otherX, long otherY)
		{
			return (this.x * otherX + this.y * otherY) >> FixedMath.SHIFT_AMOUNT;
		}

		public long Dot(Vector2d other)
		{
			return this.Dot(other.x, other.y);
		}


		public long Cross(long otherX, long otherY)
		{
			return (this.x * otherY - this.y * otherX) >> FixedMath.SHIFT_AMOUNT;
		}

		public long Cross(Vector2d vec)
		{
			return Cross(vec.x, vec.y);
		}

		static long temp1;
		static long temp2;
		static long temp3;
		static long tempMag;

		public long Distance(long otherX, long otherY)
		{
			temp1 = this.x - otherX;
			temp1 *= temp1;
			temp2 = this.y - otherY;
			temp2 *= temp2;
			return (FixedMath.Sqrt((temp1 + temp2) >> FixedMath.SHIFT_AMOUNT));
		}

		public long Distance(Vector2d other)
		{
			return Distance(other.x, other.y);
		}

		public long SqrDistance(long otherX, long otherY)
		{

			temp1 = this.x - otherX;
			temp1 *= temp1;
			temp2 = this.y - otherY;
			temp2 *= temp2;
			return ((temp1 + temp2) >> FixedMath.SHIFT_AMOUNT);
		}

		/// <summary>
		/// Returns a value that is greater if the distance is greater.
		/// </summary>
		/// <returns>The FastDistance.</returns>
		public long FastDistance(long otherX, long otherY)
		{
			temp1 = this.x - otherX;
			temp1 *= temp1;
			temp2 = this.y - otherY;
			temp2 *= temp2;
			return (temp1 + temp2);
		}
		public long FastDistance(Vector2d other)
		{
			temp1 = this.x - other.x;
			temp1 *= temp1;
			temp2 = this.y - other.y;
			temp2 *= temp2;
			return (temp1 + temp2);
		}
		public bool NotZero()
		{
			return x.MoreThanEpsilon() || y.MoreThanEpsilon();
		}

		#endregion

		#region Static Math

		public static readonly Vector2d defaultRotation = new Vector2d(1,0);
		public static readonly Vector2d up = new Vector2d(0, 1);
		public static readonly Vector2d right = new Vector2d(1, 0);
		public static readonly Vector2d down = new Vector2d(0, -1);
		public static readonly Vector2d left = new Vector2d(-1, 0);
		public static readonly Vector2d one = new Vector2d(1, 1);
		public static readonly Vector2d negative = new Vector2d(-1, -1);
		public static readonly Vector2d zero = new Vector2d(0, 0);

		public static readonly Vector2d radian0 = new Vector2d(1, 0);
		public static readonly Vector2d radian1 = new Vector2d(0, 1);
		public static readonly Vector2d radian2 = new Vector2d(-1, 0);
		public static readonly Vector2d radian3 = new Vector2d(0, -1);

		public static long Dot(long v1x, long v1y, long v2x, long v2y)
		{
			return (v1x * v2x + v1y * v2y) >> FixedMath.SHIFT_AMOUNT;
		}

		public static long Cross(long v1x, long v1y, long v2x, long v2y)
		{
			return (v1x * v2y - v1y * v2x) >> FixedMath.SHIFT_AMOUNT;
		}

		/// <summary>
		/// Note: Not deterministic. Use only for serialization and sending in Commands.
		/// </summary>
		/// <returns>The from angle.</returns>
		/// <param name="angleRadians">Angle.</param>
		public static Vector2d CreateRotation(double angleRadians)
		{
			return new Vector2d(Math.Cos(angleRadians), Math.Sin(angleRadians));
		}

		public static Vector2d CreateRotation(float angleRadians)
		{
			return CreateRotation((double)angleRadians);
		}

		/// <summary>
		/// Deterministic!
		/// </summary>
		/// <returns>The rotation.</returns>
		/// <param name="angle">Angle.</param>
		public static Vector2d CreateRotation(long angle)
		{
			return new Vector2d(FixedMath.Trig.Cos(angle), FixedMath.Trig.Sin(angle));
		}

		public Vector2d ToDirection()
		{
			return new Vector2d(y, x);
		}

		public Vector2d ToRotation()
		{
			return new Vector2d(y, x);
		}

		#endregion

		#region Convert

		public override string ToString()
		{
			return (
			        "(" +
			        Math.Round(FixedMath.ToDouble(this.x), 2, MidpointRounding.AwayFromZero).ToString() +
			        ", " +
			        Math.Round(FixedMath.ToDouble(this.y), 2, MidpointRounding.AwayFromZero) +
			        ")"
			);
		}

		public Vector2 ToVector2()
		{
			return new Vector2(
				(float)FixedMath.ToDouble(this.x),
				(float)FixedMath.ToDouble(this.y)
			);
		}

		public Vector3d ToVector3d(long z = 0)
		{
			return new Vector3d(x, y, z);
		}

		public Vector3 ToVector3(float z = 0f)
		{
			return new Vector3((float)FixedMath.ToDouble(this.x), z, (float)FixedMath.ToDouble(this.y));
		}



		#endregion

		public override bool Equals(object obj)
		{
			if (obj is Vector2d)
			{
				return (Vector2d)obj == this;
			}
			return false;
		}

		#region Operators

		public static Vector2d operator +(Vector2d v1, Vector2d v2)
		{
			return new Vector2d(v1.x + v2.x, v1.y + v2.y);
		}

		public static Vector2d operator -(Vector2d v1, Vector2d v2)
		{
			return new Vector2d(v1.x - v2.x, v1.y - v2.y);
		}

		public static Vector2d operator *(Vector2d v1, long mag)
		{
			return new Vector2d((v1.x * mag) >> FixedMath.SHIFT_AMOUNT, (v1.y * mag) >> FixedMath.SHIFT_AMOUNT);
		}

		public static Vector2d operator *(Vector2d v1, int mag)
		{
			return new Vector2d((v1.x * mag), (v1.y * mag));
		}

		public static Vector2d operator /(Vector2d v1, long div)
		{
			return new Vector2d(((v1.x << FixedMath.SHIFT_AMOUNT) / div), (v1.y << FixedMath.SHIFT_AMOUNT) / div);
		}

		public static Vector2d operator /(Vector2d v1, int div)
		{
			return new Vector2d((v1.x / div), v1.y / div);
		}

		public static Vector2d operator >>(Vector2d v1, int shift)
		{
			return new Vector2d(v1.x >> shift, v1.y >> shift);
		}

		public static bool operator ==(Vector2d v1, Vector2d v2)
		{
			return v1.x == v2.x && v1.y == v2.y;
		}

		public static bool operator !=(Vector2d v1, Vector2d v2)
		{
			return v1.x != v2.x || v1.y != v2.y;
		}

		#endregion

		public long GetLongHashCode()
		{
			return x * 31 + y * 7;
		}

		public int GetStateHash()
		{
			return (int)(GetLongHashCode() % int.MaxValue);
		}

		public override int GetHashCode()
		{
			return this.GetStateHash();
		}

		public void Write(Writer writer)
		{
			writer.Write(this.x);
			writer.Write(this.y);
		}

		public void Read(Reader reader)
		{
			this.x = reader.ReadLong();
			this.y = reader.ReadLong();
		}
	}


}
