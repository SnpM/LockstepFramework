using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public abstract class BaseMessageStop
    {
        public BaseMessageStop()
        {

        }

        public abstract BaseMessageChannel GetChannel(string channelID);
        public abstract void Clear ();
    }
}