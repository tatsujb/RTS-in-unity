using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    #region variables
    public Material defaultMatirial, highlightMatirial;

    bool selected;

    Renderer mRenderer;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        mRenderer = GetComponent<Renderer>();

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
    }

    public void SetSelection(bool selection)
    {
        selected = selection;
    }
}
