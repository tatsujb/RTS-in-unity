using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    #region variables
    public Material defaultMatirial, highlightMatirial;

    bool selected;

    Renderer mRenderer;
    NavMeshAgent agent;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        mRenderer = GetComponent<Renderer>();
        agent = GetComponent<NavMeshAgent>();

        mRenderer.material = defaultMatirial;
    }

    // Update is called once per frame
    void Update()
    {
        if (selected)
        {
            mRenderer.material = highlightMatirial;
        }
        else
        {
            mRenderer.material = defaultMatirial;
        }

        if (selected && Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);
            }
        }
    }

    public void SetSelection(bool selection)
    {
        selected = selection;
    }
}
