using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    #region
    [Header("First point to current Camera Controller")]
    [SerializeField]
    public CameraController CameraController;

    [Header("In Run View grid bubble around cursor when close to ground and holding shift")]
    [SerializeField]
    private bool cursorGridBubble = false;
    
    [SerializeField]
    [Range(0.0001f, 0.2f)]
    private float liftFromGround = 0.0001f;
    [Header("Draw 200x200 grid in Upper Right Corner of Map")]
    [SerializeField]
    private bool drawGizmos = false;
    
    [SerializeField]
    private bool drawGrid = false;
    
    [Header("Set the material before enabling this :")]
    [SerializeField]
    private bool useGlInsteadOfDebugLines = false;
    
    [SerializeField]
    private Material mat;

    private MeshRenderer viewingGridmesh;
    
    private Vector3[] visibleNavMesh;

    private bool isShiftCurrentlyHeldDown;

    // private NavMeshSurface surface;
    
    // THIS IS HOW MUSH SMALLER THAN 1 DIGIT (in unity) OUR SQUALE IS FOR THE ENTIRE PROJECT
    // WE USE THIS TO CREATE THE GRID
    // IF MODIFIED, ORIGINAL VALUE WAS 14f
    private static float divisionFactor = 14f;
    #endregion
    
    private void Start()
    {
        //var triangles = UnityEngine.AI.NavMesh.CalculateTriangulation();
        //Vector3[] visibleNavMesh = triangles.vertices;

        //Debug.Log("this is navMesh : " + visibleNavMesh);

        initQuad();

        InvokeRepeating(nameof(ChangeColor), 2, 2);
    }

    private bool isStepOne;

    void ChangeColor()
    {
        if (isStepOne)
        {
            viewingGridmesh.GetComponent<Renderer>().sharedMaterial.SetColor("_Color", new Color(0, 1, 0, 1));
        }
        else
        {
            viewingGridmesh.GetComponent<Renderer>().sharedMaterial.SetColor("_Color", new Color(1, 0, 0, 1));
        }

        isStepOne = !isStepOne;
    }

    /// <summary>
    /// I use update to grab Shift key
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            isShiftCurrentlyHeldDown = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            isShiftCurrentlyHeldDown = false;
        }

        if (isShiftCurrentlyHeldDown && cursorGridBubble && CameraController.GetCurrentDistance() < 80f)
        {
            drawGridBubbleAroundCursor();
            viewingGridmesh.enabled = true;
        }
        else
        {
            viewingGridmesh.enabled = false;
        }
    }

    void OnRenderObject()
    {
        if (drawGrid && useGlInsteadOfDebugLines)
        {
            SmallCornerGridPreview();
        }
    }

    void OnDrawGizmos()
    {
        if (drawGrid)
        {
            SmallCornerGridPreview();
        }
    }

    /// <summary>
    ///     This is not a method for drawing grids it only repositions given coordinate to grid
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector3 GetNearestPointOn3DGrid(Vector3 position)
    {
        float record = position.x;
        
        position -= transform.position;

        float magicFormula (float entryPoint)
        {
            float decimalPart = entryPoint % 1;
            float intergerPart = entryPoint - decimalPart;
            return Mathf.RoundToInt(decimalPart / (1 / divisionFactor)) * (1 / divisionFactor) + intergerPart;
        };
        
        Vector3 result = new Vector3(
            // width
            magicFormula(position.x),
            // didn't realise this but transforming height as well created a 3D grid,... which could be usefull?
            // I'll reanme the method to avoid confusions when calling this method from elsewhere in the future
            magicFormula(position.y),
            // length
            magicFormula(position.z));

        result += transform.position;

        return result;
    }

    /// <summary>
    /// updates bubble gid position to match cursor
    /// </summary>
    public void drawGridBubbleAroundCursor()
    {
        viewingGridmesh.transform.position -= transform.position;
        Vector3 newPosition = new Vector3(
            CameraController.GetCursor().x - 1f,
            CameraController.GetCursor().y,
            CameraController.GetCursor().z - 1f
            );
        viewingGridmesh.transform.position += transform.position;
        viewingGridmesh.transform.position = newPosition;
    }
    
    private Mesh CreateMesh() {
        Mesh mesh = new Mesh();

        mesh.name = "Grid Cell";
        mesh.vertices = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        mesh.triangles = new int[] { 0, 1, 2, 2, 1, 3 };
        mesh.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        mesh.uv = new Vector2[] { new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 0) };

        return mesh;
    }

    /// <summary>
    /// this method is a debugging tool that force the drawing of the grid in the top right corner.
    /// </summary>
    private void SmallCornerGridPreview()
    {
        if (!mat)
        {
            Debug.LogError("Please Assign a material on the inspector");
            return;
        }
        int amountOfGridToDraw = 100;
        int amountOfGizmosToDraw = 40;
        if (useGlInsteadOfDebugLines)
        {
            GL.Begin(GL.LINES);
            mat.SetPass(0);
        }

        Gizmos.color = Color.yellow;

        float currentX = 0f;
        float currentY = 0f;

        for (int x = 0; x < amountOfGridToDraw; x++)
        {
            for (int y = 0; y < amountOfGridToDraw; y++)
            {
                var point = new Vector3(currentX, 0f, currentY);
                if (drawGizmos && x < amountOfGizmosToDraw && y < amountOfGizmosToDraw)
                {
                    Gizmos.DrawSphere(point, 0.2f/divisionFactor);
                }
    
                if (drawGrid)
                {                 // 0.14f, same
                    float nextX = divisionFactor/100f + currentX;
                    float nextY = divisionFactor/100f + currentY;
                    var drawPoint2 = new Vector3(nextX, liftFromGround, currentY);
                    var drawPoint3 = new Vector3(currentX, liftFromGround, nextY);
                    point.y += liftFromGround;
                    
                    if (useGlInsteadOfDebugLines)
                    {
                        GL.Color(Color.red);
                        GL.Vertex(point);
                        GL.Vertex(drawPoint2);
                        if (currentY != amountOfGridToDraw)
                        {
                            GL.Vertex(point);
                            GL.Vertex(drawPoint3);
                            currentY += divisionFactor/100f;
                        }
                    }
                    else
                    {
                        Debug.DrawLine(point, drawPoint2, Color.red, 0.01f);
                        Debug.DrawLine(point, drawPoint3, Color.red, 0.01f);
                    }
                }
            }

            currentY = 0f;
            currentX += divisionFactor/100f;
        }

        if (useGlInsteadOfDebugLines)
        {
            GL.End();
        }
    }

    void initQuad()
    {
        viewingGridmesh = gameObject.AddComponent<MeshRenderer>();
        viewingGridmesh.sharedMaterial = new Material(Shader.Find("Unlit/GridViewer"));

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(2, 0, 0),
            new Vector3(0, 0, 2),
            new Vector3(2, 0, 2)
        };
        mesh.vertices = vertices;

        int[] tris = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        meshFilter.mesh = mesh;
    }
    
    
    ///new shader technique
    ///
    /// gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_YourParameter", someValue);

}
