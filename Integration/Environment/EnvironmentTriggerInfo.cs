using UnityEngine;
using System.Collections;

namespace Lockstep
{
    [System.Serializable]
    public class EnvironmentTriggerInfo
    {
        public EnvironmentTriggerInfo (LSTrigger trigger) {
            Trigger = trigger;
        }
        public LSTrigger Trigger;
    }
}