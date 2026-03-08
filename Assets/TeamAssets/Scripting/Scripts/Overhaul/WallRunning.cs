using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    private PlayerController playerController;
    private InputManager2 inputManager;
    private Rigidbody rb;

    [Header("Wall Running")]
    [SerializeField] private float wallRunForce;
    private float wallRunTimer;
    [SerializeField, Range(0, 5)] private float maxWallRunTimeSeconds;
    [SerializeField] private float wallRunJumpUpForce;
    [SerializeField] private float wallRunJumpSideForce;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    [SerializeField, Range(0, 1)] private float wallCheckDistance = 0.75f;
    [SerializeField] private float minWallJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting Wall Settings")]
    private bool exitingWall;
    [SerializeField, Range(0, 0.5f)] private float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity Settings")]
    [SerializeField] private bool useGravity;
    [SerializeField, Range(0, 1000)] private float gravityCounterForce;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();
        inputManager = GetComponent<InputManager2>();

    }

    private void OnEnable()
    {
        inputManager.OnJumpPressed += AttemptWallJump;
    }

    private void OnDisable()
    {
        inputManager.OnJumpPressed -= AttemptWallJump;
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    void FixedUpdate()
    {
        GetInput(inputManager.MoveInput);
        if (playerController.m_bIsWallRunning) WallRunningMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, wallLayer);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, wallLayer);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minWallJumpHeight, groundLayer);
    }

    private void GetInput(Vector2 input)
	{
		horizontalInput = input.x;
		verticalInput = input.y;
	}

    private void StateMachine()
    {
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
           StartWallRun();

           if(wallRunTimer > 0)
            {
                wallRunTimer -= Time.deltaTime;
            }

            if(wallRunTimer <= 0 && playerController.m_bIsWallRunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }
        }

        else if(exitingWall)
        {
            if (playerController.m_bIsWallRunning) StopWallRun();

            if(exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }
            if(exitWallTimer <= 0)
            {
                exitingWall = false;
            }
        }

        else
        {
            StopWallRun();
        }
    }

    private void StartWallRun()
    {
        playerController.m_bIsWallRunning = true;

        wallRunTimer = maxWallRunTimeSeconds;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    }

    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;
        //rb.useGravity = false;
        //rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        // Make sure the wall forward is always pointing in the same direction as the orientation forward
        if((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // Pushes the player into the wall so they don't lose contact unless attempting escape
        if(!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
        {
            rb.AddForce(-wallNormal * 10, ForceMode.Force);
        }

        // if(useGravity)
        // {
        //     rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        // }
    }

    private void StopWallRun()
    {
        rb.useGravity = true;
        playerController.m_bIsWallRunning = false;
    }

    private void AttemptWallJump()
    {
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround())
        {
            WallJump();
        }
    }

    private void WallJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallRunJumpUpForce + wallNormal * wallRunJumpSideForce;

        rb.linearVelocity = new UnityEngine.Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }

}
