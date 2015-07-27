using System.IO;
using System;
using UnityEngine;

namespace Lockstep
{
	public static class ReplayManager
	{
		#region Settings
		static readonly string Path = UnityEngine.Application.persistentDataPath;
		static readonly char PathSeperator = System.IO.Path.DirectorySeparatorChar;
		const string FileExtension = ".replay";
		#endregion

		public static bool IsPlayingBack;
		public static byte[] PlaybackBytes;
		public static int PlaybackPosition;

		private static int LastFrameCount;
		private static int LastFrameByteCount;
		private static bool IsWaitingOnFrame;
		private static int RemainingFrameCount;

		public static void Play (string Name)
		{
			IsPlayingBack = true;
			NetworkManager.sendState = SendState.None;
			PlaybackBytes = File.ReadAllBytes (ToFilePath (Name));
			PlaybackPosition = 0;
			RemainingFrameCount = 0;
			IsWaitingOnFrame = false;
		}

		public static void Stop ()
		{
			IsPlayingBack = false;
		}

		public static void Save (string Name)
		{
			byte[] saveBytes = new byte[NetworkManager.RecordedBytes.Count + 4];
			Array.Copy (NetworkManager.RecordedBytes.innerArray, saveBytes, NetworkManager.RecordedBytes.Count);
			Array.Copy (BitConverter.GetBytes (NetworkManager.ReceivedFrameCount - NetworkManager.LastRecordedFrame),
			            0,
			            saveBytes,
			            saveBytes.Length - 4,
			            4);
			Debug.Log (saveBytes.Length);
			File.WriteAllBytes (ToFilePath (Name), saveBytes);
		}

		public static void Simulate ()
		{
			if (IsPlayingBack == true) {
				if (NetworkManager.IterationCount == 0) {
					if (PlaybackPosition < PlaybackBytes.Length - 4) {
						if (IsWaitingOnFrame)
						{

						}
						else {
							LastFrameByteCount = (int)BitConverter.ToUInt16 (PlaybackBytes, PlaybackPosition);
							PlaybackPosition += 2;
							LastFrameCount = BitConverter.ToInt32 (PlaybackBytes,PlaybackPosition);
						}
						if (LockstepManager.FrameCount == LastFrameCount)
						{
							NetworkManager.ReceivedBytes.AddRange (PlaybackBytes, PlaybackPosition, LastFrameByteCount);
							PlaybackPosition += LastFrameByteCount;
							IsWaitingOnFrame = false;
						}
						else {
							IsWaitingOnFrame = true;
							NetworkManager.ReceivedBytes.AddRange (BitConverter.GetBytes (LockstepManager.FrameCount));
						}
					}
					else if (PlaybackPosition < PlaybackBytes.Length)
					{
						RemainingFrameCount = BitConverter.ToInt32 (PlaybackBytes,PlaybackBytes.Length - 4);
						PlaybackPosition += 4;
					}

					if (RemainingFrameCount > 0)
					{
						RemainingFrameCount--;
						NetworkManager.ReceivedBytes.AddRange (BitConverter.GetBytes (LockstepManager.FrameCount));
					}
				}

			}
		}

		public static string ToFilePath (string Name)
		{
			return Path + PathSeperator + Name + FileExtension;
		}

	}

	public enum RecordState
	{
		None,
		Record,
		Playback
	}
}