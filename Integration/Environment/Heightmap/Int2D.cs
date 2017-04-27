using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
    [System.Serializable]
    public class Int2D : Lockstep.Array2D<int>
    {
        public Int2D () {

        }
        public Int2D (int width, int height) :base (width,height) {

        }
    }
}