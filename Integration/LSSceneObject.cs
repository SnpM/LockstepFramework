using UnityEngine;

namespace Lockstep {
    [RequireComponent(typeof(LSBody))]
    public class LSSceneObject : MonoBehaviour {
        public bool MapToGrid;

        public void Initialize() {
            var body = GetComponent<LSBody>();
            body.Setup(null);
			body.InitializeSerialized ();
            if (MapToGrid) {
                for (long i = body.XMin; i <= body.XMax; i += FixedMath.One) {
                    for (long j = body.YMin; j <= body.YMax; j += FixedMath.One) {
                        GridManager.GetNode(i, j).Unwalkable = true;
                    }
                }
            }
        }
    }
}