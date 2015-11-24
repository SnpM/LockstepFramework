using UnityEngine;

public class FollowHelper : MonoBehaviour
{
    public GameObject FollowTarget;
    public bool Snap = true;

    private RtsCamera _rtsCamera;
    private GameObject _prevFollowTarget;

    void Reset()
    {
        FollowTarget = null;
        Snap = true;
    }

    void Start()
    {
        _rtsCamera = Camera.main.GetComponent<RtsCamera>();
        SetTarget();
    }

    void Update()
    {
        if (FollowTarget != _prevFollowTarget)
        {
            SetTarget();
        }
    }

    private void SetTarget()
    {
        _rtsCamera.Follow(FollowTarget, Snap);
        _prevFollowTarget = FollowTarget;
    }
}
