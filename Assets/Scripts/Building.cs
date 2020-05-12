﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public GameObject objectSpawned;

    Grid grid;

    // Start is called before the first frame update
    void Awake()
    {
        grid = FindObjectOfType<Grid>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit))
            {
                PlaceCubeNear(hit.point);
            }
        }
    }

    void PlaceCubeNear(Vector3 point)
    {
        var finalPosition = grid.GetNearestPointOnGrid(point);

        Instantiate(objectSpawned, finalPosition, Quaternion.identity); // Of course we can change what it's placing
    }
}