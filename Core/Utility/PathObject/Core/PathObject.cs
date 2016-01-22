using UnityEngine;
using System.Collections;
namespace Lockstep {
[System.Serializable]
public class PathObject {
    [SerializeField]
    protected string _path;
    public string Path {get {return _path;}}

    bool Setted = false;
    private UnityEngine.Object _object;
    public UnityEngine.Object Object {
        get {
            return Setted ? _object : (_object = PathObjectFactory.Load (this));
        }
    }
}
}