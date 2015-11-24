using UnityEngine;

/// <summary>
/// Encapsulates mouse movement for RtsCamera.
/// </summary>
[AddComponentMenu("Camera-Control/RtsCamera-Mouse")]
public class RtsCameraMouse : MonoBehaviour
{
    public KeyCode MouseOrbitButton;

    public bool AllowScreenEdgeMove;
    public bool ScreenEdgeMoveBreaksFollow;
    public int ScreenEdgeBorderWidth;
    public float MoveSpeed;

    public bool AllowPan;
    public bool PanBreaksFollow;
    public float PanSpeed;

    public bool AllowRotate;
    public float RotateSpeed;

    public bool AllowTilt;
    public float TiltSpeed;

    public bool AllowZoom;
    public float ZoomSpeed;

    public string RotateInputAxis = "Mouse X";
    public string TiltInputAxis = "Mouse Y";
    public string ZoomInputAxis = "Mouse ScrollWheel";
    public KeyCode PanKey1 = KeyCode.LeftShift;
    public KeyCode PanKey2 = KeyCode.RightShift;

    //

    private RtsCamera _rtsCamera;

    //

    protected void Reset()
    {
        MouseOrbitButton = KeyCode.Mouse2;    // middle mouse by default (probably should not use right mouse since it doesn't work well in browsers)

        AllowScreenEdgeMove = true;
        ScreenEdgeMoveBreaksFollow = true;
        ScreenEdgeBorderWidth = 4;
        MoveSpeed = 30f;

        AllowPan = true;
        PanBreaksFollow = true;
        PanSpeed = 50f;
        PanKey1 = KeyCode.LeftShift;
        PanKey2 = KeyCode.RightShift;

        AllowRotate = true;
        RotateSpeed = 360f;

        AllowTilt = true;
        TiltSpeed = 200f;

        AllowZoom = true;
        ZoomSpeed = 500f;

        RotateInputAxis = "Mouse X";
        TiltInputAxis = "Mouse Y";
        ZoomInputAxis = "Mouse ScrollWheel";
    }

    protected void Start()
    {
        _rtsCamera = gameObject.GetComponent<RtsCamera>();
    }

    protected void Update()
    {
        if (_rtsCamera == null)
            return; // no camera, bail!

        if (AllowZoom)
        {
            var scroll = Input.GetAxisRaw(ZoomInputAxis);
            _rtsCamera.Distance -= scroll * ZoomSpeed * Time.deltaTime;
        }

        if (Input.GetKey(MouseOrbitButton))
        {
            if (AllowPan && (Input.GetKey(PanKey1) || Input.GetKey(PanKey2)))
            {
                // pan
                var panX = -1 * Input.GetAxisRaw("Mouse X") * PanSpeed * Time.deltaTime;
                var panZ = -1 * Input.GetAxisRaw("Mouse Y") * PanSpeed * Time.deltaTime;

                _rtsCamera.AddToPosition(panX, 0, panZ);

                if (PanBreaksFollow && (Mathf.Abs(panX) > 0.001f || Mathf.Abs(panZ) > 0.001f))
                {
                    _rtsCamera.EndFollow();
                }
            }
            else
            {
                // orbit

                if (AllowTilt)
                {
                    var tilt = Input.GetAxisRaw(TiltInputAxis);
                    _rtsCamera.Tilt -= tilt * TiltSpeed * Time.deltaTime;
                }

                if (AllowRotate)
                {
                    var rot = Input.GetAxisRaw(RotateInputAxis);
                    _rtsCamera.Rotation += rot * RotateSpeed * Time.deltaTime;
                }
            }
        }

        if (AllowScreenEdgeMove && (!_rtsCamera.IsFollowing || ScreenEdgeMoveBreaksFollow))
        {
            var hasMovement = false;

            if (Input.mousePosition.y > (Screen.height - ScreenEdgeBorderWidth))
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(0, 0, MoveSpeed * Time.deltaTime);
            }
            else if (Input.mousePosition.y < ScreenEdgeBorderWidth)
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(0, 0, -1 * MoveSpeed * Time.deltaTime);
            }

            if (Input.mousePosition.x > (Screen.width - ScreenEdgeBorderWidth))
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(MoveSpeed * Time.deltaTime, 0, 0);
            }
            else if (Input.mousePosition.x < ScreenEdgeBorderWidth)
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(-1 * MoveSpeed * Time.deltaTime, 0, 0);
            }

            if (hasMovement && _rtsCamera.IsFollowing && ScreenEdgeMoveBreaksFollow)
            {
                _rtsCamera.EndFollow();
            }
        }
    }
}
