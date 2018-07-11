using UnityEngine;
using System.Collections; using FastCollections;
using System.Collections.Generic;
namespace Lockstep.NetworkHelpers
{
	public class DefaultNetworkHelper : NetworkHelper
	{

		public bool SimulateLag;
		/// <summary>
		/// Latency in milliseconds.
		/// </summary>
		public double Latency;
		public double Jitter;

		public override void Initialize ()
		{

		}

		public override void Connect (string ip)
		{

		}
		public override void Disconnect ()
		{

		}
		public override void Host (int roomSize)
		{

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

		List<Packet> Packets = new List<Packet> ();

		protected override void OnSendMessageToAll (MessageType messageType, byte [] data)
		{
			if (SimulateLag) {
				Packets.Add (new Packet (messageType, data, Latency + Random.Range(0f,(float)Jitter)));
			} else {
				base.OnSendMessageToAll (messageType, data);
			}
		}

		void Update ()
		{
			if (SimulateLag == false) return;

			/*FastList<int> list = new FastList<int> () { 1, 2, 3, 4, 5, 6, 7, 8 };
			string ss = "";
			for (int j = 0; j < list.Count; j++) {
				ss += list [j] + ", ";
				
				list.RemoveAt (j);
				j--;
			}
			Debug.Log (ss);
			return;*/
			for (int i = 0; i < Packets.Count; i++) {
				var packet = Packets [i];
				packet.TimeTillArrival -= Time.unscaledDeltaTime * 1000d;
				if (packet.TimeTillArrival <= 0) {
					base.OnSendMessageToAll (packet.MessageType, packet.Data);
					Packets.RemoveAt (i);
					i--;
				} else {
					Packets [i] = packet;
				}
			}
		}

		struct Packet
		{
			public Packet (MessageType messageType, byte [] data, double timeTillArrival)
			{
				TimeTillArrival = timeTillArrival;
				MessageType = messageType;
				Data = data;
			}
			public double TimeTillArrival;
			public MessageType MessageType;
			public byte [] Data;
		}

	}
}