#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace Lockstep
{
	public class ClearanceValueGenerator : MonoBehaviour
	{
		void Start()
		{
			//Generates a cache of clearance values and tosses it into the Assets folder
			string path = Application.dataPath + "/ClearanceValues.cs";
			if (!File.Exists(path))
			{
				File.Delete(path);
			}
			StringBuilder builder = new StringBuilder();

			//Let's write the script!
			builder.Append(
			@"using System;
			namespace Lockstep {
				public static class ClearanceValues {
					public static Coordinate[][] ClearanceSearchNodes = new Coordinate[][] {
			"
			);
			int maxClearance = 7;
			//clearance can't be even
			for (int i = 1; i <= maxClearance; i += 2)
			{
				var coords = GetClearanceNodes(i);
				builder.AppendLine("//size " + i);
				builder.AppendLine("new Coordinate[] {");
				foreach (var coord in coords)
				{
					builder.Append(
						"new Coordinate (" + coord.x + ", " + coord.y + "),"
					);
				}
				builder.AppendLine("},");
			}

			builder.Append(

					@"};
				}
			}"
			);
			File.WriteAllText(path, builder.ToString());
		}

		Coordinate[] GetClearanceNodes(int size)
		{
			List<Coordinate> coords = new List<Coordinate>();
			int halfSize = size / 2;
			for (int x = -halfSize; x <= halfSize; x++)
			{
				for (int y = -halfSize; y <= halfSize; y++)
				{
					coords.Add(new Coordinate(x, y));
				}
			}
			return coords.ToArray();
		}
	}
}
#endif