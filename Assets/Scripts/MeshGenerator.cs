using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    private Mesh _mesh;

    private Vector3[] _vertices;

    private Vector2[] _uv;

    private int[] _triangles;

    public int xSize = 180;

    public int zSize = 180;

    public float height = 3f;
    
    // Start is called before the first frame update
    void Start()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        _vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = Mathf.PerlinNoise(x * .3f, z * .3f) * height;
                _vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        _triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                _triangles[tris + 0] = vert + 0;
                _triangles[tris + 1] = vert + xSize + 1;
                _triangles[tris + 2] = vert + 1;
                _triangles[tris + 3] = vert + 1;
                _triangles[tris + 4] = vert + xSize + 1;
                _triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }

            vert++;
        }
        
        _uv = new Vector2[_vertices.Length];
        
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                _uv[i] = new Vector2((float) x / xSize, (float) z / zSize);
                i++;
            }
        }
    }

    void UpdateMesh()
    {
        _mesh.Clear();

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uv;
        
        _mesh.RecalculateNormals();
    }
    
    /*uncomment save then recomment to make the mesh visible in editor view
    */
    
    // private void OnDrawGizmos()
    // {
    //     if (_vertices == null)
    //         throw new NotImplementedException();
    //
    //     _mesh = new Mesh();
    //     GetComponent<MeshFilter>().mesh = _mesh;
    //     CreateShape();
    //     UpdateMesh();
    // }
}
