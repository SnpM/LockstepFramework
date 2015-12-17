using UnityEngine;
using System.Collections;
using System;

namespace Lockstep {
    public abstract class NetworkHelper {
        public abstract bool IsConnected { get; }

        public abstract ushort ID { get; }

        public abstract bool IsServer { get; }

        public abstract int PlayerCount { get; }

        public Action<MessageType,byte[]> OnDataReceived;
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

        protected void Receive (MessageType messageType, byte[] data) {
            if (OnDataReceived != null)
                OnDataReceived (messageType,data);
            switch (messageType) {
                case MessageType.Input:
                    if (OnInputData != null) {
                        OnInputData (data);
                    }
                    break;
                case MessageType.Frame:
                    if (OnFrameData != null) {
                        OnFrameData (data);
                    }
                    break;
                case MessageType.Init:
                    if (OnInitData != null) {
                        OnInitData (data);
                    }
                    break;
                case MessageType.Matchmaking:
                    if (OnMatchmakingData != null) {
                        OnMatchmakingData (data);
                    }
                    break;
                case MessageType.Register:
                    if (OnRegisterData != null) {
                        OnRegisterData (data);
                    }
                    break;
                case MessageType.Test:
                    if (OnTestData != null) {
                        OnTestData (data);
                    }
                    break;
            }
        }
    }
}