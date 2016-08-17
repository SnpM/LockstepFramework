using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public class EnvironmentHelper : BehaviourHelper
    {

        public override ushort ListenInput
        {
            get
            {
                return 0;
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
            InitializeEnvironmentFromObject();
            foreach (EnvironmentSaver saver in Savers) {
                saver.Save();
				UnityEditor.EditorUtility.SetDirty(saver);
            }
        }

        protected void Awake()
        {
            InitializeEnvironmentFromObject();
        }

        protected void InitializeEnvironmentFromObject()
        {
            if(SaverObject != null)
            {
                _savers = SaverObject.GetComponents<EnvironmentSaver>(); //Gets savers from SaverObject
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