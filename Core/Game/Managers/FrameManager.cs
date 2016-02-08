using System;
using UnityEngine;

namespace Lockstep
{
    public static class FrameManager
    {
        private const int StartCapacity = 30000;
        private static bool[] hasFrame = new bool[StartCapacity];
        private static Frame[] frames = new Frame[StartCapacity];
        public static Frame[] Frames {get {return frames;}}
        private static int capacity = StartCapacity;
        private static int _foreSight;

        public static int ForeSight
        {
            get { return _foreSight;}
            set
            {
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
        public static void TweakFramerate()
        {
            if (AdjustFramerate)
            {
                float scaler = (float)(ForeSight);
                scaler -= 0;
                scaler /= 16;
                Time.timeScale = 1 + scaler;
            } else
            {
                Time.timeScale = 1f;
            }
        }

        private static int nextFrame;

        public static  int EndFrame { get; set; }

        public static bool CanAdvanceFrame
        {
            get { return (ForeSight > 0 && (ClientManager.GameStarted));}
        }

        public static bool HasFrame(int frame)
        {
            return frame < capacity && hasFrame [frame];
        }

        public static void Initialize()
        {
            AdjustFramerate = true;
            EndFrame = -1;

            ForeSight = 0;
            nextFrame = 0;
            hasFrame.Clear();
        }

        public static void Simulate()
        {

            TweakFramerate();
            ForeSight--;
            Frame frame = frames [LockstepManager.InfluenceFrameCount];
            if (frame.Commands .IsNotNull())
            {
                for (int i = 0; i < frame.Commands.Count; i++)
                {
                    Command com = frame.Commands [i];
                    LockstepManager.Execute (com);

                }
            }
            frames[LockstepManager.InfluenceFrameCount] = null;

        }

        public static void AddFrame(int frameCount, Frame frame)
        {
            EnsureCapacity(frameCount + 1);
            frames [frameCount] = frame;
            hasFrame [frameCount] = true;

            while (HasFrame (nextFrame))
            {
                ForeSight++;
                nextFrame++;
            }
        }

        private static void EnsureCapacity(int min)
        {
            if (capacity < min)
            {
                capacity *= 2;
                if (capacity < min)
                {
                    capacity = min;
                }
                Array.Resize(ref frames, capacity);
                Array.Resize(ref hasFrame, capacity);
            }
        }
    }

    public class Frame
    {
        public FastList<Command> Commands;

        public void AddCommand(Command com)
        {
            if (Commands == null)
            {
                Commands = new FastList<Command>(2);
            }
            Commands.Add(com);
        }
    }
}