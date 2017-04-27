using UnityEngine;
using System.Collections; using FastCollections;

public abstract class GUIManager {
    public abstract bool CanInteract {get;}
    public abstract void InformationDown ();
    public abstract bool CameraChanged {get;}
    public abstract Camera MainCam {get;}
    public abstract bool CanHUD {get;}
    public abstract float CameraScale {get;}
    public abstract bool ShowHealthWhenFull {get;}
}
