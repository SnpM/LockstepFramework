//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================
using UnityEngine;
using System.Collections;
using System;

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
        public const long Pi = (355 * One) / 113;
        public const long MaxFixedNumber = long.MaxValue >> SHIFT_AMOUNT;
        public const long TenDegrees = FixedMath.One * 1736 / 10000;
        public const long Epsilon = 1 << (SHIFT_AMOUNT - 10);

        #endregion

        #region Constructors

        /// <summary>
        /// Create a fixed point number from an integer.
        /// </summary>
        /// <param name="integer">Integer.</param>
        public static long Create(long integer)
        {
            return integer << SHIFT_AMOUNT;
        }

        public static long Create(float singleFloat)
        {
            return (long)((double)singleFloat * One);
        }

        /// <summary>
        /// Create a fixed point number from a double.
        /// </summary>
        /// <param name="doubleFloat">Double float.</param>
        public static long Create(double doubleFloat)
        {
            return (long)(doubleFloat * One);
        }

        /// <summary>
        /// Create a fixed point number from a fraction.
        /// </summary>
        /// <param name="whole">Whole.</param>
        /// <param name="fraction">Fraction.</param>
        public static long Create(long Numerator, long Denominator)
        {
            return (Numerator << SHIFT_AMOUNT) / Denominator;
        }

        /// <summary>
        /// Tries to parse string into fixed point number.
        /// </summary>
        /// <returns><c>true</c>, if parse was tried, <c>false</c> otherwise.</returns>
        /// <param name="s">S.</param>
        /// <param name="result">Result.</param>
        public static bool TryParse(string s, out long result)
        {
            string[] NewValues = s.Split('.');
            if (NewValues.Length <= 2)
            {
                long Whole;
                if (long.TryParse(NewValues [0], out Whole))
                {
                    if (NewValues.Length == 1)
                    {
                        result = Whole << SHIFT_AMOUNT;
                        return true;
                    } else
                    {
                        long Numerator;
                        if (long.TryParse(NewValues [1], out Numerator))
                        {
                            int fractionDigits = NewValues [1].Length;
                            long Denominator = 1;
                            for (int i = 0; i < fractionDigits; i++)
                            {
                                Denominator *= 10;
                            }
                            result = (Whole << SHIFT_AMOUNT) + FixedMath.Create(Numerator, Denominator);
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
        public static long Add(this long f1, long f2)
        {
            return f1 + f2;
        }

        /// <summary>
        /// Subtraction.
        /// </summary>
        /// <param name="f1">f1.</param>
        /// <param name="f2">f2.</param>
        public static long Sub(this long f1, long f2)
        {
            return f1 - f2;
        }

        /// <summary>
        /// Multiplication.
        /// </summary>
        /// <param name="f1">f1.</param>
        /// <param name="f2">f2.</param>
        public static long Mul(this long f1, long f2)
        {
            return (f1 * f2) >> SHIFT_AMOUNT;
        }

        public static long Mul(this long f1, int intr)
        {
            return (f1 * intr);
        }

        /// <summary>
        /// Division.
        /// </summary>
        /// <param name="f1">f1.</param>
        /// <param name="f2">f2.</param>
        public static long Div(this long f1, long f2)
        {
            return (f1 << SHIFT_AMOUNT) / f2;
        }

        /// <summary>
        /// Modulo.
        /// </summary>
        /// <param name="f1">f1.</param>
        /// <param name="f2">f2.</param>
        public static long Remainder(this long f1, long f2)
        {
            return f1 % f2;
        }

        public static long Mod(this long f1, long f2)
        {
            long f = f1 % f2;
            return f < 0 ? f + f2 : f;
        }

        /// <summary>
        /// Square root.
        /// </summary>
        /// <param name="f1">f1.</param>

        static long n, n1;

        public static long Sqrt(long f1)
        {
            if (f1 == 0)
                return 0;
            n = (f1 >> 1) + 1;
            n1 = (n + (f1 / n)) >> 1;
            while (n1 < n)
            {
                n = n1;
                n1 = (n + (f1 / n)) >> 1;
            }
            return n << (SHIFT_AMOUNT / 2);
        }

        public static long SinToCos(long sin)
        {
            return Sqrt(FixedMath.One - (sin.Mul( sin)).Normalized());
        }

        public static long Normalized(this long f1)
        {
            return f1 >> FixedMath.SHIFT_AMOUNT;
        }

        public static long Abs(this long f1)
        {
            return f1 < 0 ? -f1 : f1;
        }

        public static bool AbsMoreThan(this long f1, long f2)
        {
            return f1.Abs() > f2;
        }

        #endregion

        #region Helpful

        /// <summary>
        /// Truncate the specified fixed-point number.
        /// </summary>
        /// <param name="f1">F1.</param>
        public static long Truncate(long f1)
        {
            return ((f1) >> SHIFT_AMOUNT) << SHIFT_AMOUNT;
        }

        /// <summary>
        /// Round the specified fixed point number.
        /// </summary>
        /// <param name="f1">F1.</param>
        public static long Round(long f1)
        {
            return ((f1 + FixedMath.Half - 1) >> SHIFT_AMOUNT) << SHIFT_AMOUNT;
        }

        /// <summary>
        /// Ceil the specified fixed point number.
        /// </summary>
        /// <param name="f1">F1.</param>
        public static long Ceil(long f1)
        {
            return ((f1 + One - 1) >> SHIFT_AMOUNT) << SHIFT_AMOUNT;
        }

        public static long Lerp(long from, long to, long t)
        {
            if (t >= One)
                return to;
            else if (t <= 0)
                return from;
            return (to * t + from * (One - t)) >> SHIFT_AMOUNT;
        }

        public static long Min(this long f1, long f2)
        {
            return f1 <= f2 ? f1 : f2;
        }

        public static long Max(this long f1, long f2)
        {
            return f1 >= f2 ? f1 : f2;
        }

        public static double ToFormattedDouble(this long f1)
        {
            return Math.Round(FixedMath.ToDouble(f1), 2, MidpointRounding.AwayFromZero);
        }

        public static bool MoreThanEpsilon(this long f1)
        {
            return f1 > Epsilon || f1 < Epsilon;
        }

        #endregion

        #region Convert

        public static int ToInt(this long f1)
        {
            return (int)(f1 >> SHIFT_AMOUNT);
        }

        public static int RoundToInt(this long f1)
        {
            return (int)((f1 + Half - 1) >> SHIFT_AMOUNT);
        }

        public static int CeilToInt(this long f1)
        {
            return (int)((f1 + One - 1) >> SHIFT_AMOUNT);
        }

        /// <summary>
        /// Convert to double.
        /// </summary>
        /// <returns>The double.</returns>
        /// <param name="f1">f1.</param>
        public static double ToDouble(this long f1)
        {
            return (f1 / OneD);
        }

        /// <summary>
        /// Convert to float.
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="f1">f1.</param>
        public static float ToFloat(this long f1)
        {
            return (float)(f1 / OneD);
        }

        public static float ToPreciseFloat(this long f1)
        {
            return (float)ToDouble(f1);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="f1">f1.</param>

        public static string GetString(this long f1)
        {
            return (System.Math.Round((f1) / OneD, 4, System.MidpointRounding.AwayFromZero)).ToString();
        }

        #endregion

        public static class Trig
        {
            public static long Sin(long radPiAngle)
            {
                //Taylor series cuz easy
                //TODO: Profiling
                //Note: Max 4 multiplications before overflow

                radPiAngle %= FixedMath.One * 2;

                long result = radPiAngle;

                //2 shifts for 2 multiplications but there's a division so only 1 shift
                long x = (radPiAngle * radPiAngle * radPiAngle) >> FixedMath.SHIFT_AMOUNT * 1;
                const long Factorial3 = 3 * 2 * FixedMath.One;
                result -= x / Factorial3;

                x *= x * x;
                x >>= 2;
                const long Factorial5 = Factorial3 * 4 * 5;
                result += x / Factorial5;

                x *= x * x;
                x >>= 2;
                const long Factorial7 = Factorial5 * 6 * 7;
                result -=  x / Factorial7;

                x *= x * x;
                x >>= 2;
                const long Factorial9 = Factorial7 * 8 * 9;
                result +=  x / Factorial9;

                return result;
            }
        }
    }
}