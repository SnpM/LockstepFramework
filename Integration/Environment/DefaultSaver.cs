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
        private EnvironmentObject[] _environmentObjects;
        public EnvironmentObject[] EnvironmentObjects {get {return _environmentObjects;}}


        protected override void OnSave () {
            SaveBodies ();
            SaveObjects ();
        }

        protected override void OnApply () {
            foreach (EnvironmentObject obj in EnvironmentObjects) {
                obj.Initialize();
            }
        }

        protected override void OnLateApply()
        {
            foreach (EnvironmentBodyInfo info in EnvironmentBodies) {
                info.Body.Initialize(info.Position,info.Rotation);
            }
            foreach (EnvironmentObject obj in EnvironmentObjects) {
                obj.LateInitialize();
            }
        }

        void SaveBodies () {
            LSBody[] allBodies = GameObject.FindObjectsOfType<LSBody> ();
            FastList<EnvironmentBodyInfo> bodiesBuffer = new FastList<EnvironmentBodyInfo>();
            foreach (LSBody body in allBodies) {
                if (IsAgent(body)) continue;
                EnvironmentBodyInfo bodyInfo = new EnvironmentBodyInfo(
                    body,
                    new Vector2dHeight(body.transform.position),
                    new Vector2d (Mathf.Sin (body.transform.rotation.x), Mathf.Cos (body.transform.rotation.y))
                );
                bodiesBuffer.Add(bodyInfo);
            }

            _environmentBodies = bodiesBuffer.ToArray();
        }
        void SaveObjects () {
            EnvironmentObject[] allObjects = GameObject.FindObjectsOfType<EnvironmentObject> ();
            FastList<EnvironmentObject> objectBuffer = new FastList<EnvironmentObject>();

            foreach (EnvironmentObject obj in allObjects) {
                if (IsAgent(obj)) continue;
                objectBuffer.Add(obj);
            }
            _environmentObjects = objectBuffer.ToArray();
        }
        static bool IsAgent (object obj) {
            MonoBehaviour mb = obj as MonoBehaviour;
            if (mb.IsNull()) return false;
            return mb.GetComponent<LSAgent>().IsNotNull();
        }

    }
}