using UnityEngine;
using System.Collections; using FastCollections;
using System;
using System.Collections.Generic;

namespace PanLineAlgorithm.Test
{
    public class FractionalLineAlgorithmTest : MonoBehaviour
    {
        const int size = 64;
        bool[,] Outlined = new bool[size, size];
        public Transform start;
        public Transform end;

        void OnDrawGizmos()
        {
            Array.Clear(Outlined, 0, Outlined.Length);
            int offset = Outlined.GetLength(0) / 2;

            double startX = (double)start.position.x;
            double startY = (double)start.position.z;
            double endX = (double)end.position.x;
            double endY = (double)end.position.z;

            //irony lolz
            HashSet<FractionalLineAlgorithm.Coordinate> redundancyChecker = new HashSet<FractionalLineAlgorithm.Coordinate>();

            foreach (FractionalLineAlgorithm.Coordinate coor in (FractionalLineAlgorithm.Trace (startX,startY,endX,endY)))
            {
                if (!redundancyChecker.Add(coor))
                {
                    Debug.LogErrorFormat("Redundancy detected for {0}", coor);
                }

                Outlined [coor.X + offset, coor.Y + offset] = true;
            }
            Vector3 size = new Vector3(1, .1f, 1);
            Vector3 fillSize = new Vector3(.9f, .1f, .9f);
            float posOffset = 0f;
            for (int i = 0; i < Outlined.GetLength(0); i++)
            {
                for (int j = 0; j < Outlined.GetLength(1); j++)
                {
                    Vector3 drawPos = new Vector3(i - offset + posOffset, 0, j - offset + posOffset);
                    if (Outlined [i, j])
                    {
                        Gizmos.color = Color.red;
                    } else
                    {
                        Gizmos.color = Color.green;
                    }
                    Gizmos.DrawCube(drawPos, fillSize);

                    Gizmos.color = Color.black;

                    Gizmos.DrawWireCube(drawPos, size);

                }
            }

            float lineHeight = 0f;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(new Vector3((float)startX, lineHeight, (float)startY), new Vector3((float)endX, lineHeight, (float)endY));
        }
    }
}