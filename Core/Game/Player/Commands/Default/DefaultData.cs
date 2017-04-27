using UnityEngine;
using System.Collections; using FastCollections;
using System;
using System.Text;

namespace Lockstep
{
    public class DefaultData : ICommandData
    {
        public DefaultData()
        {
        }

        public DefaultData(DataType dataType, object value)
        {
            Value = value;
            this.DataType = dataType;
        }

        public DefaultData(object value)
        {
            Value = value;
            this.DataType = GetDataType(value.GetType());
        }

        public object Value { get; protected set; }

        public DataType DataType { get; protected set; }

        public bool Is(DataType dataType)
        {
            return this.DataType == dataType;
        }

        public DataType GetDataType(Type type)
        {
            if (type == typeof(int))
                return DataType.Int;
            if (type == typeof(uint))
                return DataType.UInt;
            if (type == typeof(ushort))
                return DataType.UShort;
            if (type == typeof(long))
                return DataType.Long;
            if (type == typeof(byte))
                return DataType.Byte;
            if (type == typeof(bool))
                return DataType.Bool;
            if (type == typeof(string))
                return DataType.String;
            if (type == typeof(byte[]))
                return DataType.ByteArray;

            throw new System.Exception(string.Format("Type '{0}' is not a valid DefaultData Type.", type));
        }

        public void Write(Writer writer)
        {
            writer.Write((byte)this.DataType);
            switch (this.DataType)
            {
                case DataType.Int:
                    writer.Write((int)Value);
                    break;
                case DataType.UInt:
                    writer.Write((uint)Value);
                    break;
                case DataType.UShort:
                    writer.Write((ushort)Value);
                    break;
                case DataType.Long:
                    writer.Write((long)Value);
                    break;
                case DataType.Byte:
                    writer.Write((byte)Value);
                    break;
                case DataType.Bool:
                    writer.Write((bool)Value);
                    break;
                case DataType.String:
                    writer.Write((string)Value);
                    break;
                case DataType.ByteArray:
                    writer.WriteByteArray((byte[])Value);
                    break;
            }
        }


        public void Read(Reader reader)
        {
            this.DataType = (DataType)reader.ReadByte();
            switch (this.DataType)
            {
                case DataType.Int:
                    Value = reader.ReadInt();
                    break;
                case DataType.UInt:
                    Value = reader.ReadUInt();
                    break;
                case DataType.UShort:
                    Value = reader.ReadUShort();
                    break;
                case DataType.Long:
                    Value = reader.ReadLong();
                    break;
                case DataType.Byte:
                    Value = reader.ReadByte();
                    break;
                case DataType.Bool:
                    Value = reader.ReadBool();
                    break;
                case DataType.String:
                    Value = reader.ReadString();
                    break;
                case DataType.ByteArray:
                    Value = reader.ReadByteArray();
                    break;
            }
        }
    }

    public enum DataType : byte
    {
        Int,
        UInt,
        UShort,
        Long,
        Byte,
        Bool,
        String,
        ByteArray,
    }
}