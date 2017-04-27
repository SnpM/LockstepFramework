using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
    [DisallowMultipleComponent]

    public class DynamicBlocker : Ability
    {
        static readonly FastList<Vector2d> bufferCoordinates = new FastList<Vector2d>();

        LSBody CachedBody;

        protected override void OnInitialize()
        {
            CachedBody = Agent.Body;
            UpdateCoordinates ();
        }

        void RemoveLastCoordinates () {
            for (int i = 0; i < LastCoordinates.Count; i++) {
                GridNode node = LastCoordinates[i];
                node.RemoveObstacle();
            }
            LastCoordinates.FastClear();
        }
        void UpdateCoordinates () {
            const long gridSpacing = FixedMath.One;
            bufferCoordinates.FastClear();
            CachedBody.GetCoveredSnappedPositions(gridSpacing, bufferCoordinates);
            foreach (Vector2d vec in bufferCoordinates)
            {
                GridNode node = GridManager.GetNode(vec.x, vec.y);

                if (node == null)
                    continue;

                node.AddObstacle();
                LastCoordinates.Add(node);
            }
        }


        FastList<GridNode> LastCoordinates = new FastList<GridNode>();
        protected override void OnLateSimulate()
        {
            if (this.CachedBody.PositionChangedBuffer)
            {
                RemoveLastCoordinates ();
                UpdateCoordinates ();


            }
        }
        protected override void OnDeactivate()
        {
            RemoveLastCoordinates ();
        }
    }
}