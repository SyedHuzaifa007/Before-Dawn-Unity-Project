using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target; // Player
    public float distance = 5f;
    public float height = 2f;
    public float rotationSpeed = 3f;

    private float yaw = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Optional: lock mouse
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        yaw += Input.GetAxis("Mouse X") * rotationSpeed;

        Quaternion rotation = Quaternion.Euler(0, yaw, 0);
        Vector3 offset = rotation * new Vector3(0, height, -distance);
        transform.position = target.position + offset;

        transform.LookAt(target.position + Vector3.up * height);
    }
}
