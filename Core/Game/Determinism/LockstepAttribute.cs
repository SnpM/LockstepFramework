using UnityEngine;
using System.Collections;
using System;
[AttributeUsage (AttributeTargets.Property)]
public sealed class LockstepAttribute : Attribute {
    public LockstepAttribute () {
    }
}
