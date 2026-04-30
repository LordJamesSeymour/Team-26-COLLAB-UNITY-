using UnityEngine;
using System.Collections;
using Group26.Player.Inputs;
using Unity.Mathematics;
using UnityEngine.Splines;
using System;

namespace Group26.Player.Movement
{
    public class PlayerControllerStyleTesting : MonoBehaviour
    {
        [Header("References")]
        private InputManager inputManager;

        [Header("Movement")]
        [SerializeField] float walkSpeed;
        [SerializeField] float slideSpeed;
        [SerializeField] float wallRunSpeed;
        [SerializeField] float dashSpeed;
        [SerializeField] float swingSpeed;
        [SerializeField] float dashSpeedChangeFactor;

        public float groundDrag;

        public float maxYSpeed;
        public float moveSpeed;
        float desiredMoveSpeed;
        float lastDesiredMoveSpeed;

        [SerializeField] float speedIncreaseMultiplier = 1f;
        [SerializeField] float slopeIncreaseMultiplier = 1f;

        [Header("Jumping")]
        [SerializeField] float jumpForce;
        [SerializeField] float jumpCooldown = 0.1f;
        [SerializeField] float airMultiplier = 0.4f;

        [Header("Jump Buffering")]
        [SerializeField] float jumpBufferTime = 0.15f;

        bool exitingSlope;

        [Header("Crouching")]
        [SerializeField] float crouchSpeed;
        float startYScale;
        float crouchYScale;

        [Header("Ground Check")]
        [SerializeField] float m_fGroundDistance = 0.15f;
        [SerializeField] Transform m_tGroundCheck;
        [SerializeField] LayerMask m_lGround;

        [Header("Slope Handling")]
        [SerializeField] float MaxSlopeAngle = 45f;
        RaycastHit slopeHit;

        [Header("Rail System")]
        [SerializeField] float railJumpUpForce = 6f;
        [SerializeField] float railExitForwardBoost = 2f;

        [Header("Straight Grapple")]
        [SerializeField] private float m_straightGrappleReleaseDistance = 1.0f;

        public MovementState state;
        public enum MovementState
        {
            freeze,
            walking,
            crouching,
            sliding,
            air,
            swinging,
            wallRunning,
            dashing,
            rail
        }

        public bool m_bActiveGrapple;
        public bool m_bActiveSwing;
        public bool m_bFreeze;
        public bool m_bSliding;
        public bool m_bDashing;
        private bool m_bDashMovementLocked = true;
        public bool m_bIsGrounded;
        public bool m_bIsWallRunning;
        public bool m_bOnRail;

        public bool IsOnRail => m_bOnRail;
        public bool IsGrounded => m_bIsGrounded;
        public bool IsSliding => m_bSliding;

        public bool IsMovementLockedForSlide =>
            m_bOnRail ||
            m_bActiveGrapple ||
            m_bActiveSwing ||
            m_bIsWallRunning ||
            m_bDashing ||
            m_bFreeze;

        [SerializeField] Transform orientation;
        public Transform OrientationTransform => orientation;

        float horizontalInput;
        float verticalInput;

        Vector3 moveDir;
        Rigidbody rb;

        Collider m_cPlayerCollider;

        float jumpBufferTimer;
        bool readyToJump = true;

        Sliding slidingComp;
        SlopeMomentum m_momentumScript;

        private MovementState lastState;
        private bool keepMomentum;
        private float speedChangeFactor = 1f;

        private bool enableMovementOnNextTouch;
        private Vector3 VelocityToSet;

        private bool m_bStraightGrappleMovement;
        private Vector3 m_straightGrappleTarget;
        private float m_straightGrappleSpeed;

        RailSpline currentRail;
        float currentRailT;
        float currentRailSpeed;

        public event Action TrickSystemEvent;

        public Vector3 SlopeNormal => slopeHit.normal;
        public Vector3 FlatVelocity => new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        public float CurrentHorizontalSpeed => FlatVelocity.magnitude;
        public Rigidbody Body => rb;

        private StyleSystem styleSystem;


