namespace Lockstep {
    public class Command {
        private const int CompressionShift = FixedMath.SHIFT_AMOUNT - 7;
        private const int FloatToInt = 100;
        private const float IntToFloat = 1f / FloatToInt;

        private static uint ValuesMask;
        private static readonly FastList<byte> serializeList = new FastList<byte>();
        private static readonly Writer writer = new Writer(serializeList);
        private static readonly Reader reader = new Reader();

        private Vector2d _position;
        private ushort _target;
        private bool _flag;
        private Coordinate _coord;
        private int _count;
        private Selection _select;
        private byte _groupID;
        private float[] _transform;

        public bool HasPosition { get; private set; }
        public bool HasTarget { get; private set; }
        public bool HasFlag { get; private set; }
        public bool HasCoord { get; private set; }
        public bool HasCount { get; private set; }
        public bool HasSelect { get;  set; }
        public bool HasGroupID { get; private set; }

        public bool Used;
        public byte ControllerID;
        public InputCode LeInput;

        public Command() {}

        public Command(InputCode inputCode) {
            LeInput = inputCode;
        }
		public Command(InputCode inputCode, byte controllerID) {
			this.LeInput = inputCode;
			this.ControllerID = controllerID;
		}
        public Vector2d Position {
            get { return _position; }
            set {
                _position = value;
                HasPosition = true;
            }
        }

        public ushort Target {
            get { return _target; }
            set {
                _target = value;
                HasTarget = true;
            }
        }

        public bool Flag {
            get { return _flag; }
            set {
                _flag = value;
                HasFlag = true;
            }
        }

        public Coordinate Coord {
            get { return _coord; }
            set {
                _coord = value;
                HasCoord = true;
            }
        }

        public int Count {
            get { return _count; }
            set {
                _count = value;
                HasCount = true;
            }
        }

        public Selection Select {
            get { return _select; }
            set {
                _select = value;
                HasSelect = true;
            }
        }

        public byte GroupID {
            get { return _groupID; }
            set {
                _groupID = value;
                HasGroupID = true;
            }
        }


        /// <summary>
        /// Reconstructs this command from a serialized command and returns the size of the command.
        /// </summary>
        public int Reconstruct(byte[] Source, int StartIndex) {
            Used = false;
            reader.Initialize(Source, StartIndex);
            ControllerID = reader.ReadByte();
            LeInput = (InputCode)reader.ReadByte();
            ValuesMask = reader.ReadUInt();

            HasPosition = GetMaskBool(ValuesMask, DataType.Position);
            HasTarget = GetMaskBool(ValuesMask, DataType.Target);
            HasFlag = GetMaskBool(ValuesMask, DataType.Flag);
            HasCoord = GetMaskBool(ValuesMask, DataType.Coord);
            HasCount = GetMaskBool(ValuesMask, DataType.Count);
            HasSelect = GetMaskBool(ValuesMask, DataType.Select);
            HasGroupID = GetMaskBool(ValuesMask, DataType.GroupID);

            if (HasPosition) {
                _position.x = reader.ReadShort() << CompressionShift;
                _position.y = reader.ReadShort() << CompressionShift;
            }

            if (HasTarget) {
                _target = reader.ReadUShort();
            }

            if (HasFlag) {
                _flag = reader.ReadBool();
            }

            if (HasCoord) {
                _coord.x = reader.ReadInt();
                _coord.y = reader.ReadInt();
            }

            if (HasCount) {
                _count = reader.ReadInt();
            }

            if (HasSelect) {
                Select = new Selection();
                reader.count += Select.Reconstruct(reader.source, reader.count);
            }

            if (HasGroupID) {
                _groupID = reader.ReadByte();
            }

            return reader.count - StartIndex;
        }

        private static bool GetMaskBool(uint mask, DataType dataType) {
            return (mask & (uint)dataType) == (uint)dataType;
        }

        public byte[] Serialized {
            get {
                serializeList.FastClear();

                //Essential Information
                writer.Write(ControllerID);
                writer.Write((byte)LeInput);

                //Header 
                ValuesMask = (HasPosition ? (uint)DataType.Position : (uint)0) 
                           | (HasTarget ? (uint)DataType.Target : (uint)0) 
                           | (HasFlag ? (uint)DataType.Flag : (uint)0) 
                           | (HasCoord ? (uint)DataType.Coord : (uint)0) 
                           | (HasCount ? (uint)DataType.Count : (uint)0) 
                           | (HasSelect ? (uint)DataType.Select : (uint)0) 
                           | (HasGroupID ? (uint)DataType.GroupID : (uint)0) ;

                writer.Write(ValuesMask);

                if (HasPosition) {
                    writer.Write((short)(_position.x >> CompressionShift));
                    writer.Write((short)(_position.y >> CompressionShift));
                }

                if (HasTarget) {
                    writer.Write(_target);
                }

                if (HasFlag) {
                    writer.Write(_flag);
                }

                if (HasCoord) {
                    writer.Write(_coord.x);
                    writer.Write(_coord.y);
                }

                if (HasCount) {
                    writer.Write(_count);
                }

                if (HasSelect) {
                    writer.Write(_select.Header);
					for (int i = 0; i < 64; i++) {
                        if (_select.Data[i] != 0) {
                            writer.Write(_select.Data[i]);
                        }
                    }
                }

                if (HasGroupID) {
					writer.Write (_groupID);
                }
                return serializeList.ToArray();
            }
        }
    }
    [System.Flags]
    public enum DataType : uint {
        Position = 1 << 0,
        Target = 1 << 1,
        Flag = 1 << 2,
        Coord = 1 << 3,
        Count = 1 << 4,
        Select = 1 << 5,
        GroupID = 1 << 6,
        TExt = 1 << 7
	}
}