using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using FastCollections;
namespace Lockstep
{
    public partial class Command
    {

		public static void Setup () {
            RegisterDefaults ();
        }



        private static readonly FastList<byte> serializeList = new FastList<byte>();
        private static readonly Writer writer = new Writer(serializeList);
        private static readonly Reader reader = new Reader();

        private static ushort RegisterCount;

        private static BiDictionary<Type, ushort> RegisteredData = new BiDictionary<Type, ushort>();

		static void RegisterDefaults ()
		{
//			#if UNITY_IOS
				Register<DefaultData> ();
				Register<EmptyData> ();
				Register<Coordinate> ();
				Register<Selection> ();
				Register<Vector2d> ();
				Register<Vector3d> ();
//			#else
//				foreach (Type t in Assembly.GetCallingAssembly().GetTypes())
//				{
//					if (t.GetInterface("ICommandData") != null)
//					{
//						Register (t);
//					} 
//				}
//			#endif
		}

        private static void Register<TData>() where TData : ICommandData
        {
            Register (typeof(TData));
        }
        private static void Register (Type t) {
            if (RegisterCount > ushort.MaxValue)
            {
                throw new System.Exception(string.Format("Cannot register more than {0} types of data.", ushort.MaxValue + 1));
            }
            if (RegisteredData.ContainsKey(t)) return;
            RegisteredData.Add(t, RegisterCount++);
        }

        public byte ControllerID;
        public ushort InputCode;
        /// <summary>
        /// Backward compatability for InputCode
        /// </summary>
        /// <value>The le input.</value>
        private Dictionary<ushort,FastList<ICommandData>> ContainedData = new Dictionary<ushort,FastList<ICommandData>>();
        private ushort ContainedTypesCount;


        public Command()
        {
        }


		public Command(ushort inputCode, byte controllerID = byte.MaxValue)
        {
            this.Initialize();
            this.InputCode = inputCode;
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
		public TData[] GetDataArray<TData>() where TData : ICommandData{
			int count = this.GetDataCount<TData> ();
			TData[] array = new TData[count];
			for (int i = 0; i < count; i++) {
				array [i] = GetData<TData> (i);
			}
			return array;
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
            InputCode = reader.ReadUShort();
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
                writer.Write((byte)ControllerID);
                writer.Write((ushort)InputCode);
                writer.Write((ushort)ContainedTypesCount);
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

		public Command Clone()
		{
			Command com = new Command();
			com.ControllerID = this.ControllerID;
			com.InputCode = this.InputCode;
			foreach (KeyValuePair<ushort, FastList<ICommandData>> pair in this.ContainedData) {
				FastList<ICommandData> list = new FastList<ICommandData>();
				pair.Value.CopyTo(list);

				com.ContainedData.Add(pair.Key,list);
			}
			return com;
		}
    }
}