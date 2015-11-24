using System;

namespace Lockstep {

    [Serializable]
    public sealed class Replay {
        public string Name;
        public DateTime Date;
		public int FrameCount;
		public int LastCommandedFrameCount;
		public double Seconds {get {return FrameCount / (double)LockstepManager.FrameRate;}}
        public byte[] Content;
    }
    
}