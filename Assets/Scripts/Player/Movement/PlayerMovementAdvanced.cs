using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementAdvanced : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    public bool isCrouching = false;

    [Header("Climbing")]
    public bool isMoving;
    public float climbSpeed;
    public float moveDistance;

    public bool pipeDown = false;
    public bool pipeUp = false;
    public bool OnPlace = false;
    
    public LayerMask requiredLayerLedge;
    public LayerMask requiredLayerPipe;
    public bool isInLedgeTrigger = false;
    public Transform ledgeObject = null;

    public float maxXPositionLeft = 1.5f;
    public float maxXPositionRight = -1.5f;
    public float maxYPositionDown;
    public float maxYPositionUp;

    PlayerClimbing pc;

    public bool canMoveLeft = true;
    public bool canMoveRight = true;

    public bool canMoveUp = true;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    public bool onLedge;
    public bool onPipe;

    [HideInInspector]public Vector3 moveDirection;

    Rigidbody rb;

    public bool isSprinting = false;

    public MovementState state;
    public enum MovementState
    {
        freeze,
        unlimited,
        walking,
        sprinting,
        crouching,
        sliding,
        air
    }

    public bool sliding;
    public bool climbing;

    public bool freeze;
    public bool unlimited;

    public bool restricted;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        pc = GetComponent<PlayerClimbing>();

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if (Input.GetKeyDown(sprintKey)) 
            isSprinting = !isSprinting;

        if(Input.GetKeyDown(crouchKey))
        {
            isCrouching = !isCrouching;
            isSprinting = false;
            if(isCrouching == true) 
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if (isCrouching == true)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z); 
        }
        else if (isCrouching == false)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        if (freeze)
        {
            state = MovementState.freeze;
            rb.velocity = Vector3.zero;
        }
        else if(unlimited)
        {
            state = MovementState.unlimited;
            moveSpeed = 999f;
            return;
        }
        if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;

            else
                desiredMoveSpeed = sprintSpeed;
        }
        else if (isCrouching == true)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else if(grounded && isSprinting == true && !isCrouching)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
        if(Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        if(onLedge)
        {
            if (Input.GetKey(KeyCode.D) && canMoveRight && !isMoving)
            {
                isMoving = true;
                Vector3 newPosition = transform.localPosition + new Vector3(-moveDistance, 0, 0);
                Vector3 WorldNewPosition = transform.parent.TransformPoint(newPosition);
                if (isInLedgeTrigger)
                {
                    if (transform.parent.CompareTag("LedgeRight"))
                    {
                        if (newPosition.x <= maxXPositionRight)
                        {
                            newPosition.x = maxXPositionRight;
                            WorldNewPosition = transform.parent.TransformPoint(newPosition);
                            canMoveRight = false;
                        }
                        else 
                            canMoveRight = true;
                    }
                    else
                        canMoveLeft = true;
                }
                StartCoroutine(MoveToPosition(WorldNewPosition, climbSpeed));                
            }
            if (Input.GetKey(KeyCode.A) && canMoveLeft && !isMoving)
            {
                isMoving = true;
                Vector3 newPosition = transform.localPosition + new Vector3(moveDistance, 0, 0);
                Vector3 WorldNewPosition = transform.parent.TransformPoint(newPosition);
                if (isInLedgeTrigger)
                {
                    if (transform.parent.CompareTag("LedgeLeft"))
                    {
                        if (newPosition.x >= maxXPositionLeft)
                        {
                            newPosition.x = maxXPositionLeft;
                            WorldNewPosition = transform.parent.TransformPoint(newPosition);
                            canMoveLeft = false;
                        }
                        else
                            canMoveLeft = true;
                    }
                    else
                        canMoveRight = true;
                }
                StartCoroutine(MoveToPosition(WorldNewPosition, climbSpeed));
            }

        }
        if (onPipe)
        {
            if (Input.GetKey(KeyCode.W) && isMoving == false)
            {
                isMoving = true;
                Vector3 newPosition = transform.position + new Vector3(0, moveDistance, 0);
                if (pipeUp && !pipeDown)
                {
                    if (newPosition.y >= maxYPositionUp)
                    {
                        if (OnPlace)
                        {
                            Vector3 localPos = new Vector3(0.25f, 4.5f, 0);
                            Vector3 worldPos = transform.parent.TransformPoint(localPos);
                            pc.ExitLedgeHold();
                            StartCoroutine(MoveToPosition(worldPos, climbSpeed));
                            OnPlace = false;
                            onPipe = false;
                            return;
                        }
                        Vector3 newLocalPos = new Vector3(0, maxYPositionUp, 0);
                        Vector3 newWorldPos = transform.parent.TransformPoint(newLocalPos);
                        newPosition.y = newWorldPos.y;
                        OnPlace = true;
                    }
                    else
                        pipeUp = false;
                }
                else
                    pipeUp = false;
                StartCoroutine(MoveToPosition(newPosition, climbSpeed));
            }

            if (Input.GetKey(KeyCode.S) && isMoving == false)
            {
                isMoving = true;
                Vector3 newPosition = transform.position + new Vector3(0, -moveDistance,0);
                canMoveUp = true;
                if (pipeDown)
                {
                    if (newPosition.y <= maxYPositionDown)
                    {
                        transform.SetParent(null);
                        pc.ExitLedgeHold();
                        return;
                    }
                    else 
                        pipeDown = false;
                }
                else
                    pipeDown = false;
                StartCoroutine(MoveToPosition(newPosition, climbSpeed));
            }
        }
        if (restricted) return;
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        rb.useGravity = !OnSlope();
    }
    private IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
    private void OnTriggerStay(Collider other)
    {
        if ((requiredLayerLedge.value & (1 << other.gameObject.layer)) > 0)
        {
            transform.SetParent(other.transform);
            ledgeObject = other.transform;

            if (other.CompareTag("LedgeLeft"))
            {
                isInLedgeTrigger = true;
                ledgeObject = other.transform;
            }
            else if (other.CompareTag("LedgeRight"))
            {
                isInLedgeTrigger = true;
                ledgeObject = other.transform;
            }
        }
        if ((requiredLayerPipe.value & (1 << other.gameObject.layer)) > 0)
        {
            transform.SetParent(other.transform);

            if (other.CompareTag("PipeDown"))
            {
                pipeDown = true;
            }
            else if (other.CompareTag("PipeUp"))
            {
                pipeUp = true;
            }
            else
            {
                pipeUp = false;
                pipeDown = false;
                OnPlace = false;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LedgeLeft") || other.CompareTag("LedgeRight"))
        {
            isInLedgeTrigger = false;
            ledgeObject = null;
        }
        if(other.CompareTag("PipeDown") || other.CompareTag("PipeUp"))
        {
            transform.SetParent(null);
            pipeDown = false;
            pipeUp = false;
        }
        
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}