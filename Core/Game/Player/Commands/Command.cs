using System;
using System.Collections.Generic;
using UnityEngine;
namespace Lockstep
{
    public partial class Command
    {

		public static void Setup () {
            RegisterDefaults ();
            RegisterBackwards ();
        }

        private const int CompressionShift = FixedMath.SHIFT_AMOUNT - 7;
        private const int FloatToInt = 100;
        private const float IntToFloat = 1f / FloatToInt;


        private static readonly FastList<byte> serializeList = new FastList<byte>();
        private static readonly Writer writer = new Writer(serializeList);
        private static readonly Reader reader = new Reader();

        private static ushort RegisterCount;

        private static BiDictionary<Type, ushort> RegisteredData = new BiDictionary<Type, ushort>();

        static void RegisterDefaults () {
            Register<DefaultData> ();
            Register<Vector2d> ();
            Register<Selection> ();
            Register<Vector2dHeight> ();
            Register<Coordinate> ();
        }

        public static void Register<TData>() where TData : ICommandData
        {
            if (RegisterCount > ushort.MaxValue)
            {
                throw new System.Exception(string.Format("Cannot register more than {0} types of data.", ushort.MaxValue + 1));
            }
            if (RegisteredData.ContainsKey(typeof(TData))) return;
            RegisteredData.Add(typeof(TData), RegisterCount++);
        }

        public byte ControllerID;
        public ushort LeInput;
        private Dictionary<ushort,FastList<ICommandData>> ContainedData = new Dictionary<ushort,FastList<ICommandData>>();
        private ushort ContainedTypesCount;


        public Command()
        {
        }

        public Command(ushort inputCode)
        {
            this.Initialize();
            LeInput = inputCode;
        }

        public Command(ushort inputCode, byte controllerID)
        {
            this.Initialize();
            this.LeInput = inputCode;
            this.ControllerID = controllerID;
        }

        public void Initialize()
        {
            ContainedData.Clear();
            ContainedTypesCount = 0;
        }

        public void Add<TData>(params TData[] addItems) where TData : ICommandData
        {
            for (int i = 0; i < addItems.Length; i++)
                Add(addItems [i]);
        }

        public void Add<TData>(TData item) where TData : ICommandData
        {
            ushort dataID;
            if (RegisteredData.TryGetValue(typeof(TData), out dataID))
            {
                FastList<ICommandData> items = GetItemsList(dataID);
                if (items.Count == ushort.MaxValue)
                {
                    throw new Exception("No more than '{0}' of a type can be added to a Command.");
                }
                if (items.Count == 0)
                    ContainedTypesCount++;
                items.Add(item);
            } else
            {
                throw new System.Exception(string.Format("Type '{0}' not registered.", typeof(TData)));
            }
        }

        public bool ContainsData <TData>()
        {
            return GetDataCount<TData> () > 0;
        }

        public int GetDataCount <TData>()
        {
            ushort dataID;
            if (!RegisteredData.TryGetValue(typeof(TData), out dataID))
                return 0;
            FastList<ICommandData> items;
            if (!ContainedData.TryGetValue(dataID, out items))
            {
                return 0;
            }
            return items.Count;
        }

        public TData GetData <TData>(int index = 0) where TData : ICommandData
        {
			TData item;
			if (TryGetData (out item, index)) {
				return item;
			}
			return default(TData);
        }
        public bool TryGetData<TData> (out TData data, int index = 0) where TData : ICommandData
        {
            data = default(TData);
            ushort dataID;
            if (!RegisteredData.TryGetValue (typeof (TData), out dataID)) return false;
            FastList<ICommandData> items;
            if (!this.ContainedData.TryGetValue(dataID, out items)) return false;
            if (items.Count <= index) return false;
            data = (TData)items[index];
            return true;
        }

        public void SetData<TData> (TData value, int index = 0) where TData : ICommandData
        {
            this.ContainedData[RegisteredData[typeof(TData)]][index] = value;
        }

        public bool SetFirstData<TData> (TData value) where TData : ICommandData
        {
            ushort dataID;
            if (!RegisteredData.TryGetValue (typeof (TData), out dataID)) return false;
            FastList<ICommandData> items;
            if (!this.ContainedData.TryGetValue(dataID, out items)) return false;
            if(items.Count == 0)
                items.Add(value);
            else
                items[0] = value;
            return true;
        }
        public void ClearData<TData> () {
            this.ContainedData[RegisteredData[typeof(TData)]].Clear();
        }

        /// <summary>
        /// Reconstructs this command from a serialized command and returns the size of the command.
        /// </summary>
        public int Reconstruct(byte[] Source, int StartIndex = 0)
        {
            reader.Initialize(Source, StartIndex);
            ControllerID = reader.ReadByte();
            LeInput = reader.ReadUShort();
            this.ContainedTypesCount = reader.ReadUShort();

            for (int i = 0; i < this.ContainedTypesCount; i++)
            {
                ushort dataID = reader.ReadUShort();
                ushort dataCount = reader.ReadUShort();
                FastList<ICommandData> items = GetItemsList(dataID);
                Type dataType = RegisteredData.GetReversed(dataID);
                for (int j = 0; j < dataCount; j++)
                {
                    ICommandData item = Activator.CreateInstance(dataType) as ICommandData;
                    item.Read(reader);
                    items.Add(item);
                }
            }

            return reader.Position - StartIndex;
        }


        public byte[] Serialized
        {
            get
            {
                writer.Reset();
                

                //Essential Information
                writer.Write(ControllerID);
                writer.Write(LeInput);
                writer.Write(ContainedTypesCount);
                foreach (KeyValuePair<ushort,FastList<ICommandData>> pair in ContainedData)
                {
                    writer.Write(pair.Key);
                    writer.Write((ushort)pair.Value.Count);
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        pair.Value [i].Write(writer);
                    }
                }

                return serializeList.ToArray();
            }
        }

        FastList<ICommandData> GetItemsList(ushort dataID)
        {
            FastList<ICommandData> items;
            if (!ContainedData.TryGetValue(dataID, out items))
            {
                items = new FastList<ICommandData>();
                ContainedData.Add(dataID, items);
            }
            return items;
        }
    }
}