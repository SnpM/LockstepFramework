using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public static class TriggerManager
    {

        private static FastBucket<LSTrigger> AssimilatedTriggers = new FastBucket<LSTrigger>();
        public static void Initialize()
        {

        }

        public static void Simulate()
        {
            for (int i = AssimilatedTriggers.PeakCount - 1; i >= 0; i--) {
                if (AssimilatedTriggers.arrayAllocation[i]) {
                    LSTrigger trigger = AssimilatedTriggers[i];
                    trigger.CheckInput();
                }
            }
        }

        internal static void Assimilate (LSTrigger trigger) {
            trigger.ID = AssimilatedTriggers.Add (trigger);
        }
        internal static void Dessimilate (LSTrigger trigger) {
            AssimilatedTriggers.RemoveAt(trigger.ID);
        }

        public static void Deactivate () {

        }
    }
}