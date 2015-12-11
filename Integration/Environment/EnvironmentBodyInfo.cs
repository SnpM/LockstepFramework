using UnityEngine;
using System.Collections;

namespace Lockstep
{
    [System.Serializable]
    public class EnvironmentBodyInfo
    {
        public EnvironmentBodyInfo (
            LSBody body,
            Vector2d position,
            Vector2d rotation
        )
        {
            this.Body = body;
            this.Position = position;
            this.Rotation = rotation;
        }

        public LSBody Body;
        public Vector2d Position;
        public Vector2d Rotation;
    }
}