using System;
using UnityEngine;
namespace Lockstep {
//Note: Not Serializable - Unity can't serialize generics
public class Array2D<T>{
	[SerializeField]
	private int _width = 0;
	public int Width {
		get {
			return _width;
		}
		set {
			if (_width != value)
			{
				if (StartVal .IsNotNull () && StartVal.GetType () == typeof (bool))
				{
				bool test = (bool)(object)StartVal;
				}
				int newWidth = value;
				T[] newArray = new T[newWidth * _height];
				for (int i = 0; i < _height; i++)
				{
					int rowIndex = i * newWidth;
					Array.Copy (_innerArray, i * _width, newArray, rowIndex, _width < newWidth ? _width : newWidth);
					if (_width < newWidth)
					{
						int fillStart = rowIndex + _width - 1;
						if (fillStart < 0) fillStart = 0;
						int fillEnd = rowIndex + newWidth;

						for (int j = fillStart; j < fillEnd; j++)
						{
							newArray[j] = StartVal;
						}
					}
				}
				_innerArray = newArray;
				_width = newWidth;
			}
		}
	}
	[SerializeField]
	public int _height = 0;
	public int Height {
		get {
			return _height;
		}
		set {
			if (_height != value) {
				int newHeight = value;
				Array.Resize <T>(ref _innerArray, newHeight * _width);
				for (int i = _height * _width; i < _innerArray.Length; i++)
				{
					_innerArray[i] = StartVal;
				}
				_height = newHeight;
			}
		}
	}
	[SerializeField]
	private T[] _innerArray = new T[0];

	public T[] InnerArray {get {return _innerArray;}}

	private T StartVal;

	public Array2D (int width, int height, T startVal) : this (width,height) {
		StartVal = startVal;
	}
	public Array2D (int width, int height) {
		_innerArray = new T[width * height];
		_width = width;
		_height = height;
	}

	public T this [int w, int h] {
		get {
			int index = GetIndex (w,h);
			return _innerArray[index];
		}
		set {
			int index = GetIndex (w,h);
			_innerArray[index] = value;
		}
	}

	private int GetIndex (int w, int h) {
		return w + h * Width;
	}

}
[Serializable]
public class BoolArray2D : Array2D<bool>{
	public BoolArray2D (int i, int j, bool startVal) : base(i,j, startVal) {

	}
}
}
