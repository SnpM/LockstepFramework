using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace Lockstep
{
    public class LSMessageManager
    {
        private readonly Dictionary<Type,object> Stops = new Dictionary<Type, object>();
        public int Subscribe <TMessage> (Action<TMessage> client, int channelID = 0) {
            MessageChannel<TMessage> channel = GetMessageChannel<TMessage> ( channelID);
            int ticket = channel.Subscribe (client);
            return ticket;
        }
        public void Unsubscribe <TMessage> (int ticket, int channelID = 0) {
            MessageChannel<TMessage> channel = GetMessageChannel<TMessage> (channelID);
            channel.Unsubscribe (ticket);
        }
        public void LazySubscribe <TMessage> (Action<TMessage> client, int channelID = 0) {
            MessageChannel<TMessage> channel = GetMessageChannel<TMessage> ( channelID);
            channel.LazySubscribe (client);
        }
        public void Unsubscribe <TMessage> (Action<TMessage> client, int channelID = 0) {
            MessageChannel<TMessage> channel = GetMessageChannel<TMessage> (channelID);
            channel.LazyUnsubscribe (client);
        }
            
        public void Invoke<TMessage> (TMessage message, int channelID = 0) {
            MessageChannel<TMessage> channel = GetMessageChannel<TMessage> (channelID);
            channel.Invoke(message);
        }
        private MessageStop<TMessage> GetMessageStop <TMessage> () {
            MessageStop<TMessage> stop;
            object stopObj;
            Type messageType = typeof (TMessage);
            if (!Stops.TryGetValue (messageType, out stopObj)) {
                stop = new MessageStop<TMessage>();
                Stops.Add(messageType, (object)stop);
            }
            else {
                stop = (MessageStop<TMessage>)stopObj;
            }
            return stop;
        }
        private MessageChannel<TMessage> GetMessageChannel <TMessage> (int channelID) {
            MessageStop<TMessage> stop = GetMessageStop<TMessage> ();
            MessageChannel<TMessage> channel = stop.GetChannel(channelID);
            return channel;
        }
    }
}