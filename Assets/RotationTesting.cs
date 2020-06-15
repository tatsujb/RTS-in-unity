using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTesting : MonoBehaviour
{
    public Transform rotator;
    public Transform player2;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*Vector3 mousePos = new Vector3(Input.mousePosition.x,
                               Input.mousePosition.y,
                               1f);*/
        Vector3 lookPos = player2.position;
        lookPos = lookPos - transform.position;
        float angle = Mathf.Atan2(lookPos.z, lookPos.x) * Mathf.Rad2Deg;
        rotator.rotation = Quaternion.AngleAxis(-angle, Vector3.up);
    }
}
