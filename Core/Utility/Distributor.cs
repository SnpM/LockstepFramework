using UnityEngine;
using System.Collections.Generic;
using System;
using FastCollections;

namespace Lockstep
{
	public static class Distributor
	{
		public delegate void Del <T> (T message) where T : LSMessage;

		private static Dictionary<string, object> events = new Dictionary<string, object> ();

		public static bool Contains<T> (Del<T> handler) where T : LSMessage
		{
			return (Contains<T> (GetTypeString<T> (), handler));
		}

		public static bool Contains<T> (string messageName, Del<T> handler) where T : LSMessage
		{
			object o;
			if (events.TryGetValue (messageName, out o) == false) return false;
			FastList<Del<T>> dels = o as FastList<Del<T>>;
			if (dels == null) return false;
			if (dels.Contains (handler)) return true;
			return false;

		}

		/// <summary>
		/// Registers a message distribution under the default channel for the type.
		/// </summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool Register<T> () where T: LSMessage
		{
			return (Register <T> (GetTypeString <T> ()) == false);
		}
		/// <summary>
		/// Register a message distribution under messageName.
		/// </summary>
		/// <param name="messageName">Message name.</param>
		/// <typeparam name="T">The return type of the message distribution.</typeparam>
		public static bool Register<T> (string messageName) where T : LSMessage
		{
			if (events.ContainsKey (messageName))
				return false;
			events.Add (messageName, (object)(new FastList<Del<T>> ()));
			return true;
		}
		/// <summary>
		/// Subscribes to the default channel for the type. If a channel is not available, a channel will be created.
		/// </summary>
		/// <param name="handler">Handler.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool Subscribe<T> (Del<T> handler ) where T : LSMessage
		{
			if (Subscribe<T> (GetTypeString <T> (), handler) == false)
			{
				Register<T> ();
			}
			else {
				return true;
			}
			return Subscribe<T> (GetTypeString <T> (), handler);
		}
		/// <summary>
		/// Subscribe to a message distribution identified by messageName.
		/// </summary>
		/// <param name="messageName">Message name.</param>
		/// <param name="handler">Handler.</param>
		/// <typeparam name="T">The return type of the message distribution.</typeparam>
		public static bool Subscribe<T> (string messageName, Del<T> handler) where T : LSMessage
		{
			object o;
			if (events.TryGetValue (messageName, out o) == false)
				return false;
			FastList<Del<T>> dels = o as FastList<Del<T>>;
			if (dels == null)
				return false;
			dels.Add (handler);
			return true;
		}
		/// <summary>
		/// Unsubscribe from the default type channel.
		/// </summary>
		/// <param name="handler">Handler.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool Unsubscribe<T> (Del<T> handler) where T : LSMessage
		{
			return Unsubscribe (GetTypeString<T> (), handler);
		}
		/// <summary>
		/// Unsubscribe from a message distribution identified by messageName.
		/// </summary>
		/// <param name="messageName">Message name.</param>
		/// <param name="handler">Handler.</param>
		/// <typeparam name="T">The return type of the message distribution.</typeparam>
		public static bool Unsubscribe<T> (string messageName, Del<T> handler) where T: LSMessage
		{
			object o;
			if (events.TryGetValue (messageName, out o) == false)
				return false;
			FastList<Del<T>> dels = o as FastList<Del<T>>;
			if (dels == null)
				return false;
			dels.Remove (handler);
			return true;
		}

		/// <summary>
		/// Invoke the default message distribution for T.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool Invoke<T> (T message) where T : LSMessage
		{
			return Invoke<T> (GetTypeString<T> (), message);
		}
		/// <summary>
		/// Invoke the message on the message distribution identified by messageName.
		/// </summary>
		/// <param name="messageName">Message name.</param>
		/// <param name="message">The message distributed.</param>
		/// <typeparam name="T">Message distribution return type.</typeparam>
		public static bool Invoke <T> (string messageName, T message) where T : LSMessage
		{
			object o;
			if (events.TryGetValue (messageName, out o) == false)
				return false;
			FastList<Del<T>> dels = o as FastList<Del<T>>;
			if (dels == null)
				return false;
			for (int i = 0; i < dels.Count; i++) {
				dels [i].Invoke (message);
			}
			return true;
		}

		public static string GetTypeString <T> ()
		{
			return "Ω≈ç√∫~åß∂ƒ©˙∑®ƒµ" + typeof(T).ToString ();
		}
	}
}
public abstract class LSMessage
{

}
