using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform camObj;
    public Transform orientation;
    public Transform body;

    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 80f);

        transform.localRotation = Quaternion.Euler(0f, yRotation, 0);
        camObj.localRotation = Quaternion.Euler(xRotation, 0f, 0);
        orientation.localRotation = Quaternion.Euler(0, yRotation, 0);
        body.localRotation = Quaternion.Euler(0, yRotation, 0);
    }
}
