using UnityEngine;
using System.Collections;
using Lockstep;

namespace Lockstep
{
    public class DefaultSaver : EnvironmentSaver
    {
        [SerializeField]
        private EnvironmentBodyInfo[] _environmentBodies;
        public EnvironmentBodyInfo[] EnvironmentBodies {get {return _environmentBodies;}}
        [SerializeField]
        private EnvironmentTriggerInfo[] _environmentTriggers;
        public EnvironmentTriggerInfo[] EnvironmentTriggers {get {return _environmentTriggers;}}


        protected override void OnSave () {
            SaveBodies ();
            SaveTriggers ();
        }

        protected override void OnApply () {
            foreach (EnvironmentBodyInfo info in EnvironmentBodies) {
                info.Body.InitializeSerialized();
            }
            foreach (EnvironmentTriggerInfo info in EnvironmentTriggers) {
                info.Trigger.Initialize();
            }
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