using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
    //TODO: Make this an attribute for Vector2d
    [Serializable]
    public struct VectorRotation
    {

        public VectorRotation(long cos, long sin)
        {
            this._cos = cos;
            this._sin = sin;

            #if UNITY_EDITOR
            _angle = Mathf.Atan2 (sin,cos) * Mathf.Rad2Deg;
            _timescaled = false;
            #endif
        }

        [SerializeField]
        private long _cos;
        [SerializeField]
        private long _sin;

        public long Cos { get { return _cos; } }

        public long Sin { get { return _sin; } }

        #if UNITY_EDITOR
        [SerializeField]
        private double _angle;
        [SerializeField]
        private bool _timescaled;
        #endif
    }
}