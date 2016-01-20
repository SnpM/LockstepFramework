using UnityEngine;
using System.Collections;

namespace Lockstep
{
    //THIS IS HAPPENING!!!!
    public struct Vector3d
    {
        [FixedNumber]
        public long x;
        [FixedNumber]
        public long y;
        [FixedNumber]
        public long z; //Height


        public void Normalize () {
            long magnitude = FixedMath.Sqrt(x * x + y * y + z * z);
            x = x.Div(magnitude);
            y = y.Div(magnitude);
            z = z.Div(magnitude);
        }

        public Vector2d ToVector2d () {
            return new Vector2d(x,y);
        }
        public Vector3 ToVector3 () {
            return new Vector3(x.ToPreciseFloat(),z.ToPreciseFloat(),y.ToPreciseFloat());
        }
    }
}