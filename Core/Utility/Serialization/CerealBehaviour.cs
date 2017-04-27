using UnityEngine;
using System.Collections; using FastCollections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Lockstep
{
	public class CerealBehaviour : MonoBehaviour
#if UNITY_EDITOR
        //, ISerializationCallbackReceiver
#endif
	{
#if UNITY_EDITOR
		public virtual bool GetSerializedFieldNames (List<string> output) {
            return false;
        }
        public void AfterSerialize () {
            OnAfterSerialize  ();
        }
        protected virtual void OnAfterSerialize  () {

        }
        public void BeforeSerialize () {
             OnBeforeSerialize ();
        }
        protected virtual void OnBeforeSerialize () {

        }
        public void BeforeGUI () {
            OnBeforeGUI ();
        }
        protected virtual void OnBeforeGUI () {}
        public void AfterGUI () {
            OnAfterGUI();
        }
        protected virtual void OnAfterGUI () {}
        public void BeforeScene () {
            OnAfterScene ();
        }
        protected virtual void OnBeforeScene () {}
        public void AfterScene () {
            OnAfterScene ();
        }
        protected virtual void OnAfterScene () {}
        #endif

	}


}