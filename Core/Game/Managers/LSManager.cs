using UnityEngine;
using System.Collections;

public abstract class LSManager {
    public void Setup () {

    }
    protected virtual void OnSetup () {}
    public void Initialize () {

    }
    protected virtual void OnInitialize () {}
    public void Simulate () {

    }
    protected virtual void OnSimulate () {}
    public void Visualize () {

    }
    protected virtual void OnVisualize () {}
    public void Deactivate () {

    }
    protected virtual void OnDeactivate () {}
}
