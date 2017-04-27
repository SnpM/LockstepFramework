using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
    [System.Serializable]
    public class EnvironmentBodyInfo
    {
        public EnvironmentBodyInfo (
            UnityLSBody body,
            Vector3d position,
            Vector2d rotation
        )
        {
            this.Body = body;
            this.Position = position;
            this.Rotation = rotation;
        }

        public UnityLSBody Body;
        public Vector3d Position;
        public Vector2d Rotation;
    }
}