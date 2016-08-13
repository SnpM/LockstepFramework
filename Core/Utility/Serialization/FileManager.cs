using UnityEngine;
using System.IO;
using System.IO.Compression;
using System;
using System.Runtime.Serialization.Formatters.Binary;
namespace Lockstep {
	public static class FileManager {
		private static readonly string BasePath = Application.persistentDataPath;
		private const string ReplayExtension = ".bcreplay";
		public static void SaveReplay (string fileName, Replay replay)
        {
			File.WriteAllBytes (ReplayPath (fileName), replay.ToByteArray ().Compressed ());
		}
		public static Replay RetrieveReplay (string fileName)
		{
			byte[] bytes = File.ReadAllBytes (ReplayPath (fileName)).Decompressed ();
			return bytes.ToObject<Replay> ();
		}
		public static string StandardPath (string fileName) {
			return BasePath + Path.PathSeparator + fileName;
		}
		public static string ReplayPath (string fileName) {
			return BasePath + Path.PathSeparator + "Replays" + Path.PathSeparator + fileName + ReplayExtension;
		}



		static BinaryFormatter Formatter = new BinaryFormatter();

		#region Extensions
		private static byte[] ToByteArray(this object obj)
		{
			using(MemoryStream ms = new MemoryStream())
			{
				Formatter.Serialize(ms, obj);
				return ms.ToArray();
			}
		}
		private static T ToObject<T> (this byte[] bytes)
		{
			using (MemoryStream ms = new MemoryStream(bytes))
			{
				return (T)Formatter.Deserialize (ms);
			}
		}
		private static byte[] Compressed (this byte[] bytes)
		{
			return Compressor.Compress (bytes);
		}
		private static byte[] Decompressed (this byte[] bytes)
		{
			return Compressor.Decompress (bytes);
		}
		#endregion
	}
}