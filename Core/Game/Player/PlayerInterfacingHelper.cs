using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public abstract class PlayerInterfacingHelper
    {
        public void Initialize()
        {
            this.OnInitialize();
        }

        protected virtual void OnInitialize()
        {

        }

        public void LateInitialize()
        {
            this.OnLateInitialize();
        }

        protected virtual void OnLateInitialize()
        {

        }

        public void Simulate()
        {
            this.OnSimulate();   
        }

        protected virtual void OnSimulate()
        {

        }

        public void Visualize()
        {
            this.OnVisualize();
        }

        protected virtual void OnVisualize()
        {
        }

        public void Deactivate()
        {
            this.OnDeactivate();
        }

        protected virtual void OnDeactivate()
        {
        }
    }
}