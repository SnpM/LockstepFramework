using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public class GridDebugger : MonoBehaviour
    {
        public GridType LeGridType;
        public float LeHeight;
        [Range (.1f,.9f)]
        public float NodeSize = .4f;

        void OnDrawGizmos ()
        {
            nodeScale = new Vector3(NodeSize,NodeSize,NodeSize);
            switch (this.LeGridType)
            {
                case GridType.Pathfinding:
                    DrawPathfinding();
                    break;
            }
        }
        private Vector3 nodeScale;
        void DrawPathfinding()
        {
            for (int i = 0; i < GridManager.NodeCount; i++)
            {
                for (int j = 0; j < GridManager.NodeCount; j++) {
                    GridNode node = GridManager.GetNode(i,j);
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
