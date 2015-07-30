using UnityEngine;
using System.Collections;

public struct Coordinate {
	public int x;
	public int y;
	public Coordinate (int X, int Y)
	{
		x = X;
		y = Y;
	}
	public override string ToString ()
	{
		return "(" + x.ToString () + ", " + y.ToString() + ")";
	}
}
