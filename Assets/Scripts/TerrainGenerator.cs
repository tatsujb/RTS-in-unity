using System;
using UnityEngine;
using UnityEngine.AI;

public class TerrainGenerator : MonoBehaviour
{
    #region variables
    public int xSize = 256;
    public int ySize = 256;

    [Range(0f, 10f)]
    public float noiseScale = 0.9999f;

    public bool useOctaves;

    [Range(1f, 5f)]
    public int octaves = 5;

    public float height = 20;

    public float xOffset;
    public float yOffset;
    public bool randomize;
    public bool randomizeHeight;

    public bool generateOnStart;

    [Header("FOR TESTING ONLY")]
    public bool testingMode;

    Terrain terrain;

    NavMeshAgent surface;
    #endregion 

    private void Start()
    {
        
        terrain = GetComponent<Terrain>();
        surface = GameObject.FindGameObjectWithTag("NavMesh").GetComponent<NavMeshAgent>();        

        if (generateOnStart)
        {
            Generate(false);
        }

        surface.BuildNavMesh();
    }

    public void Generate(bool isCallFromEditor)
    {
        if (isCallFromEditor) {
            terrain = GetComponent<Terrain>();
            surface = GameObject.FindGameObjectWithTag("NavMesh").GetComponent<NavMeshSurface>();
        }
        
        terrain.terrainData = GenerateTerrain(terrain.terrainData);

        if (randomize)
        {
            Randomize();
        }

        
        surface.BuildNavMesh();
    }

    private void Update()
    {
        if (testingMode)
        {
            terrain.terrainData = GenerateTerrain(terrain.terrainData);
        }
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = xSize + 1;

        terrainData.size = new Vector3(xSize, height, ySize);
        terrainData.SetHeights(0, 0, GenerateHeights());

        return terrainData;
    }

    float[,] GenerateHeights()
    {
        Texture2D noiseTex = new Texture2D(xSize, ySize);
        noiseTex.filterMode = FilterMode.Point;

        float[,] heights = new float[xSize, ySize];
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < xSize; y++)
            {
                float xCoord = (float)(x - xSize/2) / xSize * noiseScale + xOffset;
                float yCoord = (float)(y - ySize / 2) / ySize * noiseScale + yOffset;

                if (useOctaves)
                {
                    NoiseS3D.octaves = octaves;

                    heights[x, y] = (float)NoiseS3D.NoiseCombinedOctaves(xCoord, yCoord);
                }
                else
                {
                    heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
                }

            }
        }

        return heights;
    }

    public void Randomize()
    {
        xOffset = UnityEngine.Random.Range(-9999f, 9999f); // This part doesn't affecy ther size, only the offset
        yOffset = UnityEngine.Random.Range(-9999f, 9999f);

        noiseScale = UnityEngine.Random.Range(0f, 10f);

        octaves = (int)UnityEngine.Random.Range(1f, 5f);

        // Maybe
        // if (UnityEngine.Random.Range(0f, 2f) > 1f)
        // {
        //     useOctaves = false;
        // }
        // else
        // {
        //     useOctaves = true;
        // }
        // Tell me what you're thinking about this part

        if (randomizeHeight)
        {
            height = UnityEngine.Random.Range(-15f, 30f);
        }
    }
}
