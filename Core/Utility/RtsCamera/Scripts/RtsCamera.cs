using System;
using UnityEngine;

/// <summary>
/// 
/// Implements an RTS-style camera.
/// 
/// For KEYBOARD control, also add "Components/Camera-Control/RtsCamera-Keyboard" (RtsCameraKeys.cs)
/// For MOUSE control, also add "Components/Camera-Control/RtsCamera-Mouse" (RtsCameraMouse.cs)
/// 
/// </summary>
[AddComponentMenu("Camera-Control/RtsCamera")]
[RequireComponent(typeof(RtsCameraMouse))]
[RequireComponent(typeof(RtsCameraKeys))]
public class RtsCamera : MonoBehaviour
{
    #region NOTES
    /*
     * ----------------------------------------------
     * Options for height calculation:
     * 
     * 1) Set GetTerrainHeight function (from another script) to provide x/z height lookup:
     * 
     *     _rtsCamera.GetTerrainHeight = MyGetTerrainHeightFunction;
     *     ...
     *     private float MyGetTerrainHeightFunction(float x, float z)
     *     {
     *         return ...;
     *     }
     * 
     * 2) Set "TerrainHeightViaPhysics = true;"
     * 
     * 3) For a "simple plain" style terrain (flat), set "LookAtHeightOffset" to base terrain height
     * 
     * See demo code for examples.
     * 
     * ----------------------------------------------
     * To "Auto Follow" a target:
     * 
     *     _rtsCamera.Follow(myGameObjectOrTransform)
     * 
     * See demo code for examples.
     * 
     * ----------------------------------------------
     * To be notified when Auto-Follow target changes (or is cleared):
     * 
     *     _rtsCamera.OnBeginFollow = MyBeginFollowCallback;
     *     _rtsCamera.OnEndFollow = MyEndFollowCallback;
     * 
     *     void OnBeginFollow(Transform followTransform) { ... }
     *     void OnEndFollow(Transform followTransform) { ... }
     * 
     * See demo code for examples.
     * 
     * ----------------------------------------------
     * To force the camera to follow "behind" (with optional degree offset):
     * 
     *      _rtsCamera.FollowBehind = true;
     *      _rtsCamera.FollowRotationOffset = 90;   // optional
     *      
     * See demo code for examples.
     * 
     * ----------------------------------------------
     * For target visibility checking via Physics:
     * 
     *      _rtsCamera.TargetVisbilityViaPhysics = true;
     *      _rtsCamera.TargetVisibilityIgnoreLayerMask = {your layer masks that should block camera};
     * 
     * See demo code for examples.
     * 
     */
    #endregion

    public Vector3 LookAt;          // Desired lookat position
    public float Distance;          // Desired distance (units, ie Meters)
    public float Rotation;          // Desired rotation (degrees)
    public float Tilt;              // Desired tilt (degrees)

    public bool Smoothing;          // Should the camera "slide" between positions and targets?
    public float MoveDampening;     // How "smooth" should the camera moves be?  Note: Smaller numbers are smoother
    public float ZoomDampening;     // How "smooth" should the camera zooms be?  Note: Smaller numbers are smoother
    public float RotationDampening; // How "smooth" should the camera rotations be?  Note: Smaller numbers are smoother
    public float TiltDampening;     // How "smooth" should the camera tilts be?  Note: Smaller numbers are smoother

    public Vector3 MinBounds;       // Minimum x,y,z world position for camera target
    public Vector3 MaxBounds;       // Maximum x,y,z world position for camera target

    public float MinDistance;       // Minimum distance of camera from target
    public float MaxDistance;       // Maximum distance of camera from target

    public float MinTilt;           // Minimum tilt (degrees)
    public float MaxTilt;           // Maximum tilt (degrees)

    public Func<float, float, float> GetTerrainHeight;  // Function taking x,z position and returning height (y).

    public bool TerrainHeightViaPhysics;                // If set, camera will automatically raycast against terrain (using TerrainPhysicsLayerMask) to determine height 
    public LayerMask TerrainPhysicsLayerMask;           // Layer mask indicating which layers the camera should ray cast against for height detection

    public float LookAtHeightOffset;                    // Y coordinate of camera target.   Only used if TerrainHeightViaPhysics and GetTerrainHeight are not set.

    public bool TargetVisbilityViaPhysics;              // If set, camera will raycast from target out in order to avoid objects being between target and camera
    public float CameraRadius = 1f;
    public LayerMask TargetVisibilityIgnoreLayerMask;   // Layer mask to ignore when raycasting to determine camera visbility

