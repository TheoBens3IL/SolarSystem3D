using UnityEngine;

public class FollowCameraController : MonoBehaviour
{
    public Transform target;  // Planet to follow
    public Vector3 offset = new Vector3(0, 5, -10);  // View offset
    public float smoothSpeed = 5f;  // Interpolation speed

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.LookAt(target);
    }
}
