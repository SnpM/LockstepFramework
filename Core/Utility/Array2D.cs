using System;
using UnityEngine;

namespace Lockstep
{
	//Note: Not Serializable - Unity can't serialize generics
	/// <summary>
	/// Flattened 2D array wrapper with dynamic resizing.
	/// </summary>
	public class Array2D<T>
	{
		[SerializeField, HideInInspector]
		private int _width = 0;

		public int Width
		{
			get
			{
				return _width;
			}
		}

		[SerializeField, HideInInspector]
		public int _height = 0;

		public int Height
		{
			get
			{
				return _height;
			}
		}

		[SerializeField, HideInInspector]
		private T[] _innerArray = new T[0];

		public T[] InnerArray { get { return _innerArray; } }

		private T _startVal;
		private T StartVal { get { return _startVal; } }

		public Array2D() : this(0, 0)
		{

		}

		public Array2D(int width, int height, T startVal) : this(width, height)
		{
			_startVal = startVal;
		}

		public Array2D(int width, int height)
		{
			this.Initialize(width, height);
		}

		private void Initialize(int width, int height)
		{
			_innerArray = new T[width * height];
			_width = width;
			_height = height;
		}

		public T this[int w, int h]
		{
			get
			{
				int index = GetIndex(w, h);
				try
				{
					return _innerArray[index];
				}
				catch
				{
					throw new System.IndexOutOfRangeException(w + ", " + h);
				}
			}
			set
			{
				try
				{
					int index = GetIndex(w, h);
					_innerArray[index] = value;
				}
				catch
				{
					throw new System.IndexOutOfRangeException(w + ", " + h);
				}
			}
		}

		public bool IsValidIndex(int w, int h)
		{
			return w >= 0 && w < this.Width && h >= 0 && h < this.Height;
		}
		public bool IsValidX(int x)
		{
			return x >= 0 && x < Width;
		}
		public bool IsValidY(int y)
		{
			return y >= 0 && y < Height;
		}

		public static Array2D<T> Clone(T[,] source)
		{
			Array2D<T> array = new Array2D<T>(source.GetLength(0), source.GetLength(1));
			for (int i = 0; i < array.Width; i++)
			{
				for (int j = 0; j < array.Height; j++)
				{
					array[i, j] = source[i, j];
				}
			}
			return array;
		}

		public void LocalClone(T[,] source)
		{
			this.Resize(source.GetLength(0), source.GetLength(1));
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					this[i, j] = source[i, j];
				}
			}
		}

		public void Resize(int newWidth, int newHeight)
		{
			if (_height != newHeight || _width != newWidth)
			{
				//Height changes require remapping
				T[] newArray = new T[newWidth * newHeight];

				int minWidth = _width < newWidth ? _width : newWidth;
				int minHeight = _height < newHeight ? _height : newHeight;
				for (int i = minWidth - 1; i >= 0; i--)
				{
					Array.Copy(_innerArray, i * _height, newArray, i * newHeight, minHeight);
				}

				_innerArray = newArray;

				_height = newHeight;
				_width = newWidth;
			}
		}

		public void Shift(int xShift, int yShift)
		{
			if (yShift != 0)
			{
				int absShift = Math.Abs(yShift);

				int shiftLength = _height - absShift;
				if (shiftLength < 0)
				{
					//If the shift is more than the array's height, clear all elements
					Array.Clear(_innerArray, 0, _innerArray.Length);
				}
				else
				{
					T[] newArray = new T[_innerArray.Length];

					for (int i = _width - 1; i >= 0; i--)
					{
						int index = i * _height;
						int newIndex = index + yShift;
						if (yShift < 0)
						{
							index -= yShift;
							newIndex -= yShift;
						}
						Array.Copy(_innerArray, index, newArray, newIndex, shiftLength);
					}
					_innerArray = newArray;
				}
			}
			if (xShift != 0)
			{

				int absShift = Math.Abs(xShift);
				if (absShift >= _width)
				{
					//If the shift is more than the array's height, clear all elements
					Array.Clear(_innerArray, 0, _innerArray.Length);
				}
				else
				{
					T[] newArray = new T[_innerArray.Length];
					int start = 0;
					int end = _width;
					if (xShift < 0)
						start -= xShift;
					else
						end -= xShift;
					for (int i = start; i < end; i++)
					{
						int index = i * _height;
						int newIndex = (i + xShift) * _height;
						Array.Copy(_innerArray, index, newArray, newIndex, _height);
					}
					_innerArray = newArray;
				}
			}
		}

		public void Clear()
		{
			Array.Clear(_innerArray, 0, _innerArray.Length);
		}

		private int GetIndex(int w, int h)
		{
			return w * this._height + h;
		}

		public T[,] ToArray()
		{
			T[,] array = new T[Width, Height];
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					array[i, j] = this[i, j];
				}
			}
			return array;
		}

	}

	[Serializable]
	public class BoolArray2D : Array2D<bool>
	{
		public BoolArray2D(int i, int j, bool startVal) : base(i, j, startVal)
		{

		}
	}
}
