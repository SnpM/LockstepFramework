// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEngine;

namespace TypeReferences {

	/// <summary>
	/// Indicates how selectable classes should be collated in drop-down menu.
	/// </summary>
	public enum ClassGrouping {
		/// <summary>
		/// No grouping, just show type names in a list; for instance, "Some.Nested.Namespace.SpecialClass".
		/// </summary>
		None,
		/// <summary>
		/// Group classes by namespace and show foldout menus for nested namespaces; for
		/// instance, "Some > Nested > Namespace > SpecialClass".
		/// </summary>
		ByNamespace,
		/// <summary>
		/// Group classes by namespace; for instance, "Some.Nested.Namespace > SpecialClass".
		/// </summary>
		ByNamespaceFlat,
		/// <summary>
		/// Group classes in the same way as Unity does for its component menu. This
		/// grouping method must only be used for <see cref="MonoBehaviour"/> types.
		/// </summary>
		ByAddComponentMenu,
	}

	/// <summary>
	/// Base class for class selection constraints that can be applied when selecting
	/// a <see cref="ClassTypeReference"/> with the Unity inspector.
	/// </summary>
	public abstract class ClassTypeConstraintAttribute : PropertyAttribute {

		private ClassGrouping _grouping = ClassGrouping.ByNamespaceFlat;
		private bool _allowAbstract = false;

		/// <summary>
		/// Gets or sets grouping of selectable classes. Defaults to <see cref="ClassGrouping.ByNamespaceFlat"/>
		/// unless explicitly specified.
		/// </summary>
		public ClassGrouping Grouping {
			get { return _grouping; }
			set { _grouping = value; }
		}

		/// <summary>
		/// Gets or sets whether abstract classes can be selected from drop-down.
		/// Defaults to a value of <c>false</c> unless explicitly specified.
		/// </summary>
		public bool AllowAbstract {
			get { return _allowAbstract; }
			set { _allowAbstract = value; }
		}

		/// <summary>
		/// Determines whether the specified <see cref="Type"/> satisfies filter constraint.
		/// </summary>
		/// <param name="type">Type to test.</param>
		/// <returns>
		/// A <see cref="bool"/> value indicating if the type specified by <paramref name="type"/>
		/// satisfies this constraint and should thus be selectable.
		/// </returns>
		public virtual bool IsConstraintSatisfied(Type type) {
			return AllowAbstract || !type.IsAbstract;
		}

	}

	/// <summary>
	/// Constraint that allows selection of classes that extend a specific class when
	/// selecting a <see cref="ClassTypeReference"/> with the Unity inspector.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ClassExtendsAttribute : ClassTypeConstraintAttribute {

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassExtendsAttribute"/> class.
		/// </summary>
		public ClassExtendsAttribute() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassExtendsAttribute"/> class.
		/// </summary>
		/// <param name="baseType">Type of class that selectable classes must derive from.</param>
		public ClassExtendsAttribute(Type baseType) {
			BaseType = baseType;
		}

		/// <summary>
		/// Gets the type of class that selectable classes must derive from.
		/// </summary>
		public Type BaseType { get; private set; }

		/// <inheritdoc/>
		public override bool IsConstraintSatisfied(Type type) {
			return base.IsConstraintSatisfied(type)
				&& BaseType.IsAssignableFrom(type) && type != BaseType;
		}

	}

	/// <summary>
	/// Constraint that allows selection of classes that implement a specific interface
	/// when selecting a <see cref="ClassTypeReference"/> with the Unity inspector.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ClassImplementsAttribute : ClassTypeConstraintAttribute {

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassImplementsAttribute"/> class.
		/// </summary>
		public ClassImplementsAttribute() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassImplementsAttribute"/> class.
		/// </summary>
		/// <param name="interfaceType">Type of interface that selectable classes must implement.</param>
		public ClassImplementsAttribute(Type interfaceType) {
			InterfaceType = interfaceType;
		}

		/// <summary>
		/// Gets the type of interface that selectable classes must implement.
		/// </summary>
		public Type InterfaceType { get; private set; }

		/// <inheritdoc/>
		public override bool IsConstraintSatisfied(Type type) {
			if (base.IsConstraintSatisfied(type)) {
				foreach (var interfaceType in type.GetInterfaces())
					if (interfaceType == InterfaceType)
						return true;
			}
			return false;
		}

	}

}
