using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region variables
    public Transform startPos; // Set up an empty object with the desired transform
    
    [Header("ZOOM criterions :")]
    [Range(0.1f, 1.3f)]
    public float sensitivity = 1.1f;
    [Range(1f, 8f)]
    public float scrollSpeed = 1.5f;
    [Header("Multiplier : (used for multiplying & sampling timeout)")]
    [Range(3f, 40f)]
    public float scrollMultiplier = 36f;
    [Header("lower is more sampling :")]
    [Range(0.1f, 0.01f)]
    public float scrollIncrementTimer = 0.01f;
    [Header("PAN :")]
    [Range(1f, 200f)]
    public long panSpeed = 2;
    
    [Header("Careful : exponential")]
    
    [Range(1f, 7f)]
    public long panMultiplier = 7;
    // public bool inverted;

    [Range(0f, 40f)]
    public float distanceToGround = 5f;
    
    
    private Camera cam;
    private Rigidbody rb;
    private Vector3 movement;

    private Vector3 deadScrollPosition;

    private TerrainGenerator terrain;

    private float threshHold;

    private float distance;
    
    // markers for time multiplication
    private bool panningHorizontaly = false;
    private float continuousHorizontalPanCounter = 0f;
    
    private bool panningVerticaly = false;
    private float continuousVerticalPanCounter = 0f;
    
    private bool scrolling = false;
    private float continuousScrollsCounter = 0f;
    // end markers

    // avoiding multiple calls per frame to inputs
    private float horizontalPan = 0f;
    private float verticalPan = 0f;
    private float scrollWheel = 0f;

    private Vector3 originalPosition;
    private Vector3 cursor;
    private float currentDistance = 250f; // generic non-zero value
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        terrain = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainGenerator>();

        threshHold = terrain.height; // You can add more distance here 
        deadScrollPosition = new Vector3(terrain.xSize / 2f, 0f, terrain.ySize / 2f);

        originalPosition = startPos.position;
        distance = startPos.position.y - threshHold;
        
        InvokeRepeating(nameof(CheckForRepeatInputs), scrollIncrementTimer, scrollIncrementTimer);
    }

    public void CheckForRepeatInputs()
    {
        // #1 #2 is dead code until to-do below is fixed
        
        // #1 HORIZONTAL
        bool wasPanningHorizontaly = panningHorizontaly;
        bool isPanningHorizontaly = Math.Abs(horizontalPan) > 0;
        continuousHorizontalPanCounter = IncrementOrDecrementDoingCounter(continuousHorizontalPanCounter, isPanningHorizontaly, wasPanningHorizontaly);
        panningHorizontaly = isPanningHorizontaly;
        // #2 VERTICAL
        bool wasPanningVerticaly = panningVerticaly;
        bool isPanningVerticaly = Math.Abs(verticalPan) > 0;
        continuousVerticalPanCounter = IncrementOrDecrementDoingCounter(continuousVerticalPanCounter, isPanningVerticaly, wasPanningVerticaly);
        panningVerticaly = isPanningVerticaly;
        // #3 SCROLL
        bool wasScrolling = scrolling;
        bool isScrolling = scrollWheel != 0;
        continuousScrollsCounter = IncrementOrDecrementDoingCounter(continuousScrollsCounter, isScrolling, wasScrolling);
        scrolling = isScrolling;
    }

    /*
     * this method has a lot of conditions that lay out an incrementing curve and a decrementing curve
     * we're aiming for capped and temporarily permeated values that will serve both as a criterion for multiplying further
     * and as a multiplier itself
     */
    public float IncrementOrDecrementDoingCounter(float counter, bool isDoing, bool wasDoing)
    {
        float result = 0f;
        if (isDoing)
        {
            if (counter == 0f)
            {
                result = 1f;
            }
            else if (counter < scrollMultiplier)
            {
                if (wasDoing)
                {
                    result = counter * sensitivity * 2;
                }
                else
                {
                    result = counter * sensitivity; 
                }
            }
        }
        else if (counter > 0f)
        {
            if (!wasDoing)
            {
                if (counter < scrollMultiplier / 2f)
                {
                    counter = counter / 2f;
                    if (counter < 2f)
                    {
                        result = 0f;
                    }
                }
                else
                {
                    result = counter - (scrollMultiplier / 10);
                }
            }
        }

        return result;
    }

    // Update is called once per frame
    void Update()
    {
        // #1 HORIZONTAL 
        float horizontal = Input.GetAxis("Horizontal") * panSpeed * Time.deltaTime;
        // #2 VERTICAL
        float vertical = Input.GetAxis("Vertical") * panSpeed * Time.deltaTime;

        movement = new Vector3(-horizontal, 0f, -vertical);
        
        //TODO : implement a more functional version of the following copy of the pan code, the idea is to avoid
        // unnecessary redundant calls and to globalize the horizontal and vertical pon variables
        // also we can test having reached max pan or not and do nothing if attempting to pan even further than max
        
        // horizontalPan = Input.GetAxis("Horizontal");
        // if (Math.Abs(horizontalPan) > 0)
        // {
        //     if (Pow(panSpeed, panMultiplier) < originalPosition.x * 2)
        //     {
        //         movement = new Vector3(-(Pow(panSpeed, panMultiplier) * Time.deltaTime * horizontalPan), 0f, 0f);
        //     }
        // }
        //
        // 
        // verticalPan = Input.GetAxis("Vertical"); ;
        // if (Math.Abs(verticalPan) > 0)
        // {
        //     if (Pow(panSpeed, panMultiplier) < originalPosition.z * 2)
        //     {
        //         movement = new Vector3(0f, 0f, -(Pow(panSpeed, panMultiplier)  * Time.deltaTime * verticalPan ));
        //     }
        // }
        
        // #3 SCROLL
        scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0)
        {
            // curPos = transform.position;
            var position = transform.position;
            
            // if scrolling in (must not bypass threshold, otherwise, do nothing)
            if (scrollWheel > 0.001 && position.y > (threshHold + distanceToGround))
            {
                // Finds position of the mouse
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
    
                // Shoots the ray
                if (Physics.Raycast(ray, out hit))
                {
                    // store the hit point for external use
                    cursor = hit.point;

                    // resets the threshold based on current zoom point
                    threshHold = hit.point.y + distanceToGround;
                    
                    // Finds angle & Moves to the target point
                    MoveToPointWithAccelerationCalculation(position, hit.point - position);
                }
                else
                {
                    // Finds angle & Moves to the dead scroll pos
                    MoveToPointWithAccelerationCalculation(position, deadScrollPosition - position);
                }
                // NOTE: We can also use this method for the selection methods : noted! I thought the same :)
            }
            
            else if (scrollWheel == 0)
            {
                return;
            }
            
            else if (position != originalPosition && scrollWheel < 0.001)
            {
                // if "close enough" to start position just set start position
                if (position.y >= originalPosition.y - 2)
                {
                    transform.position = originalPosition;
                }
                else
                {
                    // Finds angle to startPos and Moves to startPos
                    MoveToPointWithAccelerationCalculation(position, startPos.position - position);
                }
            }
        }
    }

    public void MoveToPointWithAccelerationCalculation(Vector3 position, Vector3 direction)
    {
        // stores distance for external use
        currentDistance = position.y - direction.y;
        
        // accelerating scroll according to how much you scrolled but not too much.
        float heightMult = position.y < threshHold * 1.5f ? 10f : position.y;
        float mult = heightMult / (originalPosition.y / continuousScrollsCounter < scrollMultiplier ?
            continuousScrollsCounter == 0 ? 1f : continuousScrollsCounter : scrollMultiplier);
        
        Vector3 tempPosition = position;
        tempPosition += mult * scrollSpeed * Time.deltaTime * direction;
        if (tempPosition.y > threshHold)
        {
            position = tempPosition;
        }
        // god forbid, our math above creates an aberrant multiplier in the trillions or above...,
        // we rectify with a more conservative multiplier : just scrollSpeed
        // this should avoid the camera instantly crashing into the ground
        // realistically this else is not entered is serves as an error catch
        else
        {
            position += scrollSpeed * Time.deltaTime * direction;
        }
        transform.position = position;
    }
    
    // FixedUpdated is called possibly multiple times per frame
    void FixedUpdate()
    {
        transform.position += movement;
    }
    
    /*
     * Used by commented code. simple Power method for non-doubles
     */
    public static long Pow(long a, long b)
    {
        long result = 1;
        for (long i = 0; i < b; i++)
            result *= a;
        return result;
    }

    public float GetStartingDistance()
    {
        return distance;
    }

    public float GetCurrentDistance()
    {
        return currentDistance;
    }
    
    public Vector3 GetCursor()
    {
        return cursor;
    }
}
