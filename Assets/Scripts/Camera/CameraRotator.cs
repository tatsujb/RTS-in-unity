using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    #region variables
    public Transform controller;

    float sesitivity = 100f;

	[Header("[30 - 200] : ")]
    [Range(30f, 200f)]
	public float returnSpeed = 150f;

    float xRotation;

	private bool bringingCameraBack;
	private bool conditionIsFullfilled;
    CameraController cameraController;
    #endregion

    private void Start()
    {
        cameraController = controller.gameObject.GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
		if (bringingCameraBack)
		{
			bool closeT = false;
			bool closeC = false;
			if (transform.rotation.eulerAngles.x < 88f)
			{
				transform.Rotate(returnSpeed * Time.deltaTime, 0f , 0f);
			}
			else
			{
				closeT = true;
			}
			float cy = controller.transform.rotation.eulerAngles.y;
			if (controller.transform.rotation.eulerAngles.y < 175f)
			{
				controller.Rotate(0f, 0f, -returnSpeed * Time.deltaTime);
			}
			else
			{
				if (controller.transform.rotation.eulerAngles.y > 185f)
				{
					controller.Rotate(0f, 0f, returnSpeed * Time.deltaTime);
				}
				else
				{
					closeC = true;
				}
			}
			if (closeT && closeC) {
				transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            	controller.localRotation = Quaternion.Euler(90f, 180f, 0f);
				bringingCameraBack = false;
			}
		}
		else
		{
        	if ((cameraController.GetStartingDistance() / 4f) * 1f < transform.position.y)
       		{
				if(conditionIsFullfilled)
				{
					bringingCameraBack = true;
					conditionIsFullfilled = false;
				}
        	}
        	else
        	{
				if (Input.GetKey(KeyCode.Space))
            	{
                	float mouseX = Input.GetAxis("Mouse X") * sesitivity * Time.deltaTime;
                	float mouseY = Input.GetAxis("Mouse Y") * sesitivity * Time.deltaTime;

                	xRotation -= mouseY;
                	xRotation = Mathf.Clamp(xRotation, -90f, 0f); // We can change the clamp values
                	transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                	controller.Rotate(Vector3.back * mouseX);
					conditionIsFullfilled = true;
            	}
        	}
		}
    }

	public float getXRotation() {
		return xRotation;
	}
}
