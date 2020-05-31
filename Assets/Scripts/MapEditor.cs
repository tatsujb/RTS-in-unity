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
        // terrain.Generate(false);
    }

    public void ChangeOffsetX(float amount)
    {
        terrain.xOffset += amount;

        terrain.Generate(false);
    }

    public void ChangeOffsetY(float amount)
    {
        terrain.yOffset += amount;

        terrain.Generate(false);
    }

    public void ChangeHeight(float amount)
    {
        terrain.height += amount;

        terrain.Generate(false);
    }

    public void ChangeOctaves(int amount)
    {
        terrain.octaves += amount;

        if (terrain.octaves <= 0)
            terrain.octaves = 1;

        terrain.Generate(false);
    }

    public void ChangeScale()
    {
        terrain.noiseScale = scaleSlider.value;

        terrain.Generate(false);
    }
}
