using UnityEngine;
using System.Reflection;
using System;
using Lockstep.Data;

public class FixedNumberAttribute : PropertyAttribute {
	public bool Timescaled = false;
	public FixedNumberAttribute (bool timescaled = false) {Timescaled = timescaled;}
}

public class FixedNumberAngleAttribute : PropertyAttribute {
	public bool Timescaled;
	public double Max;
	public FixedNumberAngleAttribute(bool timescaled = false, double max = -1d) {Timescaled = timescaled;Max = max;}
}

public class FrameCountAttribute : PropertyAttribute {}

public class VisualizeAttribute : PropertyAttribute {
}
public class LocalVisualizeAttribute : PropertyAttribute {}
public class HideInInspectorGUI : PropertyAttribute {}
public class DataCodeAttribute : PropertyAttribute {
    public string TargetDataName {get; private set;}
    public DataCodeAttribute (string targetDataName) {
        this.TargetDataName = targetDataName;
    }

}

