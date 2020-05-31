using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapEditor : MonoBehaviour
{
    #region variables
    public Slider scaleSlider;

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

    public void ChangeScale()
    {
        terrain.noiseScale = scaleSlider.value;

        terrain.Generate(true);
    }
}
