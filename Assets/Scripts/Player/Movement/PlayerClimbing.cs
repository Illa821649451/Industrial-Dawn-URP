using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerClimbing : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementAdvanced pm;
    public Transform orientation;
    public Transform cam;
    public Rigidbody rb;

    [Header("Ledge Grabbing")]
    public float moveToLedgeSpeed;
    public float maxLedgeGrabDistance;

    public float minTimeOnLedge;
    private float timeOnLedge;

    public bool holding;

    [Header("Ledge Jumping")]
    public KeyCode jumpKey = KeyCode.Space;
    public float ledgeJumpForwardForce;
    public float ledgeJumpUpwardForce;

    [Header("Ledge Detection")]
    public float ledgeDetectionLength;
    public float ledgeSphereCastRadius;
    private Transform Ledge;
    [SerializeField] private LayerMask climablePipeLayer;
    private bool alrPressed = false;

    public LayerMask ledge;
    public bool detectedLedge;
    public LayerMask screw;
    public bool detectedScrew;
    public LayerMask wPipe;
    public bool detectedWPipe;
    public LayerMask climablePipe;
    public bool detectedClimablePipe;

    private Transform lastLedge;
    private Transform currLedge;

    private RaycastHit ledgeHit;

    public float moveSpeed;

    [Header("Exiting")]
    public bool exitingLedge;
    public float exitLedgeTime;
    private float exitLedgeTimer;

    private void Update()
    {
        LedgeDetection();
        SubStateMachine();
    }

    private void SubStateMachine()
    {
        if (holding)
        {
            FreezeRigidbodyOnLedge();
            if (detectedLedge)
            {
                float horizontalInput = Input.GetAxis("Horizontal");
                Vector3 move = transform.right * horizontalInput * moveSpeed * Time.deltaTime;
                rb.velocity = new Vector3(move.x, rb.velocity.y, rb.velocity.z);
            }
            if (detectedClimablePipe)
            {
                float verticaInput = Input.GetAxis("Vertical");
                Vector3 move = transform.forward * verticaInput * moveSpeed * Time.deltaTime;
                transform.Translate(move);
            }
            timeOnLedge += Time.deltaTime;

            if (Input.GetKeyDown(jumpKey) && pm.isMoving == false) LedgeJump();
        }
        else if (exitingLedge)
        {
            if (exitLedgeTimer > 0) exitLedgeTimer -= Time.deltaTime;
            else exitingLedge = false;
        }
    }

    private void LedgeDetection()
    {
        RaycastHit ledgeHitTemp;
        bool ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out ledgeHitTemp, ledgeDetectionLength, ledge);

        RaycastHit screwHitTemp;
        bool screwDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out screwHitTemp, ledgeDetectionLength, screw);

        RaycastHit wPipeHitTemp;
        bool wPipeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out wPipeHitTemp, ledgeDetectionLength, wPipe);

        if (!ledgeDetected && !screwDetected && !wPipeDetected) return;

        detectedLedge = ledgeDetected;
        detectedScrew = screwDetected;
        detectedWPipe = wPipeDetected;

        if (ledgeDetected) ledgeHit = ledgeHitTemp;
        else if (screwDetected) ledgeHit = screwHitTemp;
        else if (wPipeDetected) ledgeHit = wPipeHitTemp;

        if (ledgeHit.transform == null) return;

        float distanceToLedge = Vector3.Distance(transform.position, ledgeHit.transform.position);
        Ledge = ledgeHit.transform;

        if (ledgeHit.transform == lastLedge) return;

        if (distanceToLedge < maxLedgeGrabDistance && !holding) EnterLedgeHold();
    }

    private void LedgeJump()
    {
        ExitLedgeHold();

        Invoke(nameof(DelayedJumpForce), 0.05f);
    }

    private void DelayedJumpForce()
    {
        Vector3 forceToAdd = cam.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpUpwardForce;
        rb.velocity = Vector3.zero;
        rb.AddForce(forceToAdd, ForceMode.Impulse);
    }

    private void EnterLedgeHold()
    {
        holding = true;

        transform.SetParent(Ledge);

        pm.restricted = true;

        if(!alrPressed)
        {
            currLedge = ledgeHit.transform;
            lastLedge = ledgeHit.transform;
        }

        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }
    private void OnTriggerStay(Collider other)
    {
        if (((1 << other.gameObject.layer) & climablePipeLayer) != 0)
        {
            //ÒÓÒ ÍÀÄÀ ÂÑÒÀÂÈÒÈ ÀÊÒÈÂÀÖ²Þ ÏËÀØÊÈ Ò²ÏÀ ÍÀÆÌÈ E ØÎÁ ÍÀ×ÀÒÈ Ë²ÇÒÈ ÏÎ ÒÐÓÁ²
            if(Input.GetKey(KeyCode.E)&& !alrPressed)
            {
                alrPressed = true;
                EnterLedgeHold();
                detectedClimablePipe = true;
                currLedge = other.gameObject.transform;
            }
        }
    }
    private void FreezeRigidbodyOnLedge()
    {
        rb.useGravity = false;
        Vector3 playerPosition = transform.position;
        Vector3 localTargetPosition = Vector3.zero;
        Vector3 targetPosition = playerPosition;

        if (detectedLedge)
        {
            localTargetPosition = currLedge.InverseTransformPoint(playerPosition);
            localTargetPosition.y = 0f;
            localTargetPosition.z = 1f;
            targetPosition = currLedge.TransformPoint(localTargetPosition);
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
            transform.rotation = targetRotation;
            pm.onLedge = true;
        }
        else if (detectedScrew)
        {
            localTargetPosition = new Vector3(-0.06400144f, 0.8653333f, 0f);
            targetPosition = currLedge.TransformPoint(localTargetPosition);
        }
        else if (detectedWPipe)
        {
            localTargetPosition = new Vector3(-0.06400144f, 0.8653333f, 0f);
            targetPosition = currLedge.TransformPoint(localTargetPosition);
        }
        else if (detectedClimablePipe)
        {
            localTargetPosition = currLedge.InverseTransformPoint(playerPosition);
            localTargetPosition.x = -1.25f;
            localTargetPosition.z = 0f;
            targetPosition = currLedge.TransformPoint(localTargetPosition);
            transform.position = targetPosition;

            pm.onPipe = true;
        }

        Vector3 directionToLedge = targetPosition - transform.position;
        float distanceToLedge = directionToLedge.magnitude;

        if (distanceToLedge > 0.1f)
        {
            if (rb.velocity.magnitude < moveToLedgeSpeed)
            {
                rb.velocity = Vector3.zero;
                rb.AddForce(directionToLedge.normalized * moveToLedgeSpeed * 1000f * Time.deltaTime);
            }               
        }
        else
        {
            if (!pm.freeze) pm.freeze = true;
            if (pm.unlimited) pm.unlimited = false;
        }

        if (distanceToLedge > maxLedgeGrabDistance) ExitLedgeHold();
    }

    public void ExitLedgeHold()
    {
        alrPressed = false;

        pm.onLedge = false;
        pm.onPipe = false;

        pm.canMoveRight = true;
        pm.canMoveLeft = true;

        pm.canMoveUp = true;

        pm.isMoving = false;
        pm.pipeDown = false;

        detectedLedge = false;
        detectedScrew = false;
        detectedWPipe = false;
        detectedClimablePipe = false;

        transform.SetParent(null);
        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;

        holding = false;
        timeOnLedge = 0f;

        pm.restricted = false;
        pm.freeze = false;

        rb.useGravity = true;

        StopAllCoroutines();
        Invoke(nameof(ResetLastLedge), 1f);
    }

    private void ResetLastLedge()
    {
        lastLedge = null;
    }
}
