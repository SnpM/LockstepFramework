using UnityEngine;
using System.Collections; using FastCollections;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
namespace Lockstep
{
    public abstract class BaseSerializableValue : Lockstep.ICommandData
    {
        protected static readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        public abstract object ObjectValue {get;protected set;}

        public void Write(Writer writer)
        {
            using (MemoryStream stream = new MemoryStream()) {
                binaryFormatter.Serialize(stream, this.ObjectValue);
                byte[] streamBytes = stream.ToArray();
                ushort size = (ushort)streamBytes.Length;
                writer.Write(size);
                writer.Write(streamBytes);
            }
        }
        public void Read (Reader reader) {
            ushort size = reader.ReadUShort(); 
            using (MemoryStream stream = new MemoryStream(reader.Source,reader.Position,(int)size)){
                object o = binaryFormatter.Deserialize(stream);
                this.ObjectValue = (object[])o;
            }
        }

    }
}