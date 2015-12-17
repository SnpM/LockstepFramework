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
        public Action<byte[]> OnFrameData; //Frame data
        public Action<byte[]> OnInputData; //(For server) Client input
        public Action<byte[]> OnInitData; //Initialization data - Not Used (NU)
        public Action<byte[]> OnMatchmakingData; //Matchmaking data NU
        public Action<byte[]> OnRegisterData; //For registering clients NU
        public Action<byte[]> OnTestData; //For test purposes
        
        public NetworkHelper () {

        }

        public abstract void Connect (string ip);

        public abstract void Host (int roomSize);

        public abstract void Disconnect ();

        public abstract void SendMessageToServer (MessageType messageType, byte[] data);

        public abstract void SendMessageToAll (MessageType messageType, byte[] data);

        //For receiving data
        protected void Receive (MessageType messageType, byte[] data) {
            if (OnDataReceived != null)
                OnDataReceived (messageType,data);

            //Huge switch statement for distributing data based on MessageType
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