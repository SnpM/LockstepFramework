using UnityEngine;
using System.Collections; using FastCollections;
using System;

namespace Lockstep.NetworkHelpers {
    public abstract class NetworkHelper : MonoBehaviour{
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

		public virtual void Initialize ()
		{
			
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

		public virtual void Disconnect() { }

        /// <summary>
        /// Sends the message to server. If this client is the server, automatically sends message directly.
        /// </summary>
        /// <param name="messageType">Message type.</param>
        /// <param name="data">Data.</param>
        public void SendMessageToServer (MessageType messageType, byte[] data) {

            OnSendMessageToServer (messageType,data);
        }
        protected virtual void OnSendMessageToServer (MessageType messageType, byte[] data) {
            this.Receive (messageType, data);
        }

        /// <summary>
        /// Used by the server to send a message to everyone.
        /// </summary>
        /// <param name="messageType">Message type.</param>
        /// <param name="data">Data.</param>
        public void SendMessageToAll (MessageType messageType, byte[] data) {
            if (this.IsServer) {
                OnSendMessageToAll (messageType,data);
            }
            else {
                Debug.LogError("Only server can send message to all!");
            }
        }
        protected virtual void OnSendMessageToAll (MessageType messageType, byte[] data) {
            this.Receive(messageType, data);
        }

        /// <summary>
        /// Receives data and sends it to the lockstep frame logic. Call from derived class.
        /// </summary>
        /// <param name="messageType">Message type.</param>
        /// <param name="data">Data.</param>
        protected void Receive (MessageType messageType, byte[] data) {
            if (OnDataReceived != null)
                OnDataReceived.Invoke (messageType,data);
            //Huge switch statement for distributing data based on MessageType
            switch (messageType) {
			case MessageType.Input:
				if (OnInputData != null) {
					OnInputData.Invoke (data);
				}
                    break;
                case MessageType.Frame:
                    if (OnFrameData != null) {
                        OnFrameData.Invoke (data);
                    }
                    break;
                case MessageType.Init:
                    if (OnInitData != null) {
                        OnInitData.Invoke (data);
                    }
                    break;
                case MessageType.Matchmaking:
                    if (OnMatchmakingData != null) {
                        OnMatchmakingData.Invoke (data);
                    }
                    break;
                case MessageType.Register:
                    if (OnRegisterData != null) {
                        OnRegisterData.Invoke (data);
                    }
                    break;
                case MessageType.Test:
                    if (OnTestData != null) {
                        OnTestData.Invoke (data);
                    }
                    break;
            }
        }
    }
}