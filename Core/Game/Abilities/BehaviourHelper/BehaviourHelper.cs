using UnityEngine;
using System.Collections;
using Lockstep;
using System.Collections.Generic;
using System;

namespace Lockstep
{
    public abstract class BehaviourHelper : MonoBehaviour, IBehaviourHelper
    {

        public BehaviourHelper()
        {

        }

        private static FastList<BehaviourHelper> behaviourHelpers = new FastList<BehaviourHelper>();
        private static HashSet<Type> createdTypes = new HashSet<Type>();

        public ushort CachedListenInput { get; private set; }

        public virtual ushort ListenInput
        {
            get { return 0; }
        }


        public void Initialize()
        {
            CachedListenInput = ListenInput;
            OnInitialize();
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
            OnSimulate();
        }

        protected virtual void OnSimulate()
        {
        }

        public void LateSimulate()
        {
            OnLateSimulate();
        }

        protected virtual void OnLateSimulate()
        {

        }

        public void Visualize()
        {
            OnVisualize();
        }

        protected virtual void OnVisualize()
        {
        }

        public void GlobalExecute(Command com)
        {
            OnExecute(com);
        }

        protected virtual void OnExecute(Command com)
        {
        }

        public void RawExecute(Command com)
        {
            OnRawExecute(com);
        }

        protected virtual void OnRawExecute(Command com)
        {

        }

        public void GameStart()
        {
            OnGameStart();
        }

        protected virtual void OnGameStart()
        {

        }

        public void Deactivate()
        {
            OnDeactivate();
        }

        protected virtual void OnDeactivate()
        {
		
        }
    }
}