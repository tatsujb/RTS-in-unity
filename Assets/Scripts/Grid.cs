using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    #region
    [Header("Terrain Game Object :")]
    public TerrainGenerator terrain;
    
    [SerializeField]
    [Range(0.0001f, 1f)]
    private float size = 0.5f;
        
    [SerializeField]
    [Range(0.0001f, 0.2f)]
    private float liftFromGround = 0.01f;
    
    [SerializeField]
    private bool drawGizmos = false;
    
    [SerializeField]
    private bool drawGrid = true;
    #endregion
    

    public Vector3 GetNearestPointOnGrid(Vector3 position)
    {
        position -= transform.position;

        int xCount = Mathf.RoundToInt(position.x / size);
        int yCount = Mathf.RoundToInt(position.y / size);
        int zCount = Mathf.RoundToInt(position.z / size);

        Vector3 result = new Vector3(
            (float)xCount * size,
            (float)yCount * size,
            (float)zCount * size);

        result += transform.position;

        return result;
    }


    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
    
        int storedX = 0;
        int storedY = 0;
        Vector3 previousPoint = new Vector3(0f, 0f, 0f);
    
        for (int x = 0; x < terrain.xSize; x++)
        {
            for (int y = 0; y < terrain.ySize; y++)
            {
                var point = GetNearestPointOnGrid(new Vector3(x, 0f, y));
                if (drawGizmos && x < 40 && y < 40)
                {
                    Gizmos.DrawSphere(point, 0.1f);
                }
    
                if (drawGrid && x < 100 && y < 100) //otherwise if you mess with terrain size above that you might crash unity
                // if your map is under 200, visualized grid will be same size as your map
                {
                    int tx = 1 + x;
                    int ty = 1 + x;
                    var drawPoint2 = GetNearestPointOnGrid(new Vector3(tx, 0f, y));
                    var drawPoint3 = GetNearestPointOnGrid(new Vector3(x, 0f, ty));
                    point.y += liftFromGround;
                    drawPoint2.y += liftFromGround;
                    drawPoint3.y += liftFromGround;
                    Debug.DrawLine(point, drawPoint2, Color.red, 0.01f);
                    Debug.DrawLine(point, drawPoint3, Color.red, 0.01f);
                    
                }
                storedY = y;
            }
            storedX = x;
        }
    }
}
