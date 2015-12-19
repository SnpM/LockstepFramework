using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public class EnvironmentHelper : BehaviourHelper
    {

        public override InputCode ListenInput
        {
            get
            {
                return InputCode.None;
            }
        }


        #if UNITY_EDITOR
        [SerializeField]
        private GameObject _saverObject;
        GameObject SaverObject {get {return _saverObject;}}
        public void ScanAndSave () {
            if (SaverObject == null) {
                Debug.Log ("Please assign 'Saver Object'");
                return;
            }
            _savers = SaverObject.GetComponents<EnvironmentSaver> (); //Gets savers from SaverObject
            foreach (EnvironmentSaver saver in Savers) {
                saver.Save();
            }
        }

        void Reset () {
            this._saverObject = this.gameObject;
        }
        #endif

        [SerializeField]
        private EnvironmentSaver[] _savers;
        public EnvironmentSaver[] Savers { get {return _savers;}}

        protected override void OnInitialize()
        {
            foreach (EnvironmentSaver saver in Savers) {
                saver.Apply ();
            }
        }
        protected override void OnLateInitialize()
        {
            foreach (EnvironmentSaver saver in Savers) {
                saver.LateApply();
            }
        }
    }
}