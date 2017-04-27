using UnityEngine;
using System.Collections; using FastCollections;

public class ExampleGUIManager : GUIManager {
    public override bool CameraChanged {
        get {
            return Camera.main.transform.hasChanged;
        }
    }
    public override float CameraScale {
        get {
            return Camera.main.transform.position.y;
        }
    }
    public override bool CanHUD {
        get {
            return false;
        }
    }
    public override bool CanInteract {
        get {
            return true;
        }
    }
    public override void InformationDown () {

    }
    public override Camera MainCam {
        get {
            return Camera.main;
        }
    }
    public override bool ShowHealthWhenFull {
        get {
            return false;
        }
    }
}
