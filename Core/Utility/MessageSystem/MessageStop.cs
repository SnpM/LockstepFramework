using UnityEngine;
using System.Collections; using FastCollections;
using System.Collections.Generic;

namespace Lockstep
{
    public sealed class MessageStop<TMessage> : BaseMessageStop
    {
        private readonly Dictionary<string,MessageChannel<TMessage>> _channels = new Dictionary<string, MessageChannel<TMessage>>();
        private Dictionary<string,MessageChannel<TMessage>> Channels {get {return _channels;}}

        public MessageStop () {

        }

        public override BaseMessageChannel GetChannel(string channelID)
        {
            return GetGenericChannel (channelID) as BaseMessageChannel;
        }
        public MessageChannel<TMessage> GetGenericChannel (string channelID) {
            MessageChannel<TMessage> channel;
            if (!Channels.TryGetValue(channelID, out channel)) {
                Channels.Add(channelID, channel = new MessageChannel<TMessage>());
            }
            return channel;
        }
        public override void Clear()
        {
            foreach (MessageChannel<TMessage> channel in Channels.Values) {
                channel.Clear();
            }
        }
    }
}