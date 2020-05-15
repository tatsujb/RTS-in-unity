using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region variables
    [Header("ZOOM speed : (10 - 100)")]
	[Range(10f, 100f)]
    public float scrollSpeed = 8f;
	[Header("determines whether stronger scrolling accelerates scroll :")]
	public bool scrollMultiplier = true;
    [Header("PAN speed : (10 - 100)")]
    [Range(10, 100)]
    public float panSpeed = 80;
	[Header("determines whether continued Panning accelerates pan :")]
    public bool panMultiplier;

	// these two are hard locked now
    private float distanceToGround = 0.2f; // cannot be lower without introducing bugs, cannot be higher without being unable to view buildings and units well enough 
    private float scrollIncrementTimer = 0.0025f;



	// markers for time multiplication
    private bool panningHorizontaly;
    private float subsequentHors = 1f;
    
    private bool panningVerticaly;
    private float subsequentVerts = 1f;
    
    private bool scrolling;
    private float subsequentScrolls = 1f;
    private bool wasDoing2;
    // end multiplication markers



	public Transform startPos; // Set up an empty object with the desired transform
    private Rigidbody rb;
    private Camera cam;
    private TerrainGenerator terrain;
    private Vector3 originalPosition;
    private Vector3 deadScrollPosition;
    private Vector3 movement;
    private Vector3 cursor;
    private float horizontalPan = 0f;
    private float verticalPan = 0f;
    private float scrollWheel = 0f;
    private float threshHold;
    private float distance;
    private float currentDistance = 250f; //getter for outside calls, cannot be 0;
	private bool isZoomingOut;
	
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        terrain = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainGenerator>();

        threshHold = terrain.height;
        deadScrollPosition = new Vector3(terrain.xSize / 2f, 0f, terrain.ySize / 2f);

        originalPosition = startPos.position;
        distance = startPos.position.y - threshHold;
        
		if (scrollMultiplier || panMultiplier) {
        	InvokeRepeating(nameof(CheckForRepeatInputs), scrollIncrementTimer, scrollIncrementTimer);
		}
    }

    public void CheckForRepeatInputs()
    {
		if(panMultiplier)
		{	
			// #1 HORIZONTAL
        	bool wasPanningHorizontaly = panningHorizontaly;
        	bool isPanningHorizontaly = Math.Abs(horizontalPan) > 0;
        	subsequentHors = IncrementOrDecrementDoingCounter(subsequentHors, isPanningHorizontaly, wasPanningHorizontaly);
        	panningHorizontaly = isPanningHorizontaly;
        	// #2 VERTICAL
        	bool wasPanningVerticaly = panningVerticaly;
        	bool isPanningVerticaly = Math.Abs(verticalPan) > 0;
        	subsequentVerts = IncrementOrDecrementDoingCounter(subsequentVerts, isPanningVerticaly, wasPanningVerticaly);
        	panningVerticaly = isPanningVerticaly;
		}
		else
		{
        	// #3 SCROLL
        	bool wasScrolling = scrolling;
        	bool isScrolling = scrollWheel != 0;
        	subsequentScrolls = IncrementOrDecrementDoingCounter(subsequentScrolls, isScrolling, wasScrolling);
        	scrolling = isScrolling;
		}
    }


    public float IncrementOrDecrementDoingCounter(float counter, bool isDoing, bool wasDoing)
    {
		// TODO	: look for a more optimized solution than this
        // (this is already a thousand times better than my previous multiplier code, but still could be way better)
		// (I just don't know how)
		float ret = 2f;
		if (wasDoing && isDoing)
		{
			ret = 3f;
		}
		if (!wasDoing && !isDoing)
		{	
			if (!wasDoing2) {
				ret = 1f;
			}
			wasDoing2 = false;
		} else {
			if(wasDoing || isDoing) {
				wasDoing2 = true;
			}
		}
        return ret;
    }

    // Update is called once per frame
    void Update()
    {
        // Finds position of the mouse
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
		bool ourRaycastHitSomething = false;
		if (Physics.Raycast(ray, out hit))
        {
			ourRaycastHitSomething = true;
            // store the hit point for external use
            cursor = hit.point;
		}

		var position = transform.position;

		
        // #1 HORIZONTAL
		horizontalPan = Input.GetAxis("Horizontal");
		float horizontal = 0f;
		void addHors ()
        {
            horizontal = horizontalPan * panSpeed * subsequentHors * currentDistance / 120f * Time.deltaTime;
        };
		// TODO : make these values not hard; currently they fit a 256x256 map with 250 max zoom,
		// but what if those values change?

		if (position.x < 256f && position.x > 0f)
		{
			 addHors ();
		}
		else
		{
			if (position.x > 0f)
			{
				if (horizontalPan > 0f)
				{
					addHors ();
				}
			}
			else
			{
				if (horizontalPan < 0f)
				{
					addHors ();
				}
			}
		}
		// #2 VERTICAL
		verticalPan = Input.GetAxis("Vertical");
        float vertical = 0f;
		void addVert ()
		{
			vertical = verticalPan * panSpeed * subsequentVerts * currentDistance / 120f * Time.deltaTime;
		}
		if (position.z < 256f && position.z > 0f )
		{
			 addVert ();
		}
		else
		{
			if (position.z > 0f)
			{
				if (verticalPan > 0f)
				{
					addVert ();
				}
			}
			else
			{
				if (verticalPan < 0f)
				{
					addVert ();
				}
			}
		}

		movement = new Vector3(-horizontal, 0f, -vertical);
        
        // #3 SCROLL
        scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0)
        {
			isZoomingOut = false;
            // if scrolling in (must not bypass threshold, otherwise, do nothing)
            if (scrollWheel > 0.001 && position.y > (threshHold + distanceToGround))
            {    
                // Shoots the ray
                if (ourRaycastHitSomething)
                {
                    // resets the threshold based on current zoom point
                    threshHold = hit.point.y + distanceToGround;

					// checks for and fixes aberant values in our raycast
					if(hit.point.y < 0) {
						cursor.y = 0;
					}
                    
                    // Finds angle & Moves to the target point
                    MoveToPointWithAccelerationCalculation(position, cursor - position, true);
                }
                else
                {
                    // Finds angle & Moves to the dead scroll pos
                    MoveToPointWithAccelerationCalculation(position, deadScrollPosition - position, true);
                }
            }
            
            else if (scrollWheel == 0)
            {
				isZoomingOut = false;
                return;
            }
            
            else if (position != originalPosition && scrollWheel < 0.001)
            {
				isZoomingOut = true;
                // if "close enough" to start position just snap back to start position
                if (position.y >= originalPosition.y - 60)
                {
                    transform.position = originalPosition;
                }
                else
                {
                    // Finds angle to startPos and Moves to startPos
                    MoveToPointWithAccelerationCalculation(position, startPos.position - position, false);
                }
            }
        }
    }

    public void MoveToPointWithAccelerationCalculation(Vector3 position, Vector3 direction, bool zoomingIn)
    {
        // stores distance for external use
        currentDistance = position.y - direction.y;
        float inOrOutMult = zoomingIn ? 40f : position.y;
		
		float times = scrollSpeed * inOrOutMult / 800f;
		if (scrollMultiplier) {// accelerating scroll according to how much you scrolled but not too much.
			times = scrollSpeed * inOrOutMult * subsequentScrolls / 800f;
		}
        
        Vector3 tempPosition = position;
        tempPosition += times * Time.deltaTime * direction;

		//if (Mathf.Abs(position.x - tempPosition.x) > Mathf.Abs(position.y - tempPosition.y) || Mathf.Abs(position.z - tempPosition.z) > Mathf.Abs(position.y - tempPosition.y))
		//{
			// going further laterally then vertically.
			// this zooming/de-zooming type is prohibited by default. but allowed when camera is rotated. 
		//}
		//else
		//{
			
        	if (tempPosition.y <= 0) {
				Debug.Log("BANG ! Went under 0 : " + tempPosition.y);
				position += Time.deltaTime * direction;
			}
			else if (tempPosition.y > threshHold)
        	{
            position = tempPosition;
        	}
			else
        	{
				Debug.Log("BANG ! exploded our threshold : " + tempPosition.y + " threshhold " + threshHold);
            	position += scrollSpeed/10 * Time.deltaTime * direction;
        	}
        	transform.position = position;
		//}
        
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
    
    public bool GetZoomingOut()
    {
        return isZoomingOut;
    }
}