        private void Awake()
        {
            inputManager = GetComponent<InputManager>();
            styleSystem = GetComponent<StyleSystem>();

            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            startYScale = transform.localScale.y;
            crouchYScale = startYScale / 2f;

            m_cPlayerCollider = GetComponentInChildren<Collider>();
            slidingComp = GetComponent<Sliding>();

            m_momentumScript = GetComponent<SlopeMomentum>();
            if (m_momentumScript == null)
                Debug.LogWarning("No SlopeMomentum script found on player.");
        }

        private void OnEnable()
        {
            inputManager.OnJumpPressed += Jump;
        }

        private void OnDisable()
        {
            inputManager.OnJumpPressed -= Jump;
        }

        private void FixedUpdate()
        {
            m_bIsGrounded = Physics.CheckSphere(m_tGroundCheck.position, m_fGroundDistance, m_lGround);

            GetInput(inputManager.MoveInput);

            if (m_bOnRail)
            {
                UpdateRailMovement(Time.fixedDeltaTime);
                return;
            }

            if (m_bActiveGrapple && m_bStraightGrappleMovement)
            {
                UpdateStraightGrappleMovement(Time.fixedDeltaTime);
                return;
            }

            if (m_bIsGrounded && !m_bActiveGrapple)
            {
                if (state == MovementState.walking || state == MovementState.crouching)
                    rb.linearDamping = groundDrag;
                else
                    rb.linearDamping = 0f;
            }
            else
            {
                rb.linearDamping = 0f;
            }

            bool onSlope = OnSlope();

            rb.useGravity = !(onSlope && !exitingSlope && !m_bSliding);

            StateHandler(onSlope);

            if (jumpBufferTimer > 0f)
                jumpBufferTimer -= Time.fixedDeltaTime;

            TryConsumeJumpBuffer();

            if (m_momentumScript != null)
                moveSpeed += m_momentumScript.m_momentum;

            if (!m_bSliding)
            {
                MovePlayer(onSlope);
                SpeedControl(onSlope);
            }
        }

        private void UpdateStraightGrappleMovement(float deltaTime)
        {
            if (!m_bActiveGrapple || !m_bStraightGrappleMovement)
                return;

            Vector3 toTarget = m_straightGrappleTarget - transform.position;
            float distanceToTarget = toTarget.magnitude;

            rb.useGravity = false;
            rb.linearDamping = 0f;
            rb.angularVelocity = Vector3.zero;

            if (distanceToTarget <= m_straightGrappleReleaseDistance)
            {
                ReleaseGrappleMovement();
                return;
            }

            float safeSpeed = Mathf.Max(m_straightGrappleSpeed, 0.01f);
            Vector3 desiredVelocity = toTarget.normalized * safeSpeed;

            rb.linearVelocity = desiredVelocity;
        }

        void StateHandler(bool onSlope)
        {
            if (m_bOnRail)
            {
                state = MovementState.rail;
                desiredMoveSpeed = 0f;
                moveSpeed = 0f;
                return;
            }

            if (m_bFreeze)
            {
                state = MovementState.freeze;
                moveSpeed = 0f;
                rb.linearVelocity = Vector3.zero;
            }
            else if (m_bDashing)
            {
                state = MovementState.dashing;
                desiredMoveSpeed = dashSpeed;
                speedChangeFactor = dashSpeedChangeFactor;
            }
            else if (m_bActiveSwing)
            {
                state = MovementState.swinging;
                moveSpeed = swingSpeed;
            }
            else if (m_bIsWallRunning)
            {
                styleSystem.AddStyleCombo(10, MovementState.wallRunning.ToString(), "Wall Run");
                state = MovementState.wallRunning;
                desiredMoveSpeed = wallRunSpeed;
            }
            else if (m_bSliding)
            {
                state = MovementState.sliding;
                desiredMoveSpeed = slideSpeed;
            }
            else if (m_bIsGrounded && inputManager.isCrouching)
            {
                state = MovementState.crouching;
                desiredMoveSpeed = crouchSpeed;
            }
            else if (m_bIsGrounded)
            {
                state = MovementState.walking;
                desiredMoveSpeed = walkSpeed;
            }
            else
            {
                state = MovementState.air;
            }

            if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0f)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }

            bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
            if (lastState == MovementState.dashing)
                keepMomentum = true;

