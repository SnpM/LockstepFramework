using UnityEngine;
using System.Collections; using FastCollections;
using System;
using System.Collections.Generic;

namespace Lockstep {
    public class LSBusStop  {
    
        private Dictionary<string,MessageBus> Buses = new Dictionary<string,MessageBus> ();
    
        private class MessageBus {
            public MessageBus (string id) {
                this.ID = id;
            }

            public string ID;
            public FastBucket<MessagePassenger> Passengers = new FastBucket<MessagePassenger> ();
        
            public void Activate () {
                for (int i = 0; i < Passengers.Count; i++) {
                    Passengers [i].Call ();
                }
            }

            public int Board (MessagePassenger passenger) {
                return Passengers.Add (passenger);
            }

            public void Unboard (int passengerID) {
                Passengers.RemoveAt (passengerID);
            }
        }

        private class MessagePassenger {
            public MessagePassenger (Action call) {
                this.Call = call;
            }

            public Action Call;
        }
    
        public void CreateBus (string id) {
            this._CreateBus (id);
        }
        private MessageBus _CreateBus (string id) {
            MessageBus bus;
            if (!Buses.TryGetValue (id, out bus)) {
                bus = new MessageBus (id);
                Buses.Add (id, bus);
            }
            return bus;
        }

        public void ActivateBus (string id) {
            MessageBus bus;
            if (!Buses.TryGetValue (id, out bus)) {
                return;
            }
            bus.Activate ();
        }

        public int BoardBus (string busID, Action call) {
            MessagePassenger passenger = new MessagePassenger (call);
            MessageBus bus;
            if (!Buses.TryGetValue (busID, out bus)) {
                bus = _CreateBus (busID);
            }
            return bus.Board (passenger);
        }

        public void UnboardBus (string busID, int passengerID) {
            MessageBus bus;
            if (!Buses.TryGetValue (busID, out bus)) {
                return;
            }
            bus.Unboard (passengerID);
        }
    }
}
