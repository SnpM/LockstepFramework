using UnityEngine;
using System.Collections;
using System;

namespace Lockstep {
    public abstract class NetworkHelper {
        public abstract bool IsConnected { get; }

        public abstract int ID { get; }

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

        /// <summary>
        /// Connecting to a server with IP address of ip. Note: Not all NetworkHelpers will require Connect.
        /// </summary>
        /// <param name="ip">Ip.</param>
        public virtual void Connect (string ip) {
            throw new System.NotImplementedException("Connecting not supported for " + this.ToString() + ".");
        }

        /// <summary>
        /// Host a server with the specified address. Note: Not all NetworkHelpers will require hosting.
        /// </summary>
        /// <param name="roomSize">Room size.</param>
        public virtual void Host (int roomSize) {
            throw new System.NotImplementedException("Hosting not supported for " + this.ToString() + ".");
        }

        public virtual void Simulate () {}

        public abstract void Disconnect ();

        public virtual void SendMessageToServer (MessageType messageType, byte[] data) {
            this.Receive (messageType, data);
        }

        /// <summary>
        /// Used by a locally hosted server.
        /// </summary>
        /// <param name="messageType">Message type.</param>
        /// <param name="data">Data.</param>
        public virtual void SendMessageToAll (MessageType messageType, byte[] data) {
            this.Receive(messageType, data);
        }

        /// <summary>
        /// Receives data and sends it to the lockstep frame logic. Call from derived class.
        /// </summary>
        /// <param name="messageType">Message type.</param>
        /// <param name="data">Data.</param>
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