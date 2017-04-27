using System;
using UnityEngine;
using FastCollections;

namespace Lockstep
{
	public static class FrameManager
	{
		private const int StartCapacity = 30000;
		private static bool [] hasFrame = new bool [StartCapacity];
		private static Frame [] frames = new Frame [StartCapacity];
		public static Frame [] Frames { get { return frames; } }
		private static int capacity = StartCapacity;
		private static int _foreSight;
		public static int LoadedFrames { get; private set; }
		public static int ForeSight {
			get { return _foreSight; }
			private set {
				_foreSight = value;

				//Scaling for latency buffering

			}
		}
		private static bool _adjustFramerate;
		public static bool AdjustFramerate {
			get {
				return _adjustFramerate;
			}
			set {
				_adjustFramerate = value;
			}
		}

		static float jitterFactor = 0f;
		static float lastScaler = 1f;
		/// <summary>
		/// Tweaks how fast to run frames based on network quality.
		/// </summary>
		public static void TweakFramerate ()
		{
			float jitterCompensation = LSNetworkSettings.Settings.JitterSettings.JitterCompensation;
			float jitterSensitivity = LSNetworkSettings.Settings.JitterSettings.JitterSensitivity;
			float jitterDegrade = LSNetworkSettings.Settings.JitterSettings.JitterDegrade;
			const int rate = LockstepManager.FrameRate;
			if (AdjustFramerate) {
				float scaler = (float)(ForeSight);

				{
					if (Mathf.Abs (scaler - lastScaler) > 0)
						jitterFactor = Mathf.Lerp (jitterFactor, Mathf.Abs (scaler - lastScaler), jitterSensitivity);
					else
						jitterFactor = Mathf.Lerp (jitterFactor, 0, jitterDegrade);
					lastScaler = scaler;

					float jitterEffect = jitterFactor * jitterCompensation;
					scaler -= jitterEffect;
				}

				scaler /= rate;
				Time.timeScale = Mathf.Lerp (Time.timeScale, 1 + scaler, 1 / (float)rate);
			} else {
				//Time.timeScale = 1f;
			}
		}

		private static int nextFrame;

		public static int EndFrame { get; set; }

		public static bool CanAdvanceFrame {
			get { return (ForeSight > 0 && (ClientManager.GameStarted)); }
		}

		public static bool HasFrame (int frame)
		{
			return frame < capacity && hasFrame [frame];
		}

		public static void Initialize ()
		{
			AdjustFramerate = true;
			EndFrame = -1;

			ForeSight = 0;
			nextFrame = 0;
			hasFrame.Clear ();
		}

		public static void Simulate ()
		{

			TweakFramerate ();
			ForeSight--;
			Frame frame = frames [LockstepManager.InfluenceFrameCount];
			if (frame.Commands.IsNotNull ()) {
				for (int i = 0; i < frame.Commands.Count; i++) {
					Command com = frame.Commands [i];

					LockstepManager.Execute (com);
				}
			}
			//frames[LockstepManager.InfluenceFrameCount] = null;

		}

		public static void AddFrame (int frameCount, Frame frame)
		{
			EnsureCapacity (frameCount + 1);
			frames [frameCount] = frame;

			hasFrame [frameCount] = true;

			while (HasFrame (nextFrame)) {
				ForeSight++;
				nextFrame++;
				LoadedFrames++;
			}
		}

		private static void EnsureCapacity (int min)
		{
			if (capacity < min) {
				capacity *= 2;
				if (capacity < min) {
					capacity = min;
				}
				Array.Resize (ref frames, capacity);
				Array.Resize (ref hasFrame, capacity);
			}
		}
	}

	public class Frame
	{
		public FastList<Command> Commands;

		public void AddCommand (Command com)
		{
			if (Commands == null) {
				Commands = new FastList<Command> (2);
			}
			Commands.Add (com);
		}
	}
}