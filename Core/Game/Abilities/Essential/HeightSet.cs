using UnityEngine;
using System.Collections;
namespace Lockstep
{
    public class HeightSet : Ability
    {
        [SerializeField]
        private int _mapIndex;
        public int MapIndex {get {return _mapIndex;}}

        protected override void OnSimulate()
        {
            if (Agent.Body.PositionChanged || Agent.Body.PositionChangedBuffer) {
                long height = HeightmapHelper.Instance.GetHeight (MapIndex, Agent.Body.Position) + FixedMath.One / 10;
                Agent.Body.HeightPos = height;
            }
        }
    }
}