    public bool FollowBehind;                           // If set, keyboard and mouse rotation will be disabled when Following a target
    public float FollowRotationOffset;                  // Offset (degrees from zero) when forcing follow behind target

    public Action<Transform> OnBeginFollow;             // "Callback" for when automatic target following begins
    public Action<Transform> OnEndFollow;               // "Callback" for when automatic target following ends

    public bool ShowDebugCameraTarget;                  // If set, "small green sphere" will be shown indicating camera target position (even when Following)

    //
    // PRIVATE VARIABLES
    //

    private Vector3 _initialLookAt;
    private float _initialDistance;
    private float _initialRotation;
    private float _initialTilt;

    private float _currDistance;    // actual distance
    private float _currRotation;    // actual rotation
    private float _currTilt;        // actual tilt

    private Vector3 _moveVector;

    private GameObject _target;
    private MeshRenderer _targetRenderer;

    private Transform _followTarget;

    private bool _lastDebugCamera = false;

    //
    // Unity METHODS
    //

    #region UNITY_METHODS

    protected void Reset()
    {
        Smoothing = true;

        LookAtHeightOffset = 0f;
        TerrainHeightViaPhysics = false;
        TerrainPhysicsLayerMask = ~0;   // "Everything" by default!
        GetTerrainHeight = null;

        TargetVisbilityViaPhysics = false;
        CameraRadius = 1f;
        TargetVisibilityIgnoreLayerMask = 0;    // "Nothing" by default!

        LookAt = new Vector3(0, 0, 0);
        MoveDampening = 5f;
        MinBounds = new Vector3(-100, -100, -100);
        MaxBounds = new Vector3(100, 100, 100);

        Distance = 16f;
        MinDistance = 8f;
        MaxDistance = 32f;
        ZoomDampening = 5f;

        Rotation = 0f;
        RotationDampening = 5f;

        Tilt = 45f;
        MinTilt = 30f;
        MaxTilt = 85f;
        TiltDampening = 5f;

        FollowBehind = false;
        FollowRotationOffset = 0;
    }

    protected void Start()
    {
        if (GetComponent<Rigidbody>())
        {
            // don't allow camera to rotate
            GetComponent<Rigidbody>().freezeRotation = true;
        }

        //
        // store initial values so that we can reset them using ResetToInitialValues method
        //
        _initialLookAt = LookAt;
        _initialDistance = Distance;
        _initialRotation = Rotation;
        _initialTilt = Tilt;

        //
        // set our current values to the desired values so that we don't "slide in"
        //
        _currDistance = Distance;
        _currRotation = Rotation;
        _currTilt = Tilt;

        //
        // create a target sphere, hidden by default
        //
        CreateTarget();
    }

    protected void Update()
    {
        //
        // show or hide camera target (mainly for debugging purposes)
        //
        if (_lastDebugCamera != ShowDebugCameraTarget)
        {
            if (_targetRenderer != null)
            {
                _targetRenderer.enabled = ShowDebugCameraTarget;
                _lastDebugCamera = ShowDebugCameraTarget;
            }
        }
    }
	void Awake () {
		instance = this;
	}
	public static void Visualize () {
		if (instance != null)instance.LocalVisualize ();
	}
	public static RtsCamera instance {get; private set;}
	protected void LocalVisualize ()
    {
		if (this == null || this.isActiveAndEnabled == false) return;
        //
        // update desired target position
        //
        if (IsFollowing)
        {
            LookAt = _followTarget.position;
        }
        else
        {
            _moveVector.y = 0;
            LookAt += Quaternion.Euler(0, Rotation, 0) * _moveVector;
            LookAt.y = GetHeightAt(LookAt.x, LookAt.z);
        }
        LookAt.y += LookAtHeightOffset;

        //
        // clamp values
        //
        Tilt = Mathf.Clamp(Tilt, MinTilt, MaxTilt);
        Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);
        LookAt = new Vector3(Mathf.Clamp(LookAt.x, MinBounds.x, MaxBounds.x), Mathf.Clamp(LookAt.y, MinBounds.y, MaxBounds.y), Mathf.Clamp(LookAt.z, MinBounds.z, MaxBounds.z));

