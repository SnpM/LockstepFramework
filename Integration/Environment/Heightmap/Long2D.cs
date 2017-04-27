using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
    [System.Serializable]
    public class Long2D : Lockstep.Array2D<long>
    {
        public Long2D () {

        }
        public Long2D (int width, int height) :base (width,height) {

        }
    }
}