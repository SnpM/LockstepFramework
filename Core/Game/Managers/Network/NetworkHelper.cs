using UnityEngine;
using System.Collections;
using System;

namespace Lockstep {
    public abstract class NetworkHelper {
        public abstract bool IsConnected { get; }

        public abstract ushort ID { get; }

        public abstract bool IsServer { get; }

        public abstract int PlayerCount { get; }

        public Action<byte[]> OnFrameData;

        public Action<byte[]> OnInputData;
        public Action<byte[]> OnInitData;
        public Action<byte[]> OnMatchmakingData;
        public Action<byte[]> OnRegisterData;
        public Action<byte[]> OnTestData;
        
        public NetworkHelper () {

        }

        public abstract void Connect (string ip);

        public abstract void Host (int roomSize);

        public abstract void Disconnect ();

        public abstract void SendMessageToServer (MessageType messageType, byte[] data);

        public abstract void SendMessageToAll (MessageType messageType, byte[] data);
    }
}