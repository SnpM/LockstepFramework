using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public class ContactMessage : IMessage
    {
        public ContactMessage (LSBody source, LSBody other) {
            Source = source;
            Other = other;
        }
        /// <summary>
        /// Body of the source of the message.
        /// </summary>
        /// <value>The source.</value>
        public LSBody Source {get; private set;}
        /// <summary>
        /// Body that hit the body of the trigger.
        /// </summary>
        /// <value>The other.</value>
        public LSBody Other {get; private set;}
    }
}