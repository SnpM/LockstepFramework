using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
	public static class ReplayManager
	{

		#region public members
		public static bool IsPlayingBack;

		public static System.Action<bool> onIsPlayingBack;

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
			FrameManager.AdjustFramerate = false;

			if (onIsPlayingBack != null) {
				onIsPlayingBack.Invoke(IsPlayingBack);
			}
		}

		public static void Stop ()
		{
			if (IsPlayingBack)
			{
				AgentController.Deactivate ();
				IsPlayingBack = false;
				StopStreaming ();

				if (onIsPlayingBack != null) {
					onIsPlayingBack.Invoke(IsPlayingBack);
				}
			}
			FrameManager.AdjustFramerate = true;

		}

        static Writer cachedWriter = new Writer();
        public static Replay SerializeCurrent () {
            Replay replay = new Replay();
            Frame[] validFrames = new Frame[LockstepManager.InfluenceFrameCount];
            Array.Copy(FrameManager.Frames,0,validFrames,0,LockstepManager.InfluenceFrameCount);

            bufferBytes.FastClear();
            cachedWriter.Initialize(bufferBytes);
            Serialize (validFrames,cachedWriter);
            replay.Content = cachedWriter.Canvas.ToArray();
            replay.hash = LockstepManager.GetStateHash();
            return replay;
        }
        public static void Serialize (Frame[] frames, Writer writer) {
            int length = frames.Length;
            writer.Write(length);
            for (int i = 0; i < length; i++) {
                Frame frame = frames[i];
                if (frame.Commands.IsNotNull() && frame.Commands.Count > 0) {
                    writer.Write((int)i);
                    writer.Write((ushort)frame.Commands.Count);
                    for (int j = 0; j < frame.Commands.Count; j++) {

                        writer.Write(frame.Commands[j].Serialized);

                    }

                }
            }
        }

        static FastList<Frame> bufferFrames = new FastList<Frame>();
        public static Frame[] Deserialize (Reader reader) {
            bufferFrames.Clear();

            int length = reader.ReadInt();
            int lastSavedFrame = -1;
            while (reader.Position < reader.Length) {

                int frameCount = reader.ReadInt();
                ushort commandCount = reader.ReadUShort();
                Frame frame = new Frame();
                for (int i = 0; i < commandCount; i++) {

                    Command com = new Command();
                    int reconstructionCount = (com.Reconstruct(reader.Source,reader.Position));
                    reader.MovePosition(reconstructionCount);
                    frame.AddCommand(com);

                }
                for (int i = lastSavedFrame + 1; i < frameCount; i++) {
                    bufferFrames.Add(new Frame());
                }
                bufferFrames.Add(frame);
                lastSavedFrame = frameCount;

            }
            for (int i = lastSavedFrame + 1; i < length; i++) {
                bufferFrames.Add(new Frame());
            }
            return bufferFrames.ToArray();
        }


		static FastList<byte> bufferBytes = new FastList<byte> ();

		public static IEnumerator StreamPlayback (Replay playbackReplay)
		{
            CurrentReplay = playbackReplay;

            byte[] playbackBytes = playbackReplay.Content;

            yield return null;
            FrameManager.AdjustFramerate = false;

            Reader reader = new Reader();
            reader.Initialize(playbackBytes,0);
            Frame[] frames = Deserialize (reader);

            for (int i = 0; i < frames.Length; i++) {
                Frame frame = frames[i];
  
                CommandManager.ProcessFrame(i,frame);
            }
			yield break;
		}



		private static void StartStreaming (Replay replay)
		{
			StopStreaming ();
			streamer = UnityInstance.Instance.StartCoroutine(StreamPlayback(replay));
		}

		private static void StopStreaming ()
		{
			if (streamer .IsNotNull ()) {
				UnityInstance.Instance.StopCoroutine (streamer);
				streamer = null;
			}
		}
		#endregion

	
	}

}