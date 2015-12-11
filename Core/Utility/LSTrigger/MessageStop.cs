using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lockstep
{
    public sealed class MessageStop<TMessage>
    {
        private readonly Dictionary<int,MessageChannel<TMessage>> _channels = new Dictionary<int, MessageChannel<TMessage>>();
        private Dictionary<int,MessageChannel<TMessage>> Channels {get {return _channels;}}

        public MessageStop () {

        }

        public MessageChannel<TMessage> GetChannel (int channelID) {
            MessageChannel<TMessage> channel;
            if (!Channels.TryGetValue(channelID, out channel)) {
                Channels.Add(channelID, channel = new MessageChannel<TMessage>());
            }
            return channel;
        }
    }
}