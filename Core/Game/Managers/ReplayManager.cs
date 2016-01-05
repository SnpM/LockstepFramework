using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Collections;

namespace Lockstep
{
	public static class ReplayManager
	{

        #region public members
		public static bool IsPlayingBack;
		public static Replay CurrentReplay;
        #endregion

        #region private members
		private static UnityEngine.Coroutine streamer;
        #endregion

		static ReplayManager ()
		{

		}

        #region public methods
		const string DemoName = "Demo";

		public static void Play ()
		{
			CurrentReplay = FileManager.RetrieveReplay (DemoName);
			Play (CurrentReplay);
		}

		public static void Play (Replay replay)
		{
			CommandManager.sendType = SendState.None;
			IsPlayingBack = true;
			StartStreaming (replay);
		}

		public static void Stop ()
		{

			if (IsPlayingBack)
			{
				AgentController.Deactivate ();
				IsPlayingBack = false;
				CommandManager.sendType = CommandManager.defaultSendState;
				StopStreaming ();
			}
		}

		public static Replay Save ()
		{
			return Save (DateTime.UtcNow.ToString ());
		}

		public static Replay Save (string name)
		{
			byte[] saveBytes = CommandManager.RecordedBytes.ToArray ();
			CurrentReplay = new Replay ();
			CurrentReplay.Content = saveBytes;
			CurrentReplay.Name = name;
			CurrentReplay.Date = DateTime.UtcNow;
			CurrentReplay.FrameCount = LockstepManager.FrameCount;
			CurrentReplay.LastCommandedFrameCount = CommandManager.LastCommandedFrameCount;
			FileManager.SaveReplay (DemoName, CurrentReplay);

			return CurrentReplay;
		}

		static FastList<byte> bufferBytes = new FastList<byte> ();

		public static IEnumerator StreamPlayback (Replay playbackReplay)
		{
			int lastFrameByteCount = 0;
			int playbackPosition = 0;
			byte[] playbackBytes = playbackReplay.Content;
            
            bool getNextStream = true;
			int frameCount = 0;
			int nextFrame = -1;
            
            
			yield return null;
			FrameManager.EndFrame = playbackReplay.LastCommandedFrameCount;
            
            while (playbackPosition < playbackBytes.Length || frameCount <= nextFrame)
			{
				if (getNextStream == true)
                {
					bufferBytes.FastClear ();
		            lastFrameByteCount = (int)BitConverter.ToUInt16 (playbackBytes, playbackPosition);
					playbackPosition += 2;
					nextFrame = BitConverter.ToInt32 (playbackBytes, playbackPosition);
					bufferBytes.AddRange (playbackBytes, playbackPosition, lastFrameByteCount);
					playbackPosition += lastFrameByteCount;
					getNextStream = false;
				}

				if (nextFrame == frameCount)
				{
					getNextStream = true;
					CommandManager.ProcessPacket (bufferBytes);
				}
				else {
					CommandManager.ProcessPacket (BitConverter.GetBytes (frameCount));
                }
                frameCount++;
			}
			yield break;
		}


		private static void StartStreaming (Replay replay)
		{
			StopStreaming ();
			streamer = LockstepManager.UnityInstance.StartCoroutine (StreamPlayback (replay));
		}

		private static void StopStreaming ()
		{
			if (streamer .IsNotNull ()) {
				LockstepManager.UnityInstance.StopCoroutine (streamer);
				streamer = null;
			}
		}
        #endregion
	}
    
	public enum RecordState
	{
		None,
		Record,
		Playback
	}
}