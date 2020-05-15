using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    #region variables
    public Transform controller;

    float sesitivity = 100f;

    float xRotation;

    CameraController cameraController;
    #endregion

    private void Start()
    {
        cameraController = controller.gameObject.GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((cameraController.GetStartingDistance() / 4) * 3 >= transform.position.y && !cameraController.GetZoomingOut())
        {
            if (Input.GetKey(KeyCode.Space))
            {
                float mouseX = Input.GetAxis("Mouse X") * sesitivity * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * sesitivity * Time.deltaTime;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 0f); // We can change the clamp values

                transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                controller.Rotate(Vector3.back * mouseX);
            }
        }
        else
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            controller.localRotation = Quaternion.Euler(90f, 180f, 0f);
        }
    }
}
