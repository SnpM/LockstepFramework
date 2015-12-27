using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public partial class LSBody
    {
        private static Vector2d cacheAxis;
        private static Vector2d cacheP1;
        private static Vector2d cacheP2;
        private static long axisMin;
        private static long axisMax;
        private static long cacheProjPerp;

        public static void PrepareOverlapCheck(Vector2d p1, Vector2d p2)
        {
            cacheP1 = p1;
            cacheP2 = p2;
            cacheAxis = p2 - p1;
            cacheAxis.Normalize();
            axisMin = p1.Dot(cacheAxis.x,cacheAxis.y);
            axisMax = p2.Dot(cacheAxis.x,cacheAxis.y);
            cacheProjPerp = cacheP1.Cross(cacheAxis.x,cacheAxis.y);
        }

        public bool Overlaps () {
            //Checks if this object overlaps the line formed by p1 and p2
            switch (this.Shape) {
                case ColliderType.Circle:
                    {
                        //Check if the circle completely fits between the line
                        long projPos = this._position.Dot(cacheAxis.x,cacheAxis.y);
                        //Circle withing bounds?
                        if (projPos >= axisMin && projPos <= axisMax) {
                            long projPerp = this._position.Cross(cacheAxis.x,cacheAxis.y);

                            if ((projPerp - cacheProjPerp).AbsMoreThan(_radius) == false) {
                                return true;
                            }
                        }
                        else {
                            //If not, check distances to points
                            long p1Dist = _position.FastDistance (cacheP1.x,cacheP2.y);
                            if (p1Dist <= this.FastRadius)
                                return true;
                            long p2Dist = _position.FastDistance (cacheP2.x,cacheP2.y);
                            if (p2Dist <= this.FastRadius)
                                return true;
                        }
                    }
                    break;
                case ColliderType.AABox:
                    {
                      
                    }
                    break;
            }
            return false;
        }
    }
}