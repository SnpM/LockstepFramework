using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
	public static class LSUtility
	{
		public static readonly byte[] EmptyBytes = new byte[0];
		public static readonly FastList<LSAgent> bufferAgents = new FastList<LSAgent>();
		public static readonly FastList<Health> bufferHealths = new FastList<Health>();
		public static readonly FastList<byte> bufferBytes = new FastList<byte>();
        public static readonly FastList<int> bufferInts = new FastList<int> ();

		const uint Y = 842502087, Z = 3579807591, W = 273326509;
		public static uint Seed = 1;
		private static uint y = Y, z = Z, w;

		public static void Initialize (uint seed)
		{
			Seed = seed;
			y = Y;
			z = Z;
			w = W;
		}

		/*public static uint GetRandom (uint Count)
		{
			Debug.Log ("a");
			uint t = (Seed ^ (Seed << 11));
			Seed = y;
			y = z;
			z = w;
			return ((0xFFFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))) % Count);
		}*/

		public static int GetRandom (int Count)
		{
			uint t = (Seed ^ (Seed << 11));
			Seed = y;
			y = z;
			z = w;
			return (int)((0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))) % Count);
		}

		public static int PeekRandom (int Count)
		{
			uint cSeed = Seed;
			uint cY = y;
			uint cZ = z;
			uint cW = w;
			int ran = GetRandom (Count);
			Seed = cSeed;
			y = cY;
			z = cZ;
			w = cW;
			return ran;
		}

		public static GameObject ResourceInstantiate (string path)
		{
			return GameObject.Instantiate (Resources.Load<GameObject> (path));
		}
		public static GameObject ResourceLoadGO (string path) {
			return Resources.Load<GameObject> (path);
		}
        public static void LoadLevel (string levelName) {
            LockstepManager.Deactivate ();
            Application.LoadLevel (levelName);
        }

		#region BitMask Manipulation
		//ulong mask
		public static void SetBitTrue (ref ulong mask, int bitIndex) 
		{
			mask |= (ulong)1 << bitIndex;
		}

		public static void SetBitFalse (ref ulong mask, int bitIndex)
		{
			mask &= ~((ulong)1 << bitIndex);
		}

		public static bool GetBitTrue (this ulong mask, int bitIndex)
		{
			return (mask & ((ulong)1 << bitIndex)) != 0;
		}

		public static bool GetBitFalse (this ulong mask, int bitIndex)
		{
			return (mask & ((ulong)1 << bitIndex)) == 0;
		}

		//uint mask
		public static void SetBitTrue (ref uint mask, int bitIndex) 
		{
			mask |= (uint)1 << bitIndex;
		}
		
		public static void SetBitFalse (ref uint mask, int bitIndex)
		{
			mask &= ~((uint)1 << bitIndex);
		}

		public static bool GetBitTrue (this uint mask, int bitIndex)
		{
			return (mask & ((uint)1 << bitIndex)) != 0;
		}
		
		public static bool GetBitFalse (this uint mask, int bitIndex)
		{
			return (mask & ((uint)1 << bitIndex)) == 0;
		}

		#endregion
		static FastList<Component> componentBuffer = new FastList<Component>();

		public static T GetComponentInChildrenOrderered<T> (this MonoBehaviour mb) where T : Component
		{
			componentBuffer.Clear ();
			T temp = null;
			foreach (Transform tf in mb.transform) {
				temp = tf.GetComponent<T> ();
				if (temp .IsNotNull ()) return temp;
			}
			return temp;
		}
		public static T[] GetComponentsInChildrenOrderered<T> (this MonoBehaviour mb) where T : Component
		{
			componentBuffer.Clear ();
			RecursiveComponentsSearch<T> (mb.transform);
			T[] ret = new T[componentBuffer.Count];
			for (int i = 0; i < componentBuffer.Count; i++) {
				ret[i] = (T)componentBuffer[i];
			}
			return ret;
		}
		private static void RecursiveComponentsSearch<T> (Transform transform) where T : Component{
			foreach (Transform tf in transform) {
				T temp = tf.GetComponent<T> ();
				if (temp .IsNotNull ()) componentBuffer.Add ((Component)temp);
					RecursiveComponentsSearch<T> (tf);
			}
		}
		public static void Clear (this Array array)
		{
			System.Array.Clear (array, 0, array.Length);
		}
		public static void SetActiveIfNot (this GameObject go, bool activeState)
		{
			if (go.activeSelf != activeState)
				go.SetActive (activeState);
		}
		public static bool RefEquals (this System.Object obj, object other) {
			return System.Object.ReferenceEquals (obj, other);
		}
		public static bool IsNull (this System.Object obj){
			return obj.RefEquals (null);
		}
		public static bool IsNotNull (this System.Object obj) {
			return obj.RefEquals (null) == false;
		}

		public static float V3SqrDistance (this Vector3 vec, Vector3 other) {
			float temp1 = vec.x - other.x;
			temp1 *= temp1;
			float temp2 = vec.y * other.y;
			temp2 *= temp2;
			return temp1 + temp2;
		}
		static System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
		public static string CharString (this Enum val) {
			return "" + (char)Convert.ToUInt16 (val);
		}

		public static UnityEngine.Coroutine WaitRealTime (float wait) {
			return CoroutineManager.StartUnityCoroutine(waitRealTime (wait));
		}
		private static IEnumerator waitRealTime (float wait) {
			float accumulator = 0f;
			while (accumulator < wait) {
				accumulator += Time.unscaledDeltaTime;
				yield return null;
			}
		}
		public static void Shift (this Array array, int min, int max, int shiftAmount) {
			if (shiftAmount == 0) return;
				Array.Copy (array, min, array, min + shiftAmount, max - min);

		}
		public static string PrintRange (this Array array, int min, int max) {
			string s = "";
			for (int i = min; i < max; i++) {
				s += array.GetValue(i) + ", ";
			}
			return s;
		}
        public static string PrintAll (this IEnumerable collection) {
            string s = "";
            foreach (object obj in collection) {
                s += obj.ToString ()+ ", ";
            }
            return s;
        }
        public static bool InsensitiveContains (this string source, string other) {
            return source.IndexOf(other, StringComparison.OrdinalIgnoreCase) >= 0;
        }
	}
    public enum AbilityCode {}
}
namespace Lockstep.Integration {

}