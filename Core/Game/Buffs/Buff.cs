using UnityEngine;
using System.Collections; using FastCollections;
using System;
namespace Lockstep
{
    public class Buff
    {
        
        protected int Duration;
        protected int Timer;
        protected LSAgent Target;
        internal int ID {get; set;}
        public bool Active {get; private set;}
        public void Initialize (int duration, LSAgent target) {
            Duration = duration;
            Timer = 0;
            Target = target;

            Target.AddBuff(this);
            Active = true;
            this.OnInitialize();
        }

        protected virtual void OnInitialize () {

        }

        public void Simulate () {
            Timer++;
            OnSimulate ();
            if (Duration >= 0)
            if (Timer > Duration) {
                Deactivate ();
            }
        }

        protected virtual void OnSimulate () {

        }

        public void Deactivate () {
            Target.RemoveBuff(this);
            Active = false;
            this.OnDeactivate();
        }
        protected virtual void OnDeactivate () {

        }
    }
}