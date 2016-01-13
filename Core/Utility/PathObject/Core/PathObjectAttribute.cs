using UnityEngine;
using System.Collections;
using System;
public class PathObjectAttribute : UnityEngine.PropertyAttribute {
    public Type ObjectType {get; private set;}

    public PathObjectAttribute (Type objectType) {
        this.ObjectType = objectType;
        if (objectType.IsSubclassOf(typeof (UnityEngine.Component)) == false) {
            throw new ArgumentException(string.Format("Type '{0}' is not a UnityEngine.Component.", objectType));
        }
    }
}
