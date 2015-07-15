using System;
using System.IO;
using UnityEngine;
using System.Collections;

namespace Lockstep
{

	public class Command
	{
		public KeyCode KeyInput;

		public bool HasPosition;
		public Vector2d _position;

		public Vector2d Position {
			get { return _position;}
			set {
				_position = value;
				HasPosition = true;
			}
		}

		public bool HasTarget;
		public ushort _target;

		public ushort Target {

			get { return _target;}
			set {
				_target = value;
				HasTarget = true;
			}
		}

		public bool HasFlag;
		public bool _flag;
		public bool Flag {
			get { return _flag;}
			set {
				_flag = value;
				HasFlag = true;
			}
		}

		public bool HasCoord;
		public Coordinate _coord;

		public Coordinate Coord {
			get { return _coord;}
			set {
				_coord = value;
				HasCoord = true;
			}
		}

		public bool HasCount;
		public int _count;
		public int Count {
			get { return _count;}
			set {
				_count = value;
				HasCount = true;
			}
		}

		/*

		Positions in bitmask:
		Position: 0
		Target: 1
		Flag: 2
		Coord: 3

		*/

		static Reader reader = new Reader ();

		/// <summary>
		/// Reconstructs this command from a serialized command and returns the size of the command.
		/// </summary>
		public int Reconstruct (byte[] Source, int StartIndex)
		{
			reader.Initialize (Source,StartIndex);

			KeyInput = (KeyCode)reader.ReadInt ();

			ValuesMask = reader.ReadUInt ();

			HasPosition = GetMaskBool(ValuesMask,DataType.Position);
			HasTarget = GetMaskBool (ValuesMask,DataType.Target);
			HasFlag = GetMaskBool (ValuesMask,DataType.Flag);
			HasCoord = GetMaskBool (ValuesMask,DataType.Coord);
			HasCount = GetMaskBool (ValuesMask,DataType.Count);

			if (HasPosition)
			{
				_position.x = reader.ReadLong ();
				_position.y = reader.ReadLong ();
			}
			if (HasTarget)
			{
				_target = reader.ReadUShort( );
			}
			if (HasFlag)
			{
				_flag = reader.ReadBool ();
			}
			if (HasCoord)
			{
				_coord.x = reader.ReadInt ();
				_coord.y = reader.ReadInt ();
			}
			if (HasCount)
			{
				_count = reader.ReadInt ();
			}

			return reader.count - StartIndex;;
		}

		static uint ValuesMask;
		static FastList<byte> serializeList = new FastList<byte> ();
		static Writer writer = new Writer (serializeList);

		public byte[] Serialized {
			get {
				serializeList.FastClear ();

				//Essential Information
				writer.Write ((int)KeyInput);

				//Header 
				ValuesMask = 
					(HasPosition ? (uint)DataType.Position : (uint)0) |
					(HasTarget ? (uint)DataType.Target : (uint)0) |
					(HasFlag ? (uint)DataType.Flag : (uint)0) |
					(HasCoord ? (uint)DataType.Coord : (uint)0) |
					(HasCount ? (uint)DataType.Count : (uint)0)
					;
				writer.Write (ValuesMask);

				//Position
				if (HasPosition) {
					writer.Write (_position.x);
					writer.Write (_position.y);
				}

				//Target
				if (HasTarget) {
					writer.Write (_target);
				}

				//Flag
				if (HasFlag) {
					writer.Write (_flag);
				}

				//Coord
				if (HasCoord) {
					writer.Write (_coord.x);
					writer.Write (_coord.y);
				}

				return serializeList.ToArray ();
			}
		}

		public static bool GetMaskBool (uint mask, DataType dataType)
		{
			return (mask & (uint)dataType) == (uint)dataType;
		}
	}
	public enum DataType : uint{
		Position = 1 << 0,
		Flag = 1 << 1,
		Target = 1 << 2,
		Coord = 1 << 3,
		Count = 1 << 4
	}

}