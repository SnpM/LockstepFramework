using UnityEngine;
using System.Collections;

namespace Lockstep
{
	public static class FixedMath
	{
	#region Meta
		public const int SHIFT_AMOUNT = 16;
		public const long One = 1 << SHIFT_AMOUNT;
		public const long Half = One / 2;
		public const float OneF = (float)One;
		public const double OneD = (double)One;
		public const long Pi = (355 * One)/113;
	#endregion

	#region Constructors
		/// <summary>
		/// Create a fixed point number from an integer.
		/// </summary>
		/// <param name="integer">Integer.</param>
		public static long Create (long integer)
		{
			return integer << SHIFT_AMOUNT;
		}
		public static long Create (float singleFloat)
		{
			return (long)((double)singleFloat * One);
		}
		/// <summary>
		/// Create a fixed point number from a double.
		/// </summary>
		/// <param name="doubleFloat">Double float.</param>
		public static long Create (double doubleFloat)
		{
			return (long)(doubleFloat * One);
		}
		/// <summary>
		/// Create a fixed point number from a fraction.
		/// </summary>
		/// <param name="whole">Whole.</param>
		/// <param name="fraction">Fraction.</param>
		public static long Create (long Numerator, long Denominator)
		{
			return (Numerator << SHIFT_AMOUNT) / Denominator;
		}
		/// <summary>
		/// Tries to parse string into fixed point number.
		/// </summary>
		/// <returns><c>true</c>, if parse was tried, <c>false</c> otherwise.</returns>
		/// <param name="s">S.</param>
		/// <param name="result">Result.</param>
		public static bool TryParse (string s, out long result)
		{
			string[] NewValues = s.Split ('.');
			if (NewValues.Length <= 2) {
				long Whole;
				if (long.TryParse (NewValues [0], out Whole)) {
					if (NewValues.Length == 1) {
						result = Whole << SHIFT_AMOUNT;
						return true;
					} else {
						long Numerator;
						if (long.TryParse (NewValues [1], out Numerator)) {
							int fractionDigits = NewValues [1].Length;
							long Denominator = 1;
							for (int i = 0; i < fractionDigits; i++) {
								Denominator *= 10;
							}
							result = (Whole << SHIFT_AMOUNT) + FixedMath.Create (Numerator, Denominator);
							return true;
						}
					}
				}
			}
			result = 0;
			return false;
		}
	#endregion

	#region Math
		/// <summary>
		/// Addition.
		/// </summary>
		/// <param name="f1">f1.</param>
		/// <param name="f2">f2.</param>
		public static long Add (long f1, long f2)
		{
			return f1 + f2;
		}
		/// <summary>
		/// Subtraction.
		/// </summary>
		/// <param name="f1">f1.</param>
		/// <param name="f2">f2.</param>
		public static long Sub (long f1, long f2)
		{
			return f1 - f2;
		}
		/// <summary>
		/// Multiplication.
		/// </summary>
		/// <param name="f1">f1.</param>
		/// <param name="f2">f2.</param>
		public static long Mul (long f1, long f2)
		{
			return (f1 * f2) >> SHIFT_AMOUNT;
		}
		/// <summary>
		/// Division.
		/// </summary>
		/// <param name="f1">f1.</param>
		/// <param name="f2">f2.</param>
		public static long Div (long f1, long f2)
		{
			return (f1 << SHIFT_AMOUNT) / f2;
		}
		/// <summary>
		/// Modulo.
		/// </summary>
		/// <param name="f1">f1.</param>
		/// <param name="f2">f2.</param>
		public static long Mod (long f1, long f2)
		{
			return f1 % f2;
		}

		static long n, n1;
		/// <summary>
		/// Square root.
		/// </summary>
		/// <param name="f1">f1.</param>
		public static long Sqrt (long f1)
		{
			if (0 == f1) {
				return 0;
			}
			 n = (f1 / 2) + 1;
			 n1 = (n + (f1 / n)) / 2;  
			while (n1 < n) {  
				n = n1;  
				n1 = (n + (f1 / n)) / 2;  
			} 
			return n << SHIFT_AMOUNT / 2;  
		}
	#endregion

	#region Helpful
		/// <summary>
		/// Truncate the specified fixed-point number.
		/// </summary>
		/// <param name="f1">F1.</param>
		public static long Truncate (long f1)
		{
			return ((f1 + Half) >> SHIFT_AMOUNT) << SHIFT_AMOUNT;
		}
		/// <summary>
		/// Round the specified fixed point number.
		/// </summary>
		/// <param name="f1">F1.</param>
		public static long Round (long f1)
		{
			return ((f1 + FixedMath.Half - 1) >> SHIFT_AMOUNT) << SHIFT_AMOUNT;
		}
		public static long RoundToInteger (long f1)
		{
			return ((f1 + FixedMath.Half - 1) >> SHIFT_AMOUNT);
		}
		/// <summary>
		/// Ceil the specified fixed point number.
		/// </summary>
		/// <param name="f1">F1.</param>
		public static long Ceil (long f1)
		{
			return ((f1 + One - 1) >> SHIFT_AMOUNT) << SHIFT_AMOUNT;
		}

		public static long Lerp (long from, long to, long t)
		{
			if (t >= One) return to;
			else if (t <= 0) return from;
			return (to * t + from * (One - t)) >> SHIFT_AMOUNT;
		}
	#endregion

	#region Convert
		public static long ToInt (long f1)
		{
			return (f1 >> SHIFT_AMOUNT);
		}
		/// <summary>
		/// Convert to double.
		/// </summary>
		/// <returns>The double.</returns>
		/// <param name="f1">f1.</param>
		public static double ToDouble (long f1)
		{
			return (f1 / OneD);
		}
		/// <summary>
		/// Convert to float.
		/// </summary>
		/// <returns>The float.</returns>
		/// <param name="f1">f1.</param>
		public static float ToFloat (long f1)
		{
			return (float)(f1 / OneD);
		}
		/// <summary>
		/// Converts to string.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="f1">f1.</param>
		public static string ToString (long f1)
		{
			return (System.Math.Round((f1) / OneD, 4, System.MidpointRounding.AwayFromZero)).ToString ();
		}
	#endregion
	}
}