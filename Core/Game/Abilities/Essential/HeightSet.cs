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
            if (Agent.Body.PositionChangedBuffer) {
                this.Agent.Body.HeightPos = HeightmapHelper.Instance.GetHeight (MapIndex, Agent.Body.Position);
            }
        }
    }
}