        //
        // move from "desired" to "target" values
        //
        if (Smoothing)
        {
            _currRotation = Mathf.LerpAngle(_currRotation, Rotation, Time.deltaTime * RotationDampening);
            _currDistance = Mathf.Lerp(_currDistance, Distance, Time.deltaTime * ZoomDampening);
            _currTilt = Mathf.LerpAngle(_currTilt, Tilt, Time.deltaTime * TiltDampening);
            _target.transform.position = Vector3.Lerp(_target.transform.position, LookAt, Time.deltaTime * MoveDampening);
        }
        else
        {
            _currRotation = Rotation;
            _currDistance = Distance;
            _currTilt = Tilt;
            _target.transform.position = LookAt;
        }

        _moveVector = Vector3.zero;

        //
        // if we're following AND forcing behind, override the rotation to point to target (with offset)
        //
        if (IsFollowing && FollowBehind)
        {
            ForceFollowBehind();
        }

        //
        // optionally, we'll check to make sure the target is visible
        // Note: we only do this when following so that we don't "jar" when moving manually
        //
        if (IsFollowing && TargetVisbilityViaPhysics && DistanceToTargetIsLessThan(1f))
        {
            EnsureTargetIsVisible();
        }

        //
        // recalculate the actual position of the camera based on the above
        //
        UpdateCamera();
    }

    #endregion

    //
    // PUBLIC METHODS
    //

    #region PUBLIC_METHODS

    /// <summary>
    /// Current transform of camera target (NOTE: should not be set directly)
    /// </summary>
    public Transform CameraTarget
    {
        get { return _target.transform; }
    }

    /// <summary>
    /// True if the current camera auto-follow target is set.  Else, false.
    /// </summary>
    public bool IsFollowing
    {
        get { return FollowTarget != null; }
    }

    /// <summary>
    /// Current auto-follow target
    /// </summary>
    public Transform FollowTarget
    {
        get { return _followTarget; }
    }

    /// <summary>
    /// Reset camera to initial (startup) position, distance, rotation, tilt, etc.
    /// </summary>
    /// <param name="includePosition">If true, position will be reset as well.  If false, only distance/rotation/tilt.</param>
    /// <param name="snap">If true, camera will snap instantly to the position.  If false, camera will slide smoothly back to initial values.</param>
    public void ResetToInitialValues(bool includePosition, bool snap = false)
    {
        if (includePosition)
            LookAt = _initialLookAt;

        Distance = _initialDistance;
        Rotation = _initialRotation;
        Tilt = _initialTilt;

        if (snap)
        {
            _currDistance = Distance;
            _currRotation = Rotation;
            _currTilt = Tilt;
            _target.transform.position = LookAt;
        }
    }

    /// <summary>
    /// Manually set target position (snap or slide).
    /// </summary>
    /// <param name="toPosition">Vector3 position</param>
    /// <param name="snap">If true, camera will "snap" to the position, else will "slide"</param>
    public void JumpTo(Vector3 toPosition, bool snap = false)
    {
        EndFollow();

        LookAt = toPosition;

        if (snap)
        {
            _target.transform.position = toPosition;
        }
    }

    /// <summary>
    /// Manually set target position (snap or slide).
    /// </summary>
    /// <param name="toTransform">Transform to which the camera target will be moved</param>
    /// <param name="snap">If true, camera will "snap" to the position, else will "slide"</param>
    public void JumpTo(Transform toTransform, bool snap = false)
    {
        JumpTo(toTransform.position, snap);
    }

    /// <summary>
    /// Manually set target position (snap or slide).
    /// </summary>
    /// <param name="toGameObject">GameObject to which the camera target will be moved</param>
    /// <param name="snap">If true, camera will "snap" to the position, else will "slide"</param>
    public void JumpTo(GameObject toGameObject, bool snap = false)
    {
        JumpTo(toGameObject.transform.position, snap);
    }

    /// <summary>
    /// Set current auto-follow target (snap or slide).
    /// </summary>
    /// <param name="followTarget">Transform which the camera should follow</param>
    /// <param name="snap">If true, camera will "snap" to the position, else will "slide"</param>
    public void Follow(Transform followTarget, bool snap = false)
    {
        if (_followTarget != null)
        {
            if (OnEndFollow != null)
            {
                OnEndFollow(_followTarget);
            }
        }

        _followTarget = followTarget;

        if (_followTarget != null)
        {
            if (snap)
            {
                LookAt = _followTarget.position;
            }

            if (OnBeginFollow != null)
            {
                OnBeginFollow(_followTarget);
            }
        }
    }

    /// <summary>
    /// Set current auto-follow target (snap or slide).
    /// </summary>
    /// <param name="followTarget">GameObject which the camera should follow</param>
    /// <param name="snap">If true, camera will "snap" to the position, else will "slide"</param>
    public void Follow(GameObject followTarget, bool snap = false)
    {
        Follow(followTarget.transform);
    }

    /// <summary>
    /// Break auto-follow.   Camera will now be manually controlled by player input.
    /// </summary>
    public void EndFollow()
    {
        Follow((Transform)null, false);
    }

    /// <summary>
    /// Adds movement to the camera (world coordinates).
    /// </summary>
    /// <param name="dx">World coordinate X distance to move</param>
    /// <param name="dy">World coordinate Y distance to move</param>
    /// <param name="dz">World coordinate Z distance to move</param>
    public void AddToPosition(float dx, float dy, float dz)
    {
        _moveVector += new Vector3(dx, dy, dz);
    }

    #endregion

    //
    // PRIVATE METHODS
    //

    #region PRIVATE_METHODS

    /// <summary>
    /// If "GetTerrainHeight" function set, will call to obtain desired camera height (y position).
    /// Else, if TerrainHeightViaPhysics is true, will use Physics.RayCast to determine camera height.
    /// Else, will assume flat terrain and will return "0" (which will later be offset by LookAtHeightOffset)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private float GetHeightAt(float x, float z)
    {
        //
        // priority 1:  use supplied function to get height at point
        //
        if (GetTerrainHeight != null)
        {
            return GetTerrainHeight(x, z);
        }

        //
        // priority 2:  use physics ray casting to get height at point
        //
        if (TerrainHeightViaPhysics)
        {
            var y = MaxBounds.y;
            var maxDist = MaxBounds.y - MinBounds.y + 1f;

            RaycastHit hitInfo;
            if (Physics.Raycast(new Vector3(x, y, z), new Vector3(0, -1, 0), out hitInfo, maxDist, TerrainPhysicsLayerMask))
            {
                return hitInfo.point.y;
            }
            return 0;   // no hit!
        }

        //
        // assume flat terrain
        //
        return 0;
    }

    /// <summary>
    /// Update the camera position and rotation based on calculated values
    /// </summary>
    private void UpdateCamera()
    {
        var rotation = Quaternion.Euler(_currTilt, _currRotation, 0);
        var v = new Vector3(0.0f, 0.0f, -_currDistance);
        var position = rotation * v + _target.transform.position;

        if (GetComponent<Camera>().orthographic)
        {
            GetComponent<Camera>().orthographicSize = _currDistance;
        }

        // check that camera is not below terrain

        var y = GetHeightAt(position.x, position.z) + 1;
        if (y > position.y)
            position.y = y;

        // update position and rotation of camera

        transform.rotation = rotation;
        transform.position = position;
    }

    /// <summary>
    /// Creates the camera's target, initially not visible.
    /// </summary>
    private void CreateTarget()
    {
        _target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _target.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        _target.GetComponent<Renderer>().material.color = Color.green;

        var targetCollider = _target.GetComponent<Collider>();
        if (targetCollider != null)
        {
            targetCollider.enabled = false;
        }

        _targetRenderer = _target.GetComponent<MeshRenderer>();
        _targetRenderer.enabled = false;

        _target.name = "CameraTarget";
        _target.transform.position = LookAt;
    }

    private bool DistanceToTargetIsLessThan(float sqrDistance)
    {
        if (!IsFollowing)
            return true;    // our distance is technically zero

        var p1 = _target.transform.position;
        var p2 = _followTarget.position;
        p1.y = p2.y = 0;    // ignore height offset
        var v = p1 - p2;
        var vd = v.sqrMagnitude;    // use sqr for performance

        return vd < sqrDistance;
    }

    private void EnsureTargetIsVisible()
    {
        var direction = (transform.position - _target.transform.position);
        direction.Normalize();

        var distance = Distance;

        RaycastHit hitInfo;

        //if (Physics.Raycast(_target.transform.position, direction, out hitInfo, distance, ~TargetVisibilityIgnoreLayerMask))
        if (Physics.SphereCast(_target.transform.position, CameraRadius, direction, out hitInfo, distance, ~TargetVisibilityIgnoreLayerMask))
        {
            if (hitInfo.transform != _target)   // don't collide with outself!
            {
                _currDistance = hitInfo.distance - 0.1f;
            }
        }
    }

    private void ForceFollowBehind()
    {
        var v = _followTarget.transform.forward * -1;
        var angle = Vector3.Angle(Vector3.forward, v);
        var sign = (Vector3.Dot(v, Vector3.right) > 0.0f) ? 1.0f : -1.0f;
        _currRotation = Rotation = 180f + (sign * angle) + FollowRotationOffset;
    }

    #endregion
}
