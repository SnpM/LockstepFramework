using UnityEngine;

public class DemoGui : MonoBehaviour
{
    public GameObject MainCamera;

    private RtsCamera _scCamera;
    private RtsCameraKeys _scKeys;
    private RtsCameraMouse _scMouse;

    private DemoMain _scDemoMain;

    private bool _showOptions = false;

    void Start()
    {
        _scCamera = MainCamera.GetComponent<RtsCamera>();
        _scKeys = MainCamera.GetComponent<RtsCameraKeys>();
        _scMouse = MainCamera.GetComponent<RtsCameraMouse>();

        _scDemoMain = GetComponent<DemoMain>();
    }

    void Update()
    {
    }

    protected void OnGUI()
    {
        if (_scCamera == null)
            return; // no camera, bail!

        var padded = new GUIStyle { margin = new RectOffset(20, 20, 20, 20) };

        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height), padded);

        //
        //

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();

        GuiNotes();

        GUILayout.BeginHorizontal();

        _showOptions = GUILayout.Toggle(_showOptions, "Show Options");

        var isFullscreen = Screen.fullScreen;
        var newFullscreen = GUILayout.Toggle(isFullscreen, "Fullscreen");
        if (isFullscreen != newFullscreen)
        {
            ToggleFullScreen();
        }

        GUILayout.EndHorizontal();

        if (_showOptions)
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GuiKeyboardOptions();
            GuiMouseOptions();
            GUILayout.EndVertical();

            GuiCameraOptions();

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        //
        //

        GUILayout.FlexibleSpace();

        //
        //

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GuiActions();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        //
        //

        GUILayout.EndArea();
    }

    private void ToggleFullScreen()
    {
        if (!Screen.fullScreen)
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        else
            Screen.SetResolution(1024, 768, false);
    }

    private void GuiNotes()
    {
        //GUILayout.BeginArea(new Rect(10, Screen.height - 60, Screen.width, 60));
        GUILayout.BeginVertical("box", GUIStyle.none);

        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperLeft;

        if (_scKeys != null && _scKeys.enabled)
        {
            var msg = string.Format("Keyboard Move: {0}{5} | Rotate: {1} | Tilt: {2} | Zoom: {3} | Reset: {4}",
                _scKeys.AllowMove ? ("W/A/S/D") : "(disabled)",
                _scKeys.AllowRotate ? ("Q/E") : "(disabled)",
                _scKeys.AllowTilt ? ("R/F") : "(disabled)",
                _scKeys.AllowZoom ? ("Z/X") : "(disabled)",
                _scKeys.ResetKey != KeyCode.None ? ("C") : "(disabled)",
                _scKeys.AllowFastMove ? " (Shift=Faster)" : ""
                );
            GUILayout.Label(msg, centeredStyle);
        }
        else
            GUILayout.Label("Keyboard Move: DISABLED", centeredStyle);

        if (_scMouse != null && _scMouse.enabled)
        {
            var msg = string.Format("Mouse Move: {0} | Pan: {3} | Rotate/Tilt: {1} | Zoom: {2}",
                _scMouse.AllowScreenEdgeMove ? ("screen edges") : "(disabled)",
                _scMouse.AllowRotate || _scMouse.AllowTilt ? ("Middle Button") : "(disabled)",
                _scMouse.AllowZoom ? ("Mouse Scroll") : "(disabled)",
                _scMouse.AllowPan ? ("Shift+Middle") : "(disabled)"
                );
            GUILayout.Label(msg, centeredStyle);
        }
        else
            GUILayout.Label("Mouse Move: DISABLED", centeredStyle);

        GUILayout.EndVertical();
        //GUILayout.EndArea();
    }

    private void GuiActions()
    {
        GUILayout.BeginVertical("box");

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Spawn Worker (G)"))
        {
            _scDemoMain.SpawnWorker();
        }

        if (GUILayout.Button("Target Worker (Tab)"))
        {
            _scDemoMain.TargetRandomWorker();
        }

        if (GUILayout.Button("Reset Camera (T)"))
        {
            _scDemoMain.CenterCamera();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (_scCamera != null)
        {
            if (GUILayout.Button("Preset 1"))
            {
                _scCamera.Distance = Mathf.Lerp(_scCamera.MinDistance, _scCamera.MaxDistance, 0f);
                _scCamera.Tilt = Mathf.Lerp(_scCamera.MinTilt, _scCamera.MaxTilt, 0f);
            }

            if (GUILayout.Button("Preset 2"))
            {
                _scCamera.Distance = Mathf.Lerp(_scCamera.MinDistance, _scCamera.MaxDistance, 0.25f);
                _scCamera.Tilt = Mathf.Lerp(_scCamera.MinTilt, _scCamera.MaxTilt, 0.25f);
            }

            if (GUILayout.Button("Preset 3"))
            {
                _scCamera.Distance = Mathf.Lerp(_scCamera.MinDistance, _scCamera.MaxDistance, 0.50f);
                _scCamera.Tilt = Mathf.Lerp(_scCamera.MinTilt, _scCamera.MaxTilt, 0.50f);
            }

            if (GUILayout.Button("Preset 4"))
            {
                _scCamera.Distance = Mathf.Lerp(_scCamera.MinDistance, _scCamera.MaxDistance, 0.75f);
                _scCamera.Tilt = Mathf.Lerp(_scCamera.MinTilt, _scCamera.MaxTilt, 0.75f);
            }

            if (GUILayout.Button("Preset 5"))
            {
                _scCamera.Distance = Mathf.Lerp(_scCamera.MinDistance, _scCamera.MaxDistance, 1f);
                _scCamera.Tilt = Mathf.Lerp(_scCamera.MinTilt, _scCamera.MaxTilt, 1f);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void GuiCameraOptions()
    {
        if (_scCamera == null)
            return; // no camera, bail!

        GUILayout.BeginVertical("box");
        GUILayout.Label("Camera Options");

        _scCamera.ShowDebugCameraTarget = GUILayout.Toggle(_scCamera.ShowDebugCameraTarget, "Show Camera Target");
        _scCamera.TerrainHeightViaPhysics = GUILayout.Toggle(_scCamera.TerrainHeightViaPhysics, "Height via Physics");
        _scCamera.TargetVisbilityViaPhysics = GUILayout.Toggle(_scCamera.TargetVisbilityViaPhysics, "Target Visibility via Physics");

        GUILayout.Space(8);

        _scCamera.Smoothing = GUILayout.Toggle(_scCamera.Smoothing, "Smoothing");
        if (_scCamera.Smoothing)
        {
            var damp = _scCamera.MoveDampening;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Dampening: ");
            damp = GUILayout.HorizontalSlider(damp, 1f, 10f);
            GUILayout.EndHorizontal();

            _scCamera.MoveDampening = damp;
            _scCamera.RotationDampening = damp;
            _scCamera.TiltDampening = damp;
            _scCamera.ZoomDampening = damp;
        }

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Lookat Offset: ");
        _scCamera.LookAtHeightOffset = GUILayout.HorizontalSlider(_scCamera.LookAtHeightOffset, 0f, 3f);
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        _scCamera.FollowBehind = GUILayout.Toggle(_scCamera.FollowBehind, "Force Rotate on Follow");
        if (_scCamera.FollowBehind)
        {
            _scCamera.FollowRotationOffset = GUILayout.HorizontalSlider(_scCamera.FollowRotationOffset, 0, 360);
        }

        GUILayout.Space(8);

        GUILayout.EndVertical();
    }

    private void GuiMouseOptions()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("Mouse Controls");

        _scMouse.enabled = GUILayout.Toggle(_scMouse.enabled, "Enable Mouse Movement");
        if (_scMouse.enabled)
        {
            _scMouse.AllowScreenEdgeMove = GUILayout.Toggle(_scMouse.AllowScreenEdgeMove, "Enable Screen Edge Movement");
            if (_scMouse.AllowScreenEdgeMove)
            {
                _scMouse.ScreenEdgeMoveBreaksFollow = GUILayout.Toggle(_scMouse.ScreenEdgeMoveBreaksFollow,
                                                                       "Edge Movement Breaks Follow");
            }

            _scMouse.AllowPan = GUILayout.Toggle(_scMouse.AllowPan, "Enable Shift+Middle Panning");
            if (_scMouse.AllowPan)
            {
                _scMouse.PanBreaksFollow = GUILayout.Toggle(_scMouse.PanBreaksFollow, "Pan Breaks Follow");
            }
        }

        GUILayout.EndVertical();
    }

    private void GuiKeyboardOptions()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("Keyboard Controls");

        _scKeys.enabled = GUILayout.Toggle(_scKeys.enabled, "Enable Keyboard Movement");
        if (_scKeys.enabled)
        {
            _scKeys.MovementBreaksFollow = GUILayout.Toggle(_scKeys.MovementBreaksFollow, "Movement Breaks Follow");
        }

        GUILayout.EndVertical();
    }
}
