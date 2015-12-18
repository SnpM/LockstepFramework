using UnityEngine;
using System.Collections;
using System;
using System.Text;

namespace Lockstep
{
    public class Reader
    {
        public int count;
        public byte[] source;

        public void Initialize(byte[] Source, int StartIndex)
        {
            source = Source;
            count = StartIndex;
        }

        public byte ReadByte()
        {
            byte ret = source [count];
            count += 1;
            return ret;
        }

        public byte[] ReadBytes(int ReadLength)
        {
            byte[] RetBytes = new byte[ReadLength];
            Array.Copy(source, count, RetBytes, 0, ReadLength);
            count += ReadLength;
            return RetBytes;
        }

        public bool ReadBool()
        {
            bool ret = BitConverter.ToBoolean(source, count);
            count += 1;
            return ret;
        }

        public short ReadShort()
        {
            short ret = BitConverter.ToInt16(source, count);
            count += 2;
            return ret;
        }

        public ushort ReadUShort()
        {
            ushort ret = BitConverter.ToUInt16(source, count);
            count += 2;
            return ret;
        }

        public int ReadInt()
        {
            int ret = BitConverter.ToInt32(source, count);
            count += 4;
            return ret;
        }

        public uint ReadUInt()
        {
            uint ret = BitConverter.ToUInt32(source, count);
            count += 4;
            return ret;
        }

        public long ReadLong()
        {
            long ret = BitConverter.ToInt64(source, count);
            count += 8;
            return ret;
        }

        public ulong ReadULong()
        {
            ulong ret = BitConverter.ToUInt64(source, count);
            count += 8;
            return ret;
        }

        public string ReadString()
        {
            ushort byteLength = BitConverter.ToUInt16(source, count);
            count += 2;
            string ret = Encoding.Unicode.GetString(source, count, (int)byteLength);
            count += byteLength;
            return ret;
        }

    }
}