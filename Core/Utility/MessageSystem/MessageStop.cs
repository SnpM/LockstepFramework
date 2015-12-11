using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lockstep
{
    public sealed class MessageStop<TMessage>
    {
        private readonly Dictionary<string,MessageChannel<TMessage>> _channels = new Dictionary<string, MessageChannel<TMessage>>();
        private Dictionary<string,MessageChannel<TMessage>> Channels {get {return _channels;}}

        public MessageStop () {

        }

        public MessageChannel<TMessage> GetChannel (string channelID) {
            MessageChannel<TMessage> channel;
            if (!Channels.TryGetValue(channelID, out channel)) {
                Channels.Add(channelID, channel = new MessageChannel<TMessage>());
            }
            return channel;
        }
    }
}