            if (desiredMoveSpeedHasChanged)
            {
                if (keepMomentum)
                {
                    StopAllCoroutines();
                    StartCoroutine(SmoothlyLerpMoveSpeed());
                }
                else
                {
                    StopAllCoroutines();
                    moveSpeed = desiredMoveSpeed;
                }
            }

            lastDesiredMoveSpeed = desiredMoveSpeed;
            lastState = state;

            TrickSystemEvent?.Invoke();
        }

        private IEnumerator SmoothlyLerpMoveSpeed()
        {
            float time = 0f;
            float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
            float startValue = moveSpeed;

            while (time < difference)
            {
                moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

                time += Time.deltaTime;

                if (OnSlope())
                {
                    float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                    float slopeAngleIncrease = 1f + (slopeAngle / 90f);

                    time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
                }
                else
                {
                    time += Time.deltaTime * speedIncreaseMultiplier;
                }

                yield return null;
            }

            moveSpeed = desiredMoveSpeed;
            speedChangeFactor = 1f;
            keepMomentum = false;
        }

        public Vector3 CalculateJumpVelocity(Vector3 StartPoint, Vector3 EndPoint, float tracjectoryHeight)
        {
            float gravity = Physics.gravity.y;
            float displacementY = EndPoint.y - StartPoint.y;
            Vector3 displacementXZ = new Vector3(EndPoint.x - StartPoint.x, 0f, EndPoint.z - StartPoint.z);

            Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * tracjectoryHeight);
            Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * tracjectoryHeight / gravity)
                + Mathf.Sqrt(2 * (displacementY - tracjectoryHeight) / gravity));

            return velocityXZ + velocityY;
        }

        private void GetInput(Vector2 input)
        {
            horizontalInput = input.x;
            verticalInput = input.y;
        }

        private void MovePlayer(bool onSlope)
        {
            if (m_bActiveGrapple) return;
            if (m_bActiveSwing) return;
            if (m_bDashing && m_bDashMovementLocked) return;
            if (m_bOnRail) return;

            moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (onSlope && !exitingSlope)
            {
                rb.AddForce(GetSlopeMoveDirection(moveDir) * moveSpeed * 20f, ForceMode.Force);

                if (rb.linearVelocity.y > 0f)
                    rb.AddForce(Vector3.down * 40f, ForceMode.Force);

                return;
            }

            if (m_bIsGrounded)
                rb.AddForce(moveDir * moveSpeed * 10f, ForceMode.Force);
            else
                rb.AddForce(moveDir * moveSpeed * 10f * airMultiplier, ForceMode.Force);

            if (!m_bIsWallRunning)
                rb.useGravity = !OnSlope();
        }

        private void SpeedControl(bool onSlope)
        {
            if (m_bActiveGrapple) return;
            if (m_bSliding) return;
            if (m_bOnRail) return;

            if (onSlope && !exitingSlope)
            {
                if (rb.linearVelocity.magnitude > moveSpeed)
                    rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
            else
            {
                Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

                if (flatVel.magnitude > moveSpeed)
                {
                    Vector3 limitedVel = flatVel.normalized * moveSpeed;
                    rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
                }
            }

            if (maxYSpeed != 0 && rb.linearVelocity.y > maxYSpeed)
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);
        }

        public void Jump()
        {
            if (m_bOnRail)
            {
                ForceExitRail(true);
                return;
            }

            jumpBufferTimer = jumpBufferTime;
            TryConsumeJumpBuffer();
        }

        private void TryConsumeJumpBuffer()
        {
            if (!readyToJump) return;
            if (jumpBufferTimer <= 0f) return;
            if (!m_bIsGrounded) return;
            if (m_bOnRail) return;

            ExecuteJump();

            jumpBufferTimer = 0f;
            readyToJump = false;
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        private void ExecuteJump()
        {
            if (m_bSliding && slidingComp != null)
                slidingComp.ForceEndSlide();

            exitingSlope = true;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        private void ResetJump()
        {
            readyToJump = true;
            exitingSlope = false;
        }

        public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
        {
            if (m_bOnRail) return;

            if (m_bSliding && slidingComp != null)
                slidingComp.ForceEndSlide();

            m_bActiveGrapple = true;
            m_bStraightGrappleMovement = false;

            VelocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
            Invoke(nameof(SetVelocity), 0.1f);
        }

        public void GrappleToPositionStraight(Vector3 targetPosition, float grappleSpeed)
        {
            if (m_bOnRail) return;

            if (m_bSliding && slidingComp != null)
                slidingComp.ForceEndSlide();

            m_bActiveGrapple = true;
            m_bStraightGrappleMovement = true;
            enableMovementOnNextTouch = true;

            m_straightGrappleTarget = targetPosition;
            m_straightGrappleSpeed = Mathf.Max(grappleSpeed, 0.01f);

            rb.angularVelocity = Vector3.zero;
        }

        private void SetVelocity()
        {
            enableMovementOnNextTouch = true;
            rb.linearVelocity = VelocityToSet;
        }

        public void ResetRestrictions()
        {
            if (m_bOnRail) return;

            m_bActiveGrapple = false;
            m_bStraightGrappleMovement = false;
            enableMovementOnNextTouch = false;
            m_straightGrappleSpeed = 0f;
            m_straightGrappleTarget = Vector3.zero;
        }

        public void BeginDashState(float dashMaxYSpeed, bool lockMovement = true)
        {
            if (m_bSliding && slidingComp != null)
                slidingComp.ForceEndSlide();

            m_bDashing = true;
            m_bDashMovementLocked = lockMovement;
            maxYSpeed = dashMaxYSpeed;
        }

        public void ReleaseDashMovementLock()
        {
            if (!m_bDashing)
                return;

            m_bDashMovementLocked = false;
        }

        public void EndDashState()
        {
            m_bDashing = false;
            m_bDashMovementLocked = true;
            maxYSpeed = 0f;
        }

        public void SetSliding(bool value)
        {
            m_bSliding = value;
        }

        public void ReleaseGrappleMovement()
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GrappleGun grapple = GetComponent<GrappleGun>();
            if (grapple != null)
                grapple.ForceStopGrapple();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (enableMovementOnNextTouch && m_bActiveGrapple)
            {
                ReleaseGrappleMovement();
            }
        }

        public bool OnSlope()
        {
            if (m_cPlayerCollider == null) return false;

            float halfHeight = m_cPlayerCollider.bounds.extents.y;

            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, halfHeight + 0.35f, m_lGround))
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return angle < MaxSlopeAngle && angle > 0f;
            }
            return false;
        }

        public float GetSlopeAngle()
        {
            if (m_cPlayerCollider == null) return 0f;

            float halfHeight = m_cPlayerCollider.bounds.extents.y;

            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, halfHeight + 0.35f, m_lGround))
            {
                return Vector3.Angle(Vector3.up, slopeHit.normal);
            }
            return 0f;
        }

        public Vector3 GetSlopeMoveDirection(Vector3 direction)
        {
            return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
        }

        public Vector3 GetDirection()
        {
            return moveDir;
        }

        public void EnterRail(RailSpline rail)
        {
            if (rail == null || rail.SplineContainer == null)
                return;

            Spline spline = rail.SplineContainer.Spline;
            if (spline == null || spline.Count < 2)
                return;

            if (m_bOnRail && currentRail == rail)
                return;

            Vector3 incomingVelocity = rb.linearVelocity;

            if (m_bSliding && slidingComp != null)
                slidingComp.ForceEndSlide();

            if (m_bSliding && slidingComp != null)
                slidingComp.ForceEndSlide();

            inputManager?.ClearRailBlockedInputs();

            StopAllCoroutines();

            m_bActiveGrapple = false;
            m_bStraightGrappleMovement = false;
            m_bActiveSwing = false;
            m_bSliding = false;
            m_bIsWallRunning = false;
            m_bFreeze = false;
            exitingSlope = false;
            EndDashState();

            moveSpeed = 0f;
            desiredMoveSpeed = 0f;
            jumpBufferTimer = 0f;

            currentRail = rail;
            m_bOnRail = true;
            state = MovementState.rail;

            rb.useGravity = false;
            rb.linearDamping = 0f;
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;

            Vector3 localPlayerPos = rail.SplineContainer.transform.InverseTransformPoint(transform.position);

            SplineUtility.GetNearestPoint(
                spline,
                (float3)localPlayerPos,
                out float3 nearestLocal,
                out float nearestT,
                rail.NearestPointResolution,
                rail.NearestPointIterations);

            currentRailT = nearestT;
            currentRailSpeed = Mathf.Clamp(
                Mathf.Max(rail.EntrySpeed, incomingVelocity.magnitude),
                rail.MinSpeed,
                rail.MaxSpeed);

            SnapToRail();
        }

        private void SnapToRail()
        {
            if (!m_bOnRail || currentRail == null)
                return;

            Vector3 worldPos = currentRail.SplineContainer.EvaluatePosition(currentRailT);
            Vector3 up = currentRail.UseSplineUp
                ? ((Vector3)currentRail.SplineContainer.EvaluateUpVector(currentRailT)).normalized
                : Vector3.up;

            rb.MovePosition(worldPos + up * currentRail.RideOffset);
        }

        private void UpdateRailMovement(float deltaTime)
        {
            if (!m_bOnRail || currentRail == null)
                return;

            inputManager?.ClearRailBlockedInputs();

            m_bActiveGrapple = false;
            m_bStraightGrappleMovement = false;
            m_bActiveSwing = false;
            m_bSliding = false;
            m_bIsWallRunning = false;
            m_bFreeze = false;
            EndDashState();

            state = MovementState.rail;
            rb.useGravity = false;
            rb.linearDamping = 0f;
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;

            float railInput = Mathf.Clamp(inputManager.MoveInput.y, -1f, 1f);

            if (railInput > 0.01f)
            {
                currentRailSpeed += railInput * currentRail.Acceleration * deltaTime;
            }
            else if (railInput < -0.01f)
            {
                currentRailSpeed -= (-railInput) * currentRail.BrakeDeceleration * deltaTime;
            }
            else
            {
                currentRailSpeed -= currentRail.PassiveDeceleration * deltaTime;
            }

            currentRailSpeed = Mathf.Clamp(currentRailSpeed, currentRail.MinSpeed, currentRail.MaxSpeed);

            Spline spline = currentRail.SplineContainer.Spline;

            float3 localPoint = SplineUtility.GetPointAtLinearDistance(
                spline,
                currentRailT,
                currentRailSpeed * deltaTime,
                out float newT);

            currentRailT = Mathf.Clamp01(newT);

            Vector3 worldPos = currentRail.SplineContainer.transform.TransformPoint((Vector3)localPoint);
            Vector3 up = currentRail.UseSplineUp
                ? ((Vector3)currentRail.SplineContainer.EvaluateUpVector(currentRailT)).normalized
                : Vector3.up;

            rb.MovePosition(worldPos + up * currentRail.RideOffset);

            if (currentRail.AutoExitAtEnd && currentRailT >= 0.999f)
            {
                ForceExitRail(false);
            }
        }

        public void ForceExitRail(bool jumpedOff)
        {
            if (!m_bOnRail)
                return;

            Vector3 exitDirection = transform.forward;

            if (currentRail != null && currentRail.SplineContainer != null)
            {
                Vector3 tangent = ((Vector3)currentRail.SplineContainer.EvaluateTangent(currentRailT)).normalized;
                tangent.y = 0f;

                if (tangent.sqrMagnitude > 0.0001f)
                    exitDirection = tangent.normalized;
            }

            m_bOnRail = false;
            currentRail = null;
            currentRailT = 0f;
            currentRailSpeed = 0f;

            m_bActiveGrapple = false;
            m_bStraightGrappleMovement = false;
            m_bActiveSwing = false;
            m_bSliding = false;
            m_bIsWallRunning = false;
            m_bFreeze = false;
            exitingSlope = false;
            EndDashState();

            desiredMoveSpeed = 0f;
            moveSpeed = 0f;
            jumpBufferTimer = 0f;

            rb.useGravity = true;
            rb.linearDamping = 0f;
            rb.angularVelocity = Vector3.zero;

            Vector3 exitVelocity = exitDirection * railExitForwardBoost;

            if (jumpedOff)
                exitVelocity += Vector3.up * railJumpUpForce;

            rb.linearVelocity = exitVelocity;
            state = MovementState.air;

            inputManager?.ClearRailBlockedInputs();
        }
    }
}