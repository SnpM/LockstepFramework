using UnityEngine;
using System.Collections; using FastCollections;
using System;
namespace Lockstep
{
	public static class LSServer
	{
		public static uint InfluenceFrameCount { get; private set; }
		public static void Setup () {
		}

        static void HandleOnRegisterData (byte[] obj)
        {
            InitedPlayerCount++;
            if (InitedPlayerCount == ClientManager.RoomSize) {
                StartGame ();
            }
        }


		public static void Initialize ()
		{
			ClientManager.NetworkHelper.OnInputData += HandleOnInputData;
			ClientManager.NetworkHelper.OnRegisterData += HandleOnRegisterData;
            InitedPlayerCount = 0;
			InfluenceFrameCount = 0;
			GameStarted = false;
			bufferBytes.EnsureCapacity(4096);
		}

		public static void Deactivate()
		{
			ClientManager.NetworkHelper.OnInputData -= HandleOnInputData;
			ClientManager.NetworkHelper.OnRegisterData -= HandleOnRegisterData;
		}

		static void HandleOnInputData (byte[] obj)
		{
			receivedBytes.AddRange (obj);
		}

		public static void StartGame () {
			ClientManager.NetworkHelper.SendMessageToAll (MessageType.Init, new byte[0]);
			GameStarted = true;
		}

		private static readonly FastList<byte> bufferBytes = new FastList<byte>();
		private static readonly FastList<byte> receivedBytes = new FastList<byte>();
		static bool GameStarted = false;

        private static int InitedPlayerCount;

		public static void Simulate ()
		{

			if (GameStarted == false) {
			}
			else {
                
				bufferBytes.FastClear ();
				bufferBytes.AddRange (BitConverter.GetBytes (InfluenceFrameCount));
				bufferBytes.AddRange (receivedBytes);
				receivedBytes.FastClear ();
				ClientManager.NetworkHelper.SendMessageToAll (MessageType.Frame, bufferBytes.ToArray ());
                InfluenceFrameCount++;

			}
		}
	}
}