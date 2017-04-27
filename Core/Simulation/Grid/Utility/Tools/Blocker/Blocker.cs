using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;

//Blocker for static environment pieces in a scene.
[RequireComponent (typeof (Lockstep.UnityLSBody))]
public class Blocker : EnvironmentObject
{
    static readonly FastList<Vector2d> bufferCoordinates = new FastList<Vector2d>();

    [SerializeField]
    private bool _blockPathfinding = true;
    public bool BlockPathfinding {get {return _blockPathfinding;}}


    public LSBody CachedBody {get; private set;}

    protected override void OnLateInitialize()
    {
        base.OnInitialize();

		CachedBody = this.GetComponent<UnityLSBody> ().InternalBody;

        if (this.BlockPathfinding) {
            const long gridSpacing = FixedMath.One;
            bufferCoordinates.FastClear();
            CachedBody.GetCoveredSnappedPositions (gridSpacing, bufferCoordinates);
            foreach (Vector2d vec in bufferCoordinates) {
    			GridNode node = GridManager.GetNode(vec.x,vec.y);
                int gridX, gridY;
                GridManager.GetCoordinates(vec.x,vec.y, out gridX, out gridY);
                if (node == null) continue;

                node.AddObstacle();
            }
        }
    }


        
}
