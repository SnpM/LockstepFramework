using UnityEngine;
using System.Collections;
using Lockstep;
namespace FastCollections.Tests
{
    public class Array2DTester : MonoBehaviour
    {
        Array2D<Coordinate> testArray;
        int shiftAmount = 1;
        // Use this for initialization
        void Start()
        {
            testArray = new Array2D<Coordinate>(4, 4);
            Refill();
        }

        void Refill()
        {
            for (int i = 0; i < testArray.Width; i++)
            {
                for (int j = 0; j < testArray.Height; j++)
                {
                    testArray[i, j] = new Coordinate(i, j);
                }
            }
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < testArray.Width; i++)
            {
                GUILayout.BeginVertical();
                for (int j = testArray.Height - 1; j >= 0; j--)
                {
                    GUILayout.Label(testArray[i, j].ToString(), GUILayout.Height(50f), GUILayout.Width(50));
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Refill"))
            {
                Refill();
            }

            int.TryParse(GUILayout.TextField(shiftAmount.ToString()), out shiftAmount);
            if (GUILayout.Button("Shift Width"))
            {
                testArray.Shift(shiftAmount, 0);
            }
            if (GUILayout.Button("Shift Height"))
            {
                testArray.Shift(0, shiftAmount);
            }
        }
    }
}