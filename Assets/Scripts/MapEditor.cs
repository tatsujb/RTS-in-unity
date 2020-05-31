using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditor : MonoBehaviour
{
    #region variables
    TerrainGenerator terrain;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        terrain = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        // terrain.Generate(true);
    }

    public void ChangeHeight(float amount)
    {
        terrain.height += amount;

        terrain.Generate(true);
    }
}
