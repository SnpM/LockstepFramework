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

		public static void Play (Replay replay)
		{
			IsPlayingBack = true;
			StartStreaming (replay);
		}

		public static void Stop ()
		{
			if (IsPlayingBack)
			{
				AgentController.Deactivate ();
				IsPlayingBack = false;
				StopStreaming ();
			}
        }
        public static IEnumerator SerializeCurrent (Replay outputReplay) {
            IEnumerator enumerator =  Serialize (FrameManager.Frames,outputReplay);
            while (enumerator.MoveNext())
                yield return enumerator.Current;
            outputReplay.hash = LockstepManager.GetStateHash();
        }
        public static IEnumerator Serialize (Frame[] frames, Replay outputReplay) {
            bufferBytes.FastClear();

            int length = frames.Length;
            int lastCommandedFrame = 0;
            for (int i = 0; i < length; i++) {
                Frame frame = frames[i];
                if (frame.Commands.Count > 0) {
                    bufferBytes.AddRange(BitConverter.GetBytes(i));
                    for (int j = 0; j < frame.Commands.Count; j++) {
                        bufferBytes.AddRange(frame.Commands[j].Serialized);
                    }
                    lastCommandedFrame = i;
                    yield return 0;
                }
            }
            outputReplay.FrameCount = length;
            outputReplay.LastCommandedFrameCount = lastCommandedFrame;
            outputReplay.Frames = bufferBytes.ToArray();
        }

		static FastList<byte> bufferBytes = new FastList<byte> ();

		public static IEnumerator StreamPlayback (Replay playbackReplay)
		{
			int lastFrameByteCount = 0;
			int playbackPosition = 0;
            byte[] playbackBytes = playbackReplay.Frames;

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