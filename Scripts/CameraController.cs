using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Look Sensitivity")]
    public float sensX;
    public float sensY;

    [Header("Clamping")]
    public float minY;
    public float maxY;

    [Header("Spectator")]
    public float spectatorMoveSpeed;

    private float rotX;
    private float rotY;

    private bool isSpectator;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  //lock cursor in ceter
    }

    private void LateUpdate()
    {
        //get mouse inputs
        rotX += Input.GetAxis("Mouse X") * sensX;
        rotY += Input.GetAxis("Mouse Y") * sensY;

        // clamp vert rot
        rotY = Mathf.Clamp(rotY, minY, maxY);

        //are we spectationg
        if (isSpectator)
        {
            //rot cam vert
            

            //movement
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            float z = 0;

            if (Input.GetKey(KeyCode.E))
                y = 1;
            else if (Input.GetKey(KeyCode.Q))
                y = -1;

            Vector3 dir = transform.right * x + transform.up * y + transform.forward * z;
            transform.position += dir * spectatorMoveSpeed * Time.deltaTime;
        }
        else
        {
            //rot vert
            transform.localRotation = Quaternion.Euler(-rotY, 0, 0);

            //rot player horizontally
            transform.parent.rotation = Quaternion.Euler(0, rotX, 0);
        }
    }
}
