using System;
using UnityEngine;
using System.Collections;

namespace Lockstep
{
	public class Writer
	{
		public FastList<byte> canvas;
		public Writer (FastList<byte> Canvas)
		{
			canvas = Canvas;
		}
	
		public void Write (byte value)
		{
			canvas.Add (value);
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

		public void Write (bool value)
		{
			canvas.AddRange (BitConverter.GetBytes (value));
		}
	}
}