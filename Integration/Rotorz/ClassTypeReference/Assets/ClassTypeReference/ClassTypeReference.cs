// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEngine;

namespace TypeReferences {

	/// <summary>
	/// Reference to a class <see cref="System.Type"/> with support for Unity serialization.
	/// </summary>
	[Serializable]
	public sealed class ClassTypeReference : ISerializationCallbackReceiver {

		public static string GetClassRef(Type type) {
			return type != null
				? type.FullName + ", " + type.Assembly.GetName().Name
				: "";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassTypeReference"/> class.
		/// </summary>
		public ClassTypeReference() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassTypeReference"/> class.
		/// </summary>
		/// <param name="assemblyQualifiedClassName">Assembly qualified class name.</param>
		public ClassTypeReference(string assemblyQualifiedClassName) {
			Type = !string.IsNullOrEmpty(assemblyQualifiedClassName)
				? Type.GetType(assemblyQualifiedClassName)
				: null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassTypeReference"/> class.
		/// </summary>
		/// <param name="type">Class type.</param>
		/// <exception cref="System.ArgumentException">
		/// If <paramref name="type"/> is not a class type.
		/// </exception>
		public ClassTypeReference(Type type) {
			Type = type;
		}

		[SerializeField]
		private string _classRef;

		#region ISerializationCallbackReceiver Members

		void ISerializationCallbackReceiver.OnAfterDeserialize() {
			if (!string.IsNullOrEmpty(_classRef)) {
				_type = System.Type.GetType(_classRef);

				if (_type == null)
					Debug.LogWarning(string.Format("'{0}' was referenced but class type was not found.", _classRef));
			}
			else {
				_type = null;
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize() {
		}

		#endregion

		private Type _type;

		/// <summary>
		/// Gets or sets type of class reference.
		/// </summary>
		/// <exception cref="System.ArgumentException">
		/// If <paramref name="value"/> is not a class type.
		/// </exception>
		public Type Type {
			get { return _type; }
			set {
				if (value != null && !value.IsClass)
					throw new ArgumentException(string.Format("'{0}' is not a class type.", value.FullName), "value");

				_type = value;
				_classRef = GetClassRef(value);
			}
		}

		public static implicit operator string(ClassTypeReference typeReference) {
			return typeReference._classRef;
		}

		public static implicit operator Type(ClassTypeReference typeReference) {
			return typeReference.Type;
		}

		public static implicit operator ClassTypeReference(Type type) {
			return new ClassTypeReference(type);
		}

		public override string ToString() {
			return Type != null ? Type.FullName : "(None)";
		}

	}

}
