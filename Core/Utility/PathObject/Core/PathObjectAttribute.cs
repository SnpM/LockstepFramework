using UnityEngine;
using System.Collections; using FastCollections;
using System;

namespace Lockstep
{
    public class PathObjectAttribute : UnityEngine.PropertyAttribute
    {
        public Type ObjectType { get; private set; }

		public PathObjectAttribute(Type requiredType)
        {
            this.ObjectType = requiredType;
            if (requiredType.IsSubclassOf(typeof(UnityEngine.Object)) == false)
            {
                throw new ArgumentException(string.Format("Type '{0}' is not a UnityEngine.Object.", requiredType));
            }
        }
    }
}