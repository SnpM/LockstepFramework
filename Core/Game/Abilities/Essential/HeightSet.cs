using UnityEngine;
using System.Collections;
namespace Lockstep
{
    public class HeightSet : Ability
    {
        [SerializeField]
        private int _mapIndex;
        public int MapIndex {get {return _mapIndex;}}
        [SerializeField]
        private long _bonusHeight;
        protected override void OnSimulate()
        {
            if (Agent.Body.PositionChanged || Agent.Body.PositionChangedBuffer) {
                long height = HeightmapHelper.Instance.GetHeight (MapIndex, Agent.Body.Position) + _bonusHeight;
                Agent.Body.HeightPos = height;
            }
        }
    }
}