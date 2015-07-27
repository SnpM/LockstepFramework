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

		public static void Play (string Name)
		{
			IsPlayingBack = true;
			NetworkManager.sendState = SendState.None;
			PlaybackBytes = File.ReadAllBytes (ToFilePath (Name));
			PlaybackPosition = 0;
		}

		public static void Stop ()
		{
			IsPlayingBack = false;
		}

		public static void Save (string Name)
		{
			byte[] saveBytes = new byte[NetworkManager.AllReceivedBytes.Count];
			Array.Copy (NetworkManager.AllReceivedBytes.innerArray, saveBytes, NetworkManager.AllReceivedBytes.Count);
			File.WriteAllBytes (ToFilePath (Name), saveBytes);
		}

		public static void Simulate ()
		{
			if (IsPlayingBack == true) {
				if (NetworkManager.IterationCount == 0) {
					if (PlaybackPosition < PlaybackBytes.Length) {
						ushort FrameByteCount = BitConverter.ToUInt16 (PlaybackBytes, PlaybackPosition);
						PlaybackPosition += 2;
						NetworkManager.ReceivedBytes.AddRange (PlaybackBytes, PlaybackPosition, (int)FrameByteCount);
						PlaybackPosition += FrameByteCount;
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