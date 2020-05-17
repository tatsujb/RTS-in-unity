using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    #region variables
    public string player;

    public float range;
    public float damage;

    public Material defaultMatirial, highlightMatirial;

    public GameObject rotator;

    bool selected;

    Renderer mRenderer;
    NavMeshAgent agent;
    SphereCollider colliderS;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        mRenderer = GetComponent<Renderer>();
        agent = GetComponent<NavMeshAgent>();
        colliderS = GetComponent<SphereCollider>();

        mRenderer.material = defaultMatirial;
        colliderS.radius = range;
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

    private void OnTriggerStay(Collider other)
    {
        Unit otherUnit = other.GetComponent<Unit>();

        if (otherUnit != null && otherUnit.player != player)
        {
            // Rotation

            // Attack();
        }
    }
}
