using UnityEngine;
using System.Collections;

public abstract class LSManager {
    public void Setup () {
        this.OnSetup ();
    }
    protected virtual void OnSetup () {}
    public void Initialize () {
        this.OnInitialize ();
    }
    protected virtual void OnInitialize () {}
    public void Simulate () {
        this.OnSimulate ();
    }
    protected virtual void OnSimulate () {}
    public void Visualize () {
        this.OnVisualize();
    }
    protected virtual void OnVisualize () {}
    public void Deactivate () {
        this.OnDeactivate ();
    }
    protected virtual void OnDeactivate () {}
}
