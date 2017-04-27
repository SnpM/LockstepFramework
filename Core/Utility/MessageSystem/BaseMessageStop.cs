using UnityEngine;
using System.Collections; using FastCollections;

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