
using UnityEngine;

/// <summary>
/// Convenience script to update various effect targets (if set on Camera).
/// To add a new target update, see the comments below.
/// </summary>
[AddComponentMenu("Camera-Control/RtsCamera-RtsEffectsUpdater")]
public class RtsEffectsUpdater : MonoBehaviour
{
    private RtsCamera _ohCam;

    // ***************************
    // add private variable here
    // ***************************
    // EXAMPLE: private DepthOfField34 _dof3;

    void Start()
    {
        _ohCam = GetComponent<RtsCamera>();
        if (_ohCam == null)
            this.enabled = false;

        // ***************************
        // get instance of component here
        // ***************************
        // EXAMPLE: _dof3 = GetComponent<DepthOfField34>();
    }

    void Update()
    {
        if (_ohCam == null)
            return;

        // ***************************
        // if the script is present, update target (or whatever is needed)
        // ***************************
        /*
         * EXAMPLE:
         * 
        
        var target = _ohCam.CameraTarget;
	    if(_dof3 != null)
	    {
	        _dof3.objectFocus = target;
	    }
         */
    }
}
