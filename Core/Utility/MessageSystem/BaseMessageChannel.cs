using UnityEngine;
using System.Collections; using FastCollections;
using System;
namespace Lockstep
{
    public abstract class BaseMessageChannel
    {
        private readonly FastList<Action<object>> _lazyClients = new FastList<Action<object>>();
        FastList<Action<object>> LazyClients {get {return _lazyClients;}}
        private readonly FastBucket<Action<object>> _clients = new FastBucket<Action<object>>();
        FastBucket<Action<object>> Clients {get {return _clients;}}

        public int ObjectSubscribe (Action<object> client) {
            return Clients.Add(client);
        }
        public void ObjectUnsubscribe (int ticketNumber) {
            Clients.RemoveAt(ticketNumber);
        }
        public void ObjectLazySubscribe (Action<object> client) {
            LazyClients.Add(client);
        }
        public void ObjectLazyUnsubscribe (Action<object> client) {
            LazyClients.Remove(client);
        }

        public void Invoke (object message) {
            for (int i = Clients.PeakCount - 1; i >= 0; i--) {
                if (Clients.arrayAllocation[i]) {
                    Clients[i].Invoke (message);
                }
            }
            for (int i = LazyClients.Count - 1; i >= 0; i--) {
                LazyClients[i].Invoke(message);
            }
            this.OnInvoke(message);
        }
        protected virtual void OnInvoke (object message) {

        }

    }
}