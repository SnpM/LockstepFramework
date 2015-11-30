using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Lockstep {
    public class LSMessageBus {
    
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
            MessageBus bus = new MessageBus (id);
            Buses.Add (id, bus);
        }

        public void ActivateBus (string id) {
            Buses [id].Activate ();
        }

        public int BoardBus (string busID, Action call) {
            MessagePassenger passenger = new MessagePassenger (call);
            MessageBus bus = Buses [busID];
            return bus.Board (passenger);
        }

        public void UnboardBus (string busID, int passengerID) {
            MessageBus bus = Buses [busID];
            bus.Unboard (passengerID);
        }
    }
}
