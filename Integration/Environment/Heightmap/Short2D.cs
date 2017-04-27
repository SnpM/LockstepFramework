using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
    [System.Serializable]
    public class Short2D : Lockstep.Array2D<short>
    {
        public Short2D () {

        }
        public Short2D (int width, int height) :base (width,height) {

        }
    }
}