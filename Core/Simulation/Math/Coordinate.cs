using UnityEngine;
using System.Collections;

namespace Lockstep
{
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    public struct Coordinate
    {
        public int x;
        public int y;

        public Coordinate(int X, int Y)
        {
            x = X;
            y = Y;
        }

        public override string ToString()
        {
            return "(" + x.ToString() + ", " + y.ToString() + ")";
        }

        #if UNITY_EDITOR
        public void OnSerializeGUI()
        {
            x = EditorGUILayout.IntField("X", x);
            y = EditorGUILayout.IntField("Y", y);
        }
        #endif
    }
}