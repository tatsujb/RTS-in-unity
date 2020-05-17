using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region variables
    [Header("[10 - 100] :")]
	[Range(10f, 100f)]
    public float scrollSpeed = 80f;
	[Header("determines whether stronger scrolling accelerates scroll :")]
	public bool scrollMultiplier = true;
	[Header("[0.1 - 3]")]
	[Range(0.1f, 3f)]
	public float distanceToGround = 0.4f; // find a good value that prevents camera-clipping but as small as possible
	private float finalDistanceToGround = 0f;// placeholder, this value takes distanceToGround + hitpoint height
    [Header("[10 - 100] : ")]
    [Range(10f, 100f)]
    public float panSpeed = 80f;
	[Header("determines whether continued Panning accelerates pan :")]
    public bool panMultiplier = true;
	//[Header("Multipliers duration (backwards) [0.5 - 0.0025]")]
    //[Range(0.5f, 0.0025f)]
    private float multipliersDuration = 0.02f; // number seconds is the slowest repeat fo be able to catch double scroll wheel
	// GameObjects
	public Transform startPos; // Set up an empty object with the desired transform
	public CameraRotator cameraRotator; // needed for camera angle 

	// markers for time multiplication
    private bool wasDoing2;

    private bool panningHorizontaly;
	private float horizontalPan = 0f;
    private float subsequentHors = 1f;
    
    private bool panningVerticaly;
    private float verticalPan = 0f;
    private float subsequentVerts = 1f;
    
    private bool scrolling;
	private float scrollWheel = 0f;
    private float subsequentScrolls = 1f;
    // end multiplication markers
	
    private Rigidbody rb;
    private Camera cam;
    private TerrainGenerator terrain;
    private Vector3 originalPosition;
    private Vector3 deadScrollPosition;
    private Vector3 movement;
    private Vector3 cursor;
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

        deadScrollPosition = new Vector3(terrain.xSize / 2f, 0f, terrain.ySize / 2f);

        originalPosition = startPos.position;
        distance = startPos.position.y - distanceToGround;
        
		if (panMultiplier) { // not checking for scrollMult boolean because I need the scroll mult for camera rotation
        	InvokeRepeating(nameof(CheckForRepeatInputs), multipliersDuration, multipliersDuration);
		}
    }

    // Update is called once per frame
    void Update()
    {
        // Finds position of the mouse
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
		Vector3 correctedCursor = new Vector3(0f, 0f, 0f);
		bool ourRaycastHitSomething = false;
		if (Physics.Raycast(ray, out hit))
        {
			ourRaycastHitSomething = true;
			finalDistanceToGround = hit.point.y < 0.5f ? hit.point.y + distanceToGround : hit.point.y + distanceToGround + 4f;
            // store the hit point for external use and correct with threshold
            cursor = hit.point;
			correctedCursor = new Vector3(hit.point.x, finalDistanceToGround, hit.point.z);
		}

		var position = transform.position;

		// TODO : fix two pan bugs :
		// A.) simultaneous pan & scroll inverts pan dirrections until another big change in scroll occurs.
		// B.) pan on camera rotation is still global pan, should be relative to camera direction.
        // #1 HORIZONTAL
		horizontalPan = Input.GetAxis("Horizontal");
		float horizontal = 0f;
		void addHors ()
        {
            horizontal = horizontalPan * panSpeed * subsequentHors * currentDistance / 120f * Time.deltaTime;
        };
		// TODO : make these values not hard; currently they fit a 256x256 map with 250 max zoom,
		// but what if those are not the values of the current map? (obviously we can do all this much later)

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
        if (scrollWheel != 0f)
        {
			isZoomingOut = false;
            // if scrolling in
            if (scrollWheel > 0.001f && transform.position.y > finalDistanceToGround * 2)
            {    
                // Shoots the ray
                if (ourRaycastHitSomething)
                {
                    // Finds angle & Moves to the target point
                    MoveToPointWithAccelerationCalculation(position, correctedCursor - position, true);
                }
                else
                {
                    // Finds angle & Moves to the dead scroll pos
                    MoveToPointWithAccelerationCalculation(position, deadScrollPosition - position, true);
                }
            }
            
            else if (scrollWheel == 0f)
            {
				isZoomingOut = false;
                return;
            }
            
            else if (position != originalPosition && scrollWheel < 0.001f)
            {
				isZoomingOut = true;
                // if "close enough" to start position just snap back to start position
                if (position.y >= originalPosition.y - 60f)
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

    // FixedUpdated is called possibly multiple times per frame
    void FixedUpdate()
    {
        transform.position += movement;
    }

    public void MoveToPointWithAccelerationCalculation(Vector3 position, Vector3 target, bool zoomingIn)
    {
        // stores distance for external use
        currentDistance = position.y - target.y;
        float inOrOutMult = zoomingIn ? 200f : position.y + 60f;
		
		float times = scrollSpeed * inOrOutMult / 800f;
		if (scrollMultiplier) {// accelerating scroll according to how much you scrolled but not too much.
			times = scrollSpeed * inOrOutMult * subsequentScrolls / 800f;
		}
        
        Vector3 tempPosition = position;
        tempPosition += times * Time.deltaTime * target;
		
		
		void runTransforms ()
		{
			if (tempPosition.y < distanceToGround) // prevents going under the map or distance to ground
			{
				tempPosition.y = finalDistanceToGround;
				position = tempPosition;
			}
			else
        	{
            	position = tempPosition;
        	}
		}
		float horizontal = Mathf.Abs(position.x - tempPosition.x);
		float vertical = Mathf.Abs(position.z - tempPosition.z);
		float height = Mathf.Abs(position.y - tempPosition.y);
		if (horizontal * 0.9f > height || vertical * 0.9f > height)
		{
			// going further laterally then vertically.
			// this zooming/de-zooming type is prohibited by default. but allowed when camera is rotated.
			if (cameraRotator.getXRotation() < 2f) // angle is negative
			{
				runTransforms ();
			}
			else
			{
				Debug.Log("Interesting. vetical =  " + vertical + " and horizontal = " + horizontal + " while height = " + height);
			}
		}
		else
		{
        	runTransforms ();
		}
		transform.position = position;
    }

	// multipliers
    public void CheckForRepeatInputs()
		// TODO	: look for a more optimized solution than this
        // (this is already a thousand times better than my previous multiplier code, but still could be way better)
		// (I just don't know how)
    {
		if(panMultiplier)
		{	
			// #1 HORIZONTAL
        	bool wasPanningHorizontaly = panningHorizontaly;
        	bool isPanningHorizontaly = Math.Abs(horizontalPan) > 0f;
        	subsequentHors = IncrementOrDecrementDoingCounter(subsequentHors, isPanningHorizontaly, wasPanningHorizontaly);
        	panningHorizontaly = isPanningHorizontaly;
        	// #2 VERTICAL
        	bool wasPanningVerticaly = panningVerticaly;
        	bool isPanningVerticaly = Math.Abs(verticalPan) > 0f;
        	subsequentVerts = IncrementOrDecrementDoingCounter(subsequentVerts, isPanningVerticaly, wasPanningVerticaly);
        	panningVerticaly = isPanningVerticaly;
		}

        // #3 SCROLL
        bool wasScrolling = scrolling;
        bool isScrolling = scrollWheel != 0f;
        subsequentScrolls = IncrementOrDecrementDoingCounter(subsequentScrolls, isScrolling, wasScrolling);
        scrolling = isScrolling;
    }


    public float IncrementOrDecrementDoingCounter(float counter, bool isDoing, bool wasDoing)
    {
		float ret = 1f;

		if(isDoing){
			if(wasDoing){
				wasDoing2 = true;
				if(wasDoing2){
					ret = 2f;
				}
			}
		}else if(!wasDoing2){
			if(!wasDoing){
				wasDoing2 = false;
			}
		}
        return ret;
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

	public float GetMultipliersDuration()
	{
		return multipliersDuration;
	}
    
    public bool GetZoomingOut()
    {
        return isZoomingOut && subsequentScrolls > 1.0001f;
    }
}