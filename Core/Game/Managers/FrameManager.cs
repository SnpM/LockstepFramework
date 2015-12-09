using System;
using UnityEngine;

namespace Lockstep
{
    public static class FrameManager
    {
        private const int StartCapacity = 30000;
        private static bool[] hasFrame = new bool[StartCapacity];
        private static Frame[] frames = new Frame[StartCapacity];
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

        public static void TweakFramerate()
        {
            if (CommandManager.sendType == SendState.Network && ClientManager.NetworkHelper.IsServer == false)
            {
                float scaler = (float)(ForeSight);
                scaler -= 2;
                scaler /= 32;
                Time.timeScale = Mathf.Lerp(Time.timeScale, 1f + (scaler), .5f);
            } else
            {
                //Time.timeScale = 1f;
            }
        }

        private static int nextFrame;

        public static bool FreeSimulate { get; private set; }

        public static  int EndFrame { get; set; }

        public static bool CanAdvanceFrame
        {
            get { return (FreeSimulate || ForeSight > 0 && (CommandManager.sendType != SendState.Network || ClientManager.GameStarted));}
        }

        public static bool HasFrame(int frame)
        {
            return frame < capacity && hasFrame [frame];
        }

        public static void Initialize()
        {
            FreeSimulate = false;
            EndFrame = -1;

            ForeSight = 0;
            nextFrame = 0;
            hasFrame.Clear();
        }

        public static void Simulate()
        {
            if (FreeSimulate)
                return;
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
            if (LockstepManager.InfluenceFrameCount == EndFrame)
            {
                FreeSimulate = true;
            }
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