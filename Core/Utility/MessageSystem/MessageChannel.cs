using UnityEngine;
using System.Collections; using FastCollections;
using System;
namespace Lockstep
{
    public class MessageChannel<TMessage> : BaseMessageChannel
    {
        private readonly FastList<Action<TMessage>> _lazyClients = new FastList<Action<TMessage>>();
        FastList<Action<TMessage>> LazyClients {get {return _lazyClients;}}
        private readonly FastBucket<Action<TMessage>> _clients = new FastBucket<Action<TMessage>>();
        FastBucket<Action<TMessage>> Clients {get {return _clients;}}

        public MessageChannel () {

        }

        public int Subscribe (Action<TMessage> client) {
            return Clients.Add(client);
        }
        public void Unsubscribe (int ticketNumber) {
            Clients.RemoveAt(ticketNumber);
        }
        public void LazySubscribe (Action<TMessage> client) {
            LazyClients.Add(client);
        }
        public void LazyUnsubscribe (Action<TMessage> client) {
            LazyClients.Remove(client);
        }

        protected override void OnInvoke (object obj) {
            TMessage message = (TMessage)obj;
            for (int i = Clients.PeakCount - 1; i >= 0; i--) {
                if (Clients.arrayAllocation[i]) {
                    Clients[i].Invoke (message);
                }
            }
            for (int i = LazyClients.Count - 1; i >= 0; i--) {
                LazyClients[i].Invoke(message);
            }
        }

        public void Clear () {
            LazyClients.Clear();
            Clients.Clear();
        }
    }
}