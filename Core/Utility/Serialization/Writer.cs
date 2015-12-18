using System;
using UnityEngine;
using System.Collections;
using System.Text;
namespace Lockstep
{
	public class Writer
	{
		static int i, length;
		public FastList<byte> canvas;
		public Writer (FastList<byte> Canvas)
		{
			canvas = Canvas;
		}
	
		public void Write (byte value)
		{
			canvas.Add (value);
		}

		public void Write (byte[] values)
		{
			canvas.AddRange (values);
		}

		public void Write (short value)
		{
			canvas.AddRange (BitConverter.GetBytes(value));
		}

		public void Write (ushort value)
		{
			canvas.AddRange (BitConverter.GetBytes (value));
		}

		public void Write (int value)
		{
			canvas.AddRange (BitConverter.GetBytes (value));
		}

		public void Write (uint value)
		{
			canvas.AddRange (BitConverter.GetBytes (value));
		}

		public void Write (long value)
		{
			canvas.AddRange (BitConverter.GetBytes (value));
		}

		public void Write (ulong value)
		{
			canvas.AddRange (BitConverter.GetBytes (value));
		}

		public void Write (bool value)
		{
			canvas.AddRange (BitConverter.GetBytes (value));
		}

        public void Write (string value) {
            byte[] stringBytes = System.Text.Encoding.Unicode.GetBytes (value);
            ushort byteLength = (ushort)stringBytes.Length;
            canvas.AddRange(BitConverter.GetBytes(byteLength));
            canvas.AddRange(stringBytes);
        }

	}
}