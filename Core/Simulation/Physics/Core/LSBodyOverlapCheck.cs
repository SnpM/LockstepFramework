using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public partial class LSBody
    {
        private static Vector2d cacheAxis;
        private static Vector2d cacheAxisNormal;
        private static Vector2d cacheP1;
        private static Vector2d cacheP2;
        private static long axisMin;
        private static long axisMax;
        private static long cacheProjPerp;

        public static void PrepareAxisCheck(Vector2d p1, Vector2d p2)
        {
            cacheP1 = p1;
            cacheP2 = p2;
            cacheAxis = p2 - p1;
            cacheAxis.Normalize();
            cacheAxisNormal = cacheAxis.rotatedLeft; 

            axisMin = p1.Dot(cacheAxis.x,cacheAxis.y);
            axisMax = p2.Dot(cacheAxis.x,cacheAxis.y);
            cacheProjPerp = cacheP1.Dot(cacheAxisNormal.x,cacheAxisNormal.y);
        }

        public bool Overlaps (FastList<Vector2d> intersectionPoints) {
            if (this.Agent != null) return false;

            intersectionPoints.FastClear();
            //Checks if this object overlaps the line formed by p1 and p2
            switch (this.Shape) {
                case ColliderType.Circle:
                    {
                        bool overlaps = false;
                        //Check if the circle completely fits between the line
                        long projPos = this._position.Dot(cacheAxis.x,cacheAxis.y);
                        //Circle withing bounds?
                        if (projPos >= axisMin && projPos <= axisMax) {
                            long projPerp = this._position.Dot(cacheAxisNormal.x,cacheAxisNormal.y);
                            long perpDif = (cacheProjPerp - projPerp);
                            long perpDist = perpDif.Abs();
                            if (perpDist <= _radius) {
                                overlaps = true;
                            }
                            if (overlaps) {
                                long sin = (perpDif);
                                long cos = FixedMath.Sqrt(_radius.Mul(_radius) - sin.Mul (sin));
                                Vector2d perpVector = cacheAxisNormal * cacheProjPerp;
                                if (cos == 0) {
                                    intersectionPoints.Add ((cacheAxis * projPos) + perpVector);
                                }
                                else {
                                    
                                    intersectionPoints.Add (cacheAxis * (projPos - cos) + perpVector);
                                    intersectionPoints.Add (cacheAxis * (projPos + cos) + perpVector);
                                }
                            }
                        }
                        else {
                            //If not, check distances to points
                            long p1Dist = _position.FastDistance (cacheP1.x,cacheP2.y);
                            if (p1Dist <= this.FastRadius)
                            {
                                intersectionPoints.Add(cacheP1);
                                overlaps = true;
                            }
                            long p2Dist = _position.FastDistance (cacheP2.x,cacheP2.y);
                            if (p2Dist <= this.FastRadius)
                            {
                                intersectionPoints.Add(cacheP2);
                                overlaps = true;
                            }

                        }
                        return overlaps;
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