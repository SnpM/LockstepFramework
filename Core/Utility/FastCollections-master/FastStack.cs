using System.Collections;
using System;
namespace FastCollections
{
	public class FastStack<T>
	{
		private const int DefaultCapacity = 8;
		public T[] innerArray;
		public int Count = 0;
		public int Capacity;

		public FastStack (int StartCapacity)
		{
			Capacity = StartCapacity;
			Initialize ();
		}
		public FastStack ()
		{
			Capacity = DefaultCapacity;
			Initialize ();
		}
		
		private void Initialize ()
		{
			#if UNITY_EDITOR
			if (Capacity <= 0)
			{
				UnityEngine.Debug.LogError ("Initializing list with capacity less than or equal to zero isn't supported");
			}
			#endif
			innerArray = new T[Capacity];
		}
		
		public void Add (T item)
		{
			EnsureCapacity ();
 			innerArray [Count++] = item;
		}
		
		public T Pop ()
		{
			return (innerArray[--Count]);
		}

		public T Peek ()
		{
			return innerArray[Count - 1];
		}
		
		private void EnsureCapacity ()
		{
            EnsureCapacity (Count + 1);
		}
        public void EnsureCapacity (int min) {
            if (Capacity < min) {
                Capacity *= 2;
                if (Capacity < min)
                    Capacity = min;
                T[] newItems = new T[Capacity];
                Array.Copy (innerArray, 0, newItems, 0, Count);
                innerArray = newItems;  
            }
        }
		
		public T this [int index] {
			get {
				return innerArray [index];
			}
			set {
				innerArray [index] = value;
			}
		}

		public void Clear ()
		{
			innerArray = new T[Capacity];
		}

		/// <summary>
		/// Marks elements for overwriting. Note: After using FastClear(), this list will still keep any references to objects it previously had.
		/// </summary>
		public void FastClear ()
		{
			Count = 0;
		}
		
		public override string ToString ()
		{
			if (Count <= 0)
				return base.ToString ();
			string output = string.Empty;
			for (int i = 0; i < Count - 1; i++)
				output += innerArray [i] + ", ";
			
			return base.ToString () + ": " + output + innerArray [Count - 1];
		}


		
	}
}
