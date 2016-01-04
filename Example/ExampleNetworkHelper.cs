using UnityEngine;
using System.Collections;

namespace Lockstep.Example {
    public class ExampleNetworkHelper : NetworkHelper {
        public override void Connect (string ip) {

        }
        public override void Disconnect () {

        }
        public override void Host (int roomSize) {

        }
        public override int ID {
            get {
                return 0;
            }
        }
        public override bool IsConnected {
            get {
                return true;
            }
        }
        public override bool IsServer {
            get {
                return true;
            }
        }
        public override int PlayerCount {
            get {
                return 1;
            }
        }
        protected override void OnSendMessageToAll(MessageType messageType, byte[] data)
        {
            base.OnSendMessageToAll(messageType,data);
        }

    }
}