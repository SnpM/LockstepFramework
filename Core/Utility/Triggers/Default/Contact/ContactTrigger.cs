using UnityEngine;
using System.Collections;

namespace Lockstep
{
    [RequireComponent (typeof (LSBody))]
    public class ContactTrigger : LSTrigger
    {

        [SerializeField]
        private bool _doContactEnter = true;
        [SerializeField]
        private bool _doContact = false;
        [SerializeField]
        private bool _doContactExit = true;
        [SerializeField]

        private string _contactEnterChannel = "ContactEnter";


        [SerializeField]
        private string _contactChannel = "Contact";



        [SerializeField]
        private string _contactExitChannel = "ContactExit";

        LSBody _cachedBody;
        LSBody CachedBody {get {return _cachedBody;}}

        bool EventsAttached = false;

        protected override void OnInitialize()
        {
            if (!EventsAttached) {
                _cachedBody = GetComponent<LSBody> ();   
                if (_doContactEnter) {
                    CachedBody.OnContactEnter += HandleOnContactEnter;
                }
                if (_doContact) {
                    CachedBody.OnContact += HandleOnContact;
                }
                if (_doContactExit) {
                    CachedBody.OnContactExit += HandleOnContactExit;
                }
                EventsAttached = true;
            }
        }

        void HandleOnContactEnter (LSBody other) {
            MessageManager.Instance.Invoke<ContactMessage> (new ContactMessage(CachedBody,other),_contactEnterChannel);
        }

        void HandleOnContact (LSBody other) {
            MessageManager.Instance.Invoke<ContactMessage> (new ContactMessage (CachedBody,other),_contactChannel);
        }
        void HandleOnContactExit (LSBody other) {
            MessageManager.Instance.Invoke<ContactMessage> (new ContactMessage (CachedBody,other),_contactExitChannel);
        }
        protected override void OnDeactivate ()
        {
            //No need to detach events because they're from same object
        }
    }
}