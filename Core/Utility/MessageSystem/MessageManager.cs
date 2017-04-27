using UnityEngine;
using System.Collections; using FastCollections;
using System;
using System.Collections.Generic;
namespace Lockstep
{
    public class MessageManager
    {
        public static MessageManager Instance {get; private set;}
        static MessageManager () {
            Instance = new MessageManager();
        }
            
        public void Reset () {
            foreach (KeyValuePair<Type,BaseMessageStop> stop in this.Stops) {
                stop.Value.Clear();
            }
        }

        private readonly Dictionary<Type,BaseMessageStop> Stops = new Dictionary<Type, BaseMessageStop>();
        public int Subscribe <TMessage> (Action<TMessage> client, string channelID = "") {
            MessageChannel<TMessage> channel = GetMessageChannel<TMessage> ( channelID);
            int ticket = channel.Subscribe (client);
            return ticket;
        }
        public void Unsubscribe <TMessage> (int ticket, string channelID = "") {
            MessageChannel<TMessage> channel = GetMessageChannel<TMessage> (channelID);
            channel.Unsubscribe (ticket);
        }
        public void LazySubscribe <TMessage> (Action<TMessage> client, string channelID = "") {
            MessageChannel<TMessage> channel = GetMessageChannel<TMessage> ( channelID);
            channel.LazySubscribe (client);
        }
        public void Unsubscribe <TMessage> (Action<TMessage> client, string channelID = "") {
            MessageChannel<TMessage> channel = GetMessageChannel<TMessage> (channelID);
            channel.LazyUnsubscribe (client);
        }
            
        public void Invoke<TMessage> (TMessage message, string channelID = "") {
            MessageChannel<TMessage> channel = GetMessageChannel<TMessage> (channelID);
            channel.Invoke(message);
        }
        private MessageStop<TMessage> GetMessageStop <TMessage> () {
            MessageStop<TMessage> stop;
            BaseMessageStop baseStop = GetMessageStop (typeof (TMessage));
            stop = baseStop as MessageStop<TMessage>;
            return stop;
        }
        private BaseMessageStop GetMessageStop (Type messageType) {
            BaseMessageStop baseStop;
            if (!Stops.TryGetValue (messageType, out baseStop)) {
                Type genericClass = typeof(MessageStop<>);
                // MakeGenericType is badly named
                Type constructedClass = genericClass.MakeGenericType(messageType);

                object objStop = Activator.CreateInstance(constructedClass);
                baseStop = (BaseMessageStop)objStop;

                Stops.Add(messageType, baseStop);

            }
            else {
            }
            return baseStop;
        }
        private MessageChannel<TMessage> GetMessageChannel <TMessage> (string channelID) {
            return GetMessageChannel (typeof (TMessage), channelID) as MessageChannel<TMessage>;
        }

        public BaseMessageChannel GetMessageChannel (Type messageType, string channelID) {
            if (channelID == null)
                channelID = "";
            BaseMessageStop baseStop = GetMessageStop (messageType);
            return baseStop.GetChannel(channelID);

        }
    }
}