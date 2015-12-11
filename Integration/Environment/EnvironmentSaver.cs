using UnityEngine;
using System.Collections;
using Lockstep;

namespace Lockstep
{
    public class EnvironmentSaver : MonoBehaviour
    {
        [SerializeField]
        private EnvironmentBodyInfo[] _environmentBodies;
        public EnvironmentBodyInfo[] EnvironmentBodies {get {return _environmentBodies;}}
        [SerializeField]
        private EnvironmentTriggerInfo[] _environmentTriggers;
        public EnvironmentTriggerInfo[] EnvironmentTriggers {get {return _environmentTriggers;}}

        public EnvironmentHelper GetHelper () {
            return new EnvironmentHelper(this);
        }

        public void ScanAndSave () {
            SaveBodies ();
            SaveTriggers ();
        }

        void SaveBodies () {
            LSBody[] allBodies = GameObject.FindObjectsOfType<LSBody> ();
            FastList<EnvironmentBodyInfo> bodiesBuffer = new FastList<EnvironmentBodyInfo>();
            foreach (LSBody body in allBodies) {
                if (IsAgent(body)) continue;
                EnvironmentBodyInfo bodyInfo = new EnvironmentBodyInfo(
                    body,
                    new Vector2d(body.transform.position),
                    new Vector2d (Mathf.Sin (body.transform.rotation.x), Mathf.Cos (body.transform.rotation.y))
                );
                bodiesBuffer.Add(bodyInfo);
            }

            _environmentBodies = bodiesBuffer.ToArray();
        }
        void SaveTriggers () {
            LSTrigger[] allTriggers = GameObject.FindObjectsOfType<LSTrigger> ();
            FastList<EnvironmentTriggerInfo> triggerBuffer = new FastList<EnvironmentTriggerInfo>();
            foreach (LSTrigger trigger in allTriggers) {
                if (IsAgent(trigger)) continue;
                EnvironmentTriggerInfo triggerInfo = new EnvironmentTriggerInfo(trigger);
                triggerBuffer.Add(triggerInfo);
            }
            _environmentTriggers = triggerBuffer.ToArray();
        }
        static bool IsAgent (object obj) {
            MonoBehaviour mb = obj as MonoBehaviour;
            if (mb.IsNull()) return false;
            return mb.GetComponent<LSAgent>().IsNotNull();
        }
    }
}