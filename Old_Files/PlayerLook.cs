using UnityEditor.Experimental.GraphView;
using UnityEditor.UI;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;
    private float xRotation = 0f;
    private float yRotation = 0f;
    public float Sensitivity = 0.125f;

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void Look(Vector2 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;
        xRotation -= mouseY * Sensitivity;
        yRotation -= mouseX * Sensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * (mouseX * Sensitivity));
    }
}
