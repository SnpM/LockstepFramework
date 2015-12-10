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

        public EnvironmentHelper GetHelper () {
            return new EnvironmentHelper(this);
        }

        public void ScanAndSave () {
            LSBody[] allBodies = GameObject.FindObjectsOfType<LSBody> ();
            FastList<EnvironmentBodyInfo> bodiesBuffer = new FastList<EnvironmentBodyInfo>();
            foreach (LSBody body in allBodies) {
                if (body.GetComponent <LSAgent> () != null) continue;
                EnvironmentBodyInfo bodyInfo = new EnvironmentBodyInfo(
                    body,
                    new Vector2d(body.transform.position),
                    new Vector2d (Mathf.Sin (body.transform.rotation.x), Mathf.Cos (body.transform.rotation.y))
                );
                bodiesBuffer.Add(bodyInfo);
            }
            _environmentBodies = bodiesBuffer.ToArray();
        }
    }
}