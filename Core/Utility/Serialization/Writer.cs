using System;
using UnityEngine;
using System.Collections; using FastCollections;
using System.Text;
namespace Lockstep
{
	public class Writer
	{
		static int i, length;
        public FastList<byte> Canvas {get; private set;}
		public Writer ()
		{
		}
        public Writer (FastList<byte> canvas) {
            this.Initialize(canvas);
        }
        /// <summary>
        /// For re-useability
        /// </summary>
        /// <param name="canvas">Canvas.</param>
        public void Initialize (FastList<byte> canvas) {
            canvas.FastClear();
            Canvas = canvas;
        }
	
		public void Write (byte value)
		{
			Canvas.Add (value);
		}

		public void Write (byte[] values)
		{
			Canvas.AddRange (values);
		}

		public void Write (short value)
		{
			Canvas.AddRange (BitConverter.GetBytes(value));
		}

		public void Write (ushort value)
		{
			Canvas.AddRange (BitConverter.GetBytes (value));
		}

		public void Write (int value)
		{
			Canvas.AddRange (BitConverter.GetBytes (value));
		}

		public void Write (uint value)
		{
			Canvas.AddRange (BitConverter.GetBytes (value));
		}

		public void Write (long value)
		{
			Canvas.AddRange (BitConverter.GetBytes (value));
		}

		public void Write (ulong value)
		{
			Canvas.AddRange (BitConverter.GetBytes (value));
		}

		public void Write (bool value)
		{
			Canvas.AddRange (BitConverter.GetBytes (value));
		}

        public void Write (string value) {
            byte[] stringBytes = System.Text.Encoding.Unicode.GetBytes (value);
            ushort byteLength = (ushort)stringBytes.Length;
            Canvas.AddRange(BitConverter.GetBytes(byteLength));
            Canvas.AddRange(stringBytes);
        }

        public void WriteByteArray (byte[] byteArray) {
            ushort byteLength = (ushort)byteArray.Length;
            Canvas.AddRange(BitConverter.GetBytes(byteLength));
            Canvas.AddRange(byteArray);
        }

		public void Write (ICommandData data) {
			data.Write (this);
		}

        public void Reset () {
            this.Canvas.FastClear();
        }
	}
}