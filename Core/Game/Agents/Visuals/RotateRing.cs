using UnityEngine;

public class RotateRing : MonoBehaviour {
    public float Speed;

    private void Update() {
        var y = transform.eulerAngles.y + Speed * Time.deltaTime;
        if (y > 360) {
            y -= 360;
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, y, transform.eulerAngles.z);
    }
}