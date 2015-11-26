using UnityEngine;

public class FollowOnClick : MonoBehaviour
{
    public bool Snap = false;

    private RtsCamera _rtsCamera = null;

    void OnMouseDown()
    {
        if (_rtsCamera == null)
        {
            _rtsCamera = FindObjectOfType<RtsCamera>();
        }

        if (_rtsCamera == null)
        {
            Debug.LogWarning("FollowOnClick.OnClick - no RtsCamera instance found in this Scene!");
            return;
        }

        _rtsCamera.Follow(this.gameObject, Snap);
    }
}
