using UnityEngine;
using System.Collections; using FastCollections;
using PanLineAlgorithm;
using CCoordinate = PanLineAlgorithm.FractionalLineAlgorithm.Coordinate;
namespace Lockstep.Test
{
    public class RaycastTestHelper : MonoBehaviour
    {
        public bool testPartitions = true;
        public bool testBodies = true;
        public Transform start;
        public Transform end;
        Vector2d startPos;
        Vector2d endPos;
        void OnDrawGizmos () {
             startPos = new Vector2d(start.position);
             endPos = new Vector2d(end.position);
            if (testPartitions)TestPartitions ();
            if (testBodies)TestBodies ();
            Gizmos.DrawLine(startPos.ToVector3(0),endPos.ToVector3(0));
        }
        //FastList<LSBody_> lastBodies = new FastList<LSBody_>();
        void TestBodies () {
            /*if (Application.isPlaying == false) return;
            for (int i = 0; i < lastBodies.Count; i++) {
                lastBodies[i].GetComponentInChildren<Renderer>().material.color = Color.white;
            }
            lastBodies.FastClear();
            foreach (_LSBody body in Raycaster.RaycastAll (startPos, endPos)) {
                body.GetComponentInChildren<Renderer>().material.color = Color.red;
                lastBodies.Add(body);
            }*/
        }
        void TestPartitions () {
            int width = Partition.Nodes.Width;
            int height = Partition.Nodes.Height;
            bool[,] casted = new bool[width,height];

            foreach (CCoordinate coor in Raycaster.GetRelevantNodeCoordinates (startPos,endPos)) {
                if (Partition.CheckValid(coor.X,coor.Y))
                    casted[coor.X,coor.Y] = true;
            }

            Vector3 size = new Vector3(1 << Partition.AdditionalShiftSize, .1f, 1 << Partition.AdditionalShiftSize);
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    if (casted[i,j])
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.green;
                    Vector3 drawPos = new Vector2d (Partition.GetWorldX(i), Partition.GetWorldY(j)).ToVector3(0);
                    Gizmos.DrawCube (drawPos, size);
                    Gizmos.color = Color.black;
                    Gizmos.DrawWireCube(drawPos,size);
                }
            }
        }
    }
}