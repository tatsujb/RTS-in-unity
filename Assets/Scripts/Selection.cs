using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selection : MonoBehaviour
{
    #region variables 

    public string player;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Transform selection = hit.transform;

                Unit selectionUnit = selection.gameObject.GetComponent<Unit>();
                if (selectionUnit != null && selectionUnit.player == player)
                {
                    selectionUnit.SetSelection(true);
                }
            }
        }
    }
}
