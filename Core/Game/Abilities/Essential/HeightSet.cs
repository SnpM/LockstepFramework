using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
    [UnityEngine.DisallowMultipleComponent]
    public class HeightSet : Ability
    {
        [SerializeField]
        private int _mapIndex;

        public int MapIndex { get { return _mapIndex; } }

        [SerializeField, FixedNumber]
        private long _bonusHeight;
        public long BonusHeight {get {return _bonusHeight;}}

        private long _offset;

        [Lockstep(true)]
        public long Offset
        {
            get { return _offset; }
            set
            {
                if (_offset != value)
                {
                    _offset = value;
                    ForceUpdate = true;
                }
            }
        }

        public bool ForceUpdate { get; set; }

        public void UpdateHeight () {
            long height = HeightmapSaver.Instance.GetHeight(MapIndex, Agent.Body.Position) + _bonusHeight + Offset;
            Agent.Body.HeightPos = height;
        }

        protected override void OnSimulate()
        {
            if (Agent.Body.PositionChanged || Agent.Body.PositionChangedBuffer || ForceUpdate)
            {
                UpdateHeight ();
            }
        }
    }
}