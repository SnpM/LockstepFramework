//NetworkHelper set up for Forge Networking V16.6 (http://developers.forgearcade.com/)

#if false
using UnityEngine;
using System.Collections;
using BeardedManStudios.Network;
using System;

namespace Lockstep {
    public class ForgeNetworkHelper : NetworkHelper {
        private const ushort Port = ushort.MaxValue / 2;
        private  NetWorker _gameNetworker;

        public NetWorker GameNetworker {
            get { return _gameNetworker;}
            private set {
                if (value != _gameNetworker) {
                    _gameNetworker = value;
                    if (_gameNetworker != null) {
                        GameNetworker.AddCustomDataReadEvent ((uint)MessageType.Input, (p,s) => HandleData (MessageType.Input, s));
                        GameNetworker.AddCustomDataReadEvent ((uint)MessageType.Frame, (p,s) => HandleData (MessageType.Frame, s));
                        GameNetworker.AddCustomDataReadEvent ((uint)MessageType.Init, (p,s) => HandleData (MessageType.Init, s));
                        GameNetworker.AddCustomDataReadEvent ((uint)MessageType.Matchmaking, (p,s) => HandleData (MessageType.Matchmaking, s));
                        GameNetworker.AddCustomDataReadEvent ((uint)MessageType.Register, (p,s) => HandleData (MessageType.Register, s));
                        GameNetworker.AddCustomDataReadEvent ((uint)MessageType.Test, (p,s) => HandleData (MessageType.Test, s));
                    }
                }
            }
        }

        public override bool IsConnected {
            get{ return GameNetworker != null && GameNetworker.Connected;}
        }

        public override ushort ID {
            get { return GameNetworker != null ? (ushort)GameNetworker.Me.NetworkId : (ushort)0;}
        }

        public override bool IsServer {
            get { return GameNetworker == null || GameNetworker.IsServer;}
        }

        public override int PlayerCount {
            get {
                return GameNetworker.Connections + 1;
            }
        }

        public ForgeNetworkHelper () {
            Networking.connected += HandleConnected;

        }

        public override void Connect (string ip) {
            Networking.Connect (
                ip, 
                Port,
                Networking.TransportationProtocolType.UDP);
        }

        public override void Host (int roomSize) {
            Networking.Host (
                Port,
                Networking.TransportationProtocolType.UDP,
                roomSize);
        }
        
        private void HandleConnected (NetWorker socket) {
            GameNetworker = socket;
        }

        public override void Disconnect () {
            if (GameNetworker != null && GameNetworker.Connected) {
                GameNetworker.Disconnect ();
                GameNetworker = null;
                Networking.Disconnect ();
                Networking.NetworkingReset ();
            }
        }

        void HandleData (MessageType messageType, NetworkingStream stream) {

            LSUtility.bufferBytes.FastClear ();

            for (int i = 0; i < stream.Bytes.Size - 1; i++) {
                LSUtility.bufferBytes.Add (stream.Bytes [stream.Bytes.StartIndex (i)]);
            }
            byte[] data = LSUtility.bufferBytes.ToArray ();
            switch (messageType) {
           
        }

         BMSByte bufferBites = new BMSByte ();

        public override void SendMessageToServer (MessageType messageType, byte[] data) {
            SendMessage (messageType, NetworkReceivers.Server, data);
        }

        public override void SendMessageToAll (MessageType messageType, byte[] data) {
            SendMessage (messageType, NetworkReceivers.All, data);
        }

        private void SendMessage (MessageType messageType, NetworkReceivers receivers, byte[] data) {
            if (GameNetworker != null) {
                bufferBites.Clear ();
                bufferBites.Append (data);
                Networking.WriteCustom (
                    (uint)messageType,
                    GameNetworker,
                    bufferBites,
                    true,
                    receivers);
            }
        }
    }
}
#endif