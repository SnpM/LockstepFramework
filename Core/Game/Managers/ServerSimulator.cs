using System.Collections;
using FastCollections;
using System;

namespace Lockstep
{
	public static class ServerSimulator
	{
		static FastList<byte> bufferBytes = new FastList<byte>();
		const float Latency = .1f;
		const float Jitter = .0f;

		private static bool IsSimulating;

		private static FastList<byte> receivedBytes = new FastList<byte>();

		private static int InfluenceFrameCount;
		public static void Setup()
		{
			UnityInstance.Instance.StartCoroutine(Tick());
		}
		public static void Initialize()
		{
			InfluenceFrameCount = 0;
			IsSimulating = true;
			ClientManager.HandleInitData(new byte[0]);
		}
		public static void Stop()
		{
			IsSimulating = false;
		}

		public static void Receive(byte[] data)
		{
			UnityInstance.Instance.StartCoroutine(receive(data));
		}
		static IEnumerator receive(byte[] data)
		{
			yield return LSUtility.WaitRealTime(UnityEngine.Random.Range(Latency, Latency + Jitter));
			receivedBytes.AddRange(data);
			yield break;
		}
		static void Send(byte[] data)
		{
			UnityInstance.Instance.StartCoroutine(send(data));

		}
		static IEnumerator send(byte[] data)
		{
			yield return LSUtility.WaitRealTime(UnityEngine.Random.Range(Latency, Latency + Jitter));
			ClientManager.HandleFrameData(data);
			yield break;
		}

		static IEnumerator Tick()
		{
			while (true)
			{
				if (IsSimulating && LockstepManager.GameStarted)
				{
					bufferBytes.FastClear();
					bufferBytes.AddRange(BitConverter.GetBytes(InfluenceFrameCount));
					InfluenceFrameCount++;
					bufferBytes.AddRange(receivedBytes);
					receivedBytes.FastClear();
					Send(bufferBytes.ToArray());
				}
				yield return LSUtility.WaitRealTime(LockstepManager.DeltaTimeF * LockstepManager.InfluenceResolution);
			}
		}
	}
}