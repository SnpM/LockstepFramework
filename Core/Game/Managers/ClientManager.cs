using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
	public class ClientManager : MonoBehaviour
	{
        public static NetworkHelper NetworkHelper;
		const bool SimulateNetworking = false;
        private static int _roomSize = 1;
        public static int RoomSize {get {return _roomSize;}private set {_roomSize = value;}}
		const ushort Port = ushort.MaxValue / 2;
		public static string IP = "127.0.0.1";
		//HomePublicIP =67.0.141.231
		//HomeLocalIP = 192.168.0.15

		public static bool GameStarted { get; private set; }

		public static ushort ID {
			get {
				return NetworkHelper.ID;
			}
		}

        public static void HostGame (int roomSize) {
            RoomSize = roomSize;
            NetworkHelper.Host (roomSize);
            MakeGame();
        }
        public static void ConnectGame (string ip) {
            IP = ip;
            NetworkHelper.Connect (ip);
            MakeGame();
        }
        private static void MakeGame () {
            //Application.LoadLevel("Domination");
        }


		public static void Setup (NetworkHelper networkHelper)
		{
            NetworkHelper = networkHelper;
			if (SimulateNetworking) {
				ServerSimulator.Setup ();
			}

			NetworkHelper.OnFrameData += HandleFrameData;
			NetworkHelper.OnInitData += HandleInitData;
			LSServer.Setup ();
		}

		public static void Initialize ()
		{
			LSServer.Initialize ();
			GameStarted = false;
			if (SimulateNetworking) {
				ServerSimulator.Initialize ();
			} else {

			}
            Registered = false;
		}
        public static bool Registered {get; private set;}
        private static void Register () {
            SendMessageToServer (MessageType.Register,LSUtility.EmptyBytes);
        }
		public static void HandleFrameData (byte[] data) {
			if (GameStarted) {
				CommandManager.ProcessPacket ((byte[])data);
			}
		}
		public static void HandleInitData (byte[] data) {
			GameStarted = true;
		}


		public static void Simulate ()
		{
            if (Registered == false) {
                if (NetworkHelper.IsConnected) {
                    Register ();
                    Registered = true;
                }
                return;
            }
			if (CommandManager.sendType == SendState.Network) {
				if (SimulateNetworking) {

				} else {
					if (isConnected) {
						while (bufferedSendData.Count > 0) {
							Distribute (bufferedSendData.Pop ());
						}
                        if (NetworkHelper.IsServer) {
							LSServer.Simulate ();
						}
					}
				}
			}
		}

		private static FastQueue<byte[]> bufferedSendData = new FastQueue<byte[]> ();

		public static void Distribute (byte[] data)
		{
			if (SimulateNetworking)
				ServerSimulator.Receive (data);
			else {
				if (isConnected) {
					if (GameStarted) {
						SendMessageToServer (MessageType.Input, data);
					}
				} else {
					bufferedSendData.Add (data);
				}
			}
		}

		public static void Deactivate ()
		{
			GameStarted = false;
			ServerSimulator.Stop ();
		}

		public static void Quit ()
		{
			if (isConnected) {
				NetworkHelper.Disconnect ();
			}
		}


		void OnGUI ()
		{
			if (SimulateNetworking == false && CommandManager.sendType == SendState.Network) {
				if (isConnected == false) {
					GUILayout.Label ("Host Address: ");

					IP = GUILayout.TextField (IP.ToString ());

					if (GUILayout.Button ("Connect", GUILayout.Width (200f))) {
						Connect (IP);
					}

					GUILayout.Space (10f);
					GUILayout.Label ("Room Size: ");
					int.TryParse (GUILayout.TextField (RoomSize.ToString ()), out _roomSize);
					if (GUILayout.Button ("Host", GUILayout.Width (200f))) {
						NetworkHelper.Host (RoomSize);
					}
				} else {

				}
			}
		}

		private static void SendMessageToServer (MessageType messageType, byte[] data)
		{
			NetworkHelper.SendMessageToServer (messageType, data);
		}

		private static bool isConnected { get { return NetworkHelper != null && NetworkHelper.IsConnected;} }

		private static void Connect (string ip)
		{
			NetworkHelper.Connect (ip);
		}
	}

}