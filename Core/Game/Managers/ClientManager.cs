using FastCollections;
using Lockstep.NetworkHelpers;

namespace Lockstep
{
	public class ClientManager
	{
		public static NetworkHelper NetworkHelper;
		private static int _roomSize = 1;
		public static int RoomSize { get { return _roomSize; } private set { _roomSize = value; } }
		const ushort Port = ushort.MaxValue / 2;
		public static string IP = "127.0.0.1";
		//HomePublicIP =67.0.141.231
		//HomeLocalIP = 192.168.0.15

		private static bool _simulateNetworking = false;
		public static bool SimulateNetworking { get { return _simulateNetworking; } }

		public static bool GameStarted { get; private set; }

		public static int ClientID
		{
			get
			{
				return NetworkHelper.ID;
			}
		}

		public static void HostGame(int roomSize)
		{
			RoomSize = roomSize;
			NetworkHelper.Host(roomSize);
			MakeGame();
		}
		public static void ConnectGame(string ip)
		{
			IP = ip;
			NetworkHelper.Connect(ip);
			MakeGame();
		}
		private static void MakeGame()
		{
			//Application.LoadLevel("Domination");
		}


		public static void Setup()
		{
			if (SimulateNetworking)
			{
				ServerSimulator.Setup();
			}
			LSServer.Setup();
		}

		public static void Initialize(NetworkHelper networkHelper)
		{
			NetworkHelper = networkHelper;
			NetworkHelper.OnFrameData += HandleFrameData;
			NetworkHelper.OnInitData += HandleInitData;
			NetworkHelper.Initialize();

			LSServer.Initialize();
			GameStarted = false;
			if (SimulateNetworking)
			{
				ServerSimulator.Initialize();
			}
			else
			{

			}
			Registered = false;
		}
		public static bool Registered { get; private set; }
		private static void Register()
		{
			SendMessageToServer(MessageType.Register, new byte[1]);
		}
		static System.DateTime lastReceivedTime;
		public static void HandleFrameData(byte[] data)
		{
			var cur = System.DateTime.Now;
			lastReceivedTime = cur;
			if (GameStarted)
			{
				CommandManager.ProcessPacket((byte[])data);
			}
		}
		public static void HandleInitData(byte[] data)
		{
			GameStarted = true;
		}


		public static void Simulate()
		{
			if (Registered == false)
			{
				if (NetworkHelper.IsConnected)
				{
					Register();
					Registered = true;
				}
				return;
			}
			if (SimulateNetworking)
			{

			}
			else
			{
				if (isConnected)
				{
					while (bufferedSendData.Count > 0)
					{
						Distribute(bufferedSendData.Pop());
					}
					if (NetworkHelper.IsServer)
					{
						LSServer.Simulate();
					}
				}
			}

		}

		private static FastQueue<byte[]> bufferedSendData = new FastQueue<byte[]>();

		public static void Distribute(byte[] data)
		{
			if (SimulateNetworking)
			{
				ServerSimulator.Receive(data);
			}
			else
			{
				if (isConnected)
				{

					SendMessageToServer(MessageType.Input, data);
				}
				else
				{
					bufferedSendData.Add(data);
				}
			}
		}

		public static void Deactivate()
		{
			NetworkHelper.OnFrameData -= HandleFrameData;
			NetworkHelper.OnInitData -= HandleInitData;

			GameStarted = false;
			ServerSimulator.Stop();
		}

		public static void Quit()
		{
			if (isConnected)
			{
				NetworkHelper.Disconnect();
			}
		}



		private static void SendMessageToServer(MessageType messageType, byte[] data)
		{
			NetworkHelper.SendMessageToServer(messageType, data);
		}

		private static bool isConnected { get { return NetworkHelper != null && NetworkHelper.IsConnected; } }

		private static void Connect(string ip)
		{
			NetworkHelper.Connect(ip);
		}
	}

}