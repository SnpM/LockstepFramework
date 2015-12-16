using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public class GridDebugger : MonoBehaviour
    {
        public bool Show; //Show the grid debugging?
        public GridType LeGridType; //type of grid to show... can be changed in runtime. Possibilities: Build grid, LOS grid
        public float LeHeight; //Height of the grid
        [Range (.1f,.9f)]
        public float NodeSize = .4f; //Size of each shown grid node

        void OnDrawGizmos ()
        {
            if (Show) {
                nodeScale = new Vector3(NodeSize,NodeSize,NodeSize);
                //Switch for which grid to show
                switch (this.LeGridType)
                {
                    case GridType.Pathfinding:
                        DrawPathfinding();
                        break;
                }
            }
        }
        private Vector3 nodeScale;
        void DrawPathfinding()
        {
            for (int i = 0; i < GridManager.NodeCount; i++)
            {
                for (int j = 0; j < GridManager.NodeCount; j++) {
                    //Gets every pathfinding node and shows the draws a cube for the node
                    GridNode node = GridManager.GetNode(i,j);
                    //Color depends on whether or not the node is walkable
                    //Red = Unwalkable, Green = Walkable
                    if (node.Unwalkable) Gizmos.color = Color.red;
                    else Gizmos.color = Color.gray;
                    Gizmos.DrawCube(node.WorldPos.ToVector3(LeHeight),nodeScale);
                }
            }
        }

        public enum GridType
        {
            Pathfinding
        }
    